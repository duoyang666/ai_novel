<script>
	import { io } from 'socket.io-client';
	import { spring } from 'svelte/motion';
	let loadingProgress = spring(0, {
		stiffness: 0.05
	});

	import { onMount, tick, setContext } from 'svelte';
	import {
		config,
		user,
		theme,
		WEBUI_NAME,
		mobile,
		socket,
		activeUserCount,
		USAGE_POOL
	} from '$lib/stores';
	import { goto } from '$app/navigation';
	import { page } from '$app/stores';
	import { Toaster, toast } from 'svelte-sonner';

	import { getBackendConfig } from '$lib/apis';
	import { getSessionUser } from '$lib/apis/auths';

	import '../tailwind.css';
	import '../app.css';

	import 'tippy.js/dist/tippy.css';

	import { WEBUI_OBJ, NetSettings } from '$lib/constants';
	import { get_settings } from '$lib/apis/net';
	import i18n, { initI18n, getLanguages } from '$lib/i18n';
	import { bestMatchingLanguage } from '$lib/utils';

	setContext('i18n', i18n);

	let loaded = false;
	const BREAKPOINT = 768;

	let wakeLock = null;

	onMount(async () => {
		window['WEBUI_OBJ'] = WEBUI_OBJ;
		const loadUrl = (host) => {
			// 正则表达式，用于匹配包含端口的网站地址
			const regex = /^(https?:\/\/)?(localhost(?::\d+)?|\w+(?:\.\w+)+)(?::\d+)?(\/[\w-./?%&=]*)?$/;
			const regexh = /^(https?:\/\/)localhost(?::(\d+))?(\/[\w-./?%&=]*)?$/;

			let host2 = `http://${host}`;
			if (regexh.test(host)) {
				host2 = host;
				host = host.replace('http://', '').replace('https://', '');
			}
			console.log(host2);

			if (regexh.test(host2)) {
				WEBUI_OBJ.WEBUI2_HOSTNAME = host;
				WEBUI_OBJ.WEBUI2_BASE_URL = host2;
				let url = WEBUI_OBJ.WEBUI2_BASE_URL;
				WEBUI_OBJ.WEBUI2_API_BASE_URL = `${url}/api/v1`;
				WEBUI_OBJ.OLLAMA2_API_BASE_URL = `${url}/ollama`;
				WEBUI_OBJ.OPENAI2_API_BASE_URL = `${url}/openai`;
				WEBUI_OBJ.AUDIO2_API_BASE_URL = `${url}/audio/api/v1`;
				WEBUI_OBJ.IMAGES2_API_BASE_URL = `${url}/images/api/v1`;
				WEBUI_OBJ.RAG2_API_BASE_URL = `${url}/rag/api/v1`;
				console.log(WEBUI_OBJ);
			}
		};
		const get_url = async () => {
			let res = await get_settings('WebUi');
			console.log(res);
			if (res) {
				res = JSON.parse(res);
				loadUrl(res.WEBUI2_HOSTNAME);
			}
		};
		await get_url();

		theme.set(localStorage.theme);

		mobile.set(window.innerWidth < BREAKPOINT);
		const onResize = () => {
			if (window.innerWidth < BREAKPOINT) {
				mobile.set(true);
			} else {
				mobile.set(false);
			}
		};

		window.addEventListener('resize', onResize);

		const setWakeLock = async () => {
			try {
				wakeLock = await navigator.wakeLock.request('screen');
			} catch (err) {
				// The Wake Lock request has failed - usually system related, such as battery.
				console.log(err);
			}

			if (wakeLock) {
				// Add a listener to release the wake lock when the page is unloaded
				wakeLock.addEventListener('release', () => {
					// the wake lock has been released
					console.log('Wake Lock released');
				});
			}
		};

		if ('wakeLock' in navigator) {
			await setWakeLock();

			document.addEventListener('visibilitychange', async () => {
				// Re-request the wake lock if the document becomes visible
				if (wakeLock !== null && document.visibilityState === 'visible') {
					await setWakeLock();
				}
			});
		}

		let backendConfig = null;
		try {
			backendConfig = await getBackendConfig();
			console.log('Backend config:', backendConfig);
		} catch (error) {
			console.error('Error loading backend config:', error);
		}
		// Initialize i18n even if we didn't get a backend config,
		// so `/error` can show something that's not `undefined`.

		initI18n();
		if (!localStorage.locale) {
			const languages = await getLanguages();
			const browserLanguages = navigator.languages
				? navigator.languages
				: [navigator.language || navigator.userLanguage];
			const lang = backendConfig.default_locale
				? backendConfig.default_locale
				: bestMatchingLanguage(languages, browserLanguages, 'en-US');
			$i18n.changeLanguage(lang);
		}

		if (backendConfig) {
			// Save Backend Status to Store
			await config.set(backendConfig);
			await WEBUI_NAME.set(backendConfig.name);

			if ($config) {
				const _socket = io(`${WEBUI_OBJ.WEBUI2_BASE_URL}`, {
					path: '/ws/socket.io',
					auth: { token: localStorage.token }
				});

				_socket.on('connect', () => {
					console.log('connected');
				});

				await socket.set(_socket);

				_socket.on('user-count', (data) => {
					console.log('user-count', data);
					activeUserCount.set(data.count);
				});

				_socket.on('usage', (data) => {
					//console.log('usage', data);
					if (!$NetSettings['net_service']) USAGE_POOL.set(data['models']);
				});

				if (localStorage.token) {
					// Get Session User Info
					const sessionUser = await getSessionUser(localStorage.token).catch((error) => {
						toast.error(error);
						return null;
					});

					if (sessionUser) {
						// Save Session User to Store
						await user.set(sessionUser);
					} else {
						// Redirect Invalid Session User to /auth Page
						localStorage.removeItem('token');
						await goto('/auth');
					}
				} else {
					// Don't redirect if we're already on the auth page
					// Needed because we pass in tokens from OAuth logins via URL fragments
					if ($page.url.pathname !== '/auth') {
						await goto('/auth');
					}
				}
			}
		} else {
			// Redirect to /error when Backend Not Detected
			await goto(`/error`);
		}

		await tick();

		if (
			document.documentElement.classList.contains('her') &&
			document.getElementById('progress-bar')
		) {
			loadingProgress.subscribe((value) => {
				const progressBar = document.getElementById('progress-bar');

				if (progressBar) {
					progressBar.style.width = `${value}%`;
				}
			});

			await loadingProgress.set(100);

			document.getElementById('splash-screen')?.remove();

			const audio = new Audio(`/audio/greeting.mp3`);
			const playAudio = () => {
				audio.play();
				document.removeEventListener('click', playAudio);
			};

			document.addEventListener('click', playAudio);

			loaded = true;
		} else {
			document.getElementById('splash-screen')?.remove();
			loaded = true;
		}

		return () => {
			window.removeEventListener('resize', onResize);
		};
	});
</script>

<svelte:head>
	<title>{$WEBUI_NAME}</title>
	<link crossorigin="anonymous" rel="icon" href="{WEBUI_OBJ.WEBUI2_BASE_URL}/static/favicon.png" />

	<!-- rosepine themes have been disabled as it's not up to date with our latest version. -->
	<!-- feel free to make a PR to fix if anyone wants to see it return -->
	<!-- <link rel="stylesheet" type="text/css" href="/themes/rosepine.css" />
	<link rel="stylesheet" type="text/css" href="/themes/rosepine-dawn.css" /> -->
</svelte:head>

<!-- {$APP_NAME} -->

{#if loaded}
	<slot />
{/if}

<Toaster richColors position="top-center" />
