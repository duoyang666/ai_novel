<!-- 设置-外部连接 -->

<script lang="ts">
	import { models, user } from '$lib/stores';
	import { createEventDispatcher, onMount, getContext, tick } from 'svelte';

	const dispatch = createEventDispatcher();

	import {
		getOllamaConfig,
		getOllamaUrls,
		getOllamaVersion,
		updateOllamaConfig,
		updateOllamaUrls
	} from '$lib/apis/ollama';
	import {
		getOpenAIConfig,
		getOpenAIKeys,
		getOpenAIModels,
		getOpenAIUrls,
		updateOpenAIConfig,
		updateOpenAIKeys,
		updateOpenAIUrls
	} from '$lib/apis/openai';
	import { toast } from 'svelte-sonner';
	import Switch from '$lib/components/common/Switch.svelte';
	import Spinner from '$lib/components/common/Spinner.svelte';
	import Tooltip from '$lib/components/common/Tooltip.svelte';
	import { NetSettings, WEBUI_OBJ, APP_NAME } from '$lib/constants';
	import { get_settings, set_settings, is_connect } from '$lib/apis/net';
	import { getModels as _getModels } from '$lib/apis';
	import SensitiveInput from '$lib/components/common/SensitiveInput.svelte';

	const i18n = getContext('i18n');

	const getModels = async () => {
		const models = await _getModels(localStorage.token);
		return models;
	};

	// External
	let OLLAMA_BASE_URLS = [''];
	let NET_BASE_URLS = [''];
	let NET_BASE_URL = '';

	let OPENAI_API_KEYS = [''];
	let OPENAI_API_BASE_URLS = [''];

	let pipelineUrls = {};

	let ENABLE_OPENAI_API = null;
	let ENABLE_OLLAMA_API = null;

	let NET_OBJ = {
		APP_NAME: $APP_NAME
	};

	const verifyOpenAIHandler = async (idx) => {
		OPENAI_API_BASE_URLS = OPENAI_API_BASE_URLS.map((url) => url.replace(/\/$/, ''));

		OPENAI_API_BASE_URLS = await updateOpenAIUrls(localStorage.token, OPENAI_API_BASE_URLS);
		OPENAI_API_KEYS = await updateOpenAIKeys(localStorage.token, OPENAI_API_KEYS);

		const res = await getOpenAIModels(localStorage.token, idx).catch((error) => {
			toast.error(error);
			return null;
		});

		if (res) {
			toast.success($i18n.t('Server connection verified'));
			if (res.pipelines) {
				pipelineUrls[OPENAI_API_BASE_URLS[idx]] = true;
			}
		}

		await models.set(await getModels());
	};

	const verifyOllamaHandler = async (idx) => {
		OLLAMA_BASE_URLS = OLLAMA_BASE_URLS.filter((url) => url !== '').map((url) =>
			url.replace(/\/$/, '')
		);

		OLLAMA_BASE_URLS = await updateOllamaUrls(localStorage.token, OLLAMA_BASE_URLS);

		const res = await getOllamaVersion(localStorage.token, idx).catch((error) => {
			toast.error(error);
			return null;
		});

		if (res) {
			toast.success($i18n.t('Server connection verified'));
		}

		await models.set(await getModels());
	};

	const updateOpenAIHandler = async () => {
		OPENAI_API_BASE_URLS = OPENAI_API_BASE_URLS.map((url) => url.replace(/\/$/, ''));

		// Check if API KEYS length is same than API URLS length
		if (OPENAI_API_KEYS.length !== OPENAI_API_BASE_URLS.length) {
			// if there are more keys than urls, remove the extra keys
			if (OPENAI_API_KEYS.length > OPENAI_API_BASE_URLS.length) {
				OPENAI_API_KEYS = OPENAI_API_KEYS.slice(0, OPENAI_API_BASE_URLS.length);
			}

			// if there are more urls than keys, add empty keys
			if (OPENAI_API_KEYS.length < OPENAI_API_BASE_URLS.length) {
				const diff = OPENAI_API_BASE_URLS.length - OPENAI_API_KEYS.length;
				for (let i = 0; i < diff; i++) {
					OPENAI_API_KEYS.push('');
				}
			}
		}

		OPENAI_API_BASE_URLS = await updateOpenAIUrls(localStorage.token, OPENAI_API_BASE_URLS);
		OPENAI_API_KEYS = await updateOpenAIKeys(localStorage.token, OPENAI_API_KEYS);
		await models.set(await getModels());
	};

	const updateOllamaUrlsHandler = async () => {
		OLLAMA_BASE_URLS = OLLAMA_BASE_URLS.filter((url) => url !== '').map((url) =>
			url.replace(/\/$/, '')
		);

		console.log(OLLAMA_BASE_URLS);

		if (OLLAMA_BASE_URLS.length === 0) {
			ENABLE_OLLAMA_API = false;
			await updateOllamaConfig(localStorage.token, ENABLE_OLLAMA_API);

			toast.info($i18n.t('Ollama API disabled'));
		} else {
			OLLAMA_BASE_URLS = await updateOllamaUrls(localStorage.token, OLLAMA_BASE_URLS);

			const ollamaVersion = await getOllamaVersion(localStorage.token).catch((error) => {
				toast.error(error);
				return null;
			});

			if (ollamaVersion) {
				toast.success($i18n.t('Server connection verified'));
				await models.set(await getModels());
			}
		}
	};

	onMount(async () => {
		try {
			let host = WEBUI_OBJ.WEBUI2_HOSTNAME;
			let res = await get_settings('WebUi');
			console.log(res);
			if (res) {
				res = JSON.parse(res);
				loadUrl(res.WEBUI2_HOSTNAME);
			}

			await connections();
		} catch (error) {}
	});

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

	const connections = async () => {
		if ($user.role === 'admin') {
			await Promise.all([
				(async () => {
					OLLAMA_BASE_URLS = await getOllamaUrls(localStorage.token);
				})(),
				(async () => {
					OPENAI_API_BASE_URLS = await getOpenAIUrls(localStorage.token);
				})(),
				(async () => {
					OPENAI_API_KEYS = await getOpenAIKeys(localStorage.token);
				})()
			]);

			try {
				OPENAI_API_BASE_URLS.forEach(async (url, idx) => {
					const res = await getOpenAIModels(localStorage.token, idx);
					if (res.pipelines) {
						pipelineUrls[url] = true;
					}
				});
			} catch {}

			const ollamaConfig = await getOllamaConfig(localStorage.token);
			const openaiConfig = await getOpenAIConfig(localStorage.token);

			ENABLE_OPENAI_API = openaiConfig.ENABLE_OPENAI_API;
			ENABLE_OLLAMA_API = ollamaConfig.ENABLE_OLLAMA_API;
		}
	};
</script>

<form
	class="flex flex-col h-full justify-between text-sm"
	on:submit|preventDefault={async () => {
		updateOpenAIHandler();
		updateOllamaUrlsHandler();
		dispatch('save');
	}}
>
	<div class="space-y-3 pr-1.5 overflow-y-scroll h-[30rem] max-h-[30rem]">
		<div class="space-y-3">
			<div class="flex justify-between items-center text-sm">
				<div class="  font-medium">{$i18n.t('WebUI服务地址')}</div>
			</div>
			<div class="flex w-full gap-1.5">
				<div class="flex-1 flex flex-col gap-2">
					<div class="flex w-full gap-2">
						<div class="flex-1 relative">
							<input
								class="w-full rounded-lg py-2 px-4 text-sm dark:text-gray-300 dark:bg-gray-850 outline-none"
								placeholder={$i18n.t('基础地址')}
								bind:value={WEBUI_OBJ.WEBUI2_HOSTNAME}
								on:change={(e) => {
									loadUrl(e.target.value);
								}}
							/>
						</div>
						<div class="flex">
							<Tooltip content={$i18n.t('保存地址')} className="self-start mt-0.5">
								<button
									class="self-center p-2 bg-gray-200 hover:bg-gray-300 dark:bg-gray-900 dark:hover:bg-gray-850 rounded-lg transition"
									on:click={async () => {
										let values = { WEBUI2_HOSTNAME: WEBUI_OBJ.WEBUI2_HOSTNAME };
										let res = await set_settings({
											ClassName: 'WebUi',
											JsonData: JSON.stringify(values)
										});
										if (res) {
											toast.success('地址保存成功，建议重启openwebui服务端，请保持地址一致');
										}
										loadUrl(WEBUI_OBJ.WEBUI2_HOSTNAME);
										await connections();
									}}
									type="button"
								>
									<svg
										xmlns="http://www.w3.org/2000/svg"
										viewBox="0 0 20 20"
										fill="currentColor"
										class="w-4 h-4"
									>
										<path
											fill-rule="evenodd"
											d="M15.312 11.424a5.5 5.5 0 01-9.201 2.466l-.312-.311h2.433a.75.75 0 000-1.5H3.989a.75.75 0 00-.75.75v4.242a.75.75 0 001.5 0v-2.43l.31.31a7 7 0 0011.712-3.138.75.75 0 00-1.449-.39zm1.23-3.723a.75.75 0 00.219-.53V2.929a.75.75 0 00-1.5 0V5.36l-.31-.31A7 7 0 003.239 8.188a.75.75 0 101.448.389A5.5 5.5 0 0113.89 6.11l.311.31h-2.432a.75.75 0 000 1.5h4.243a.75.75 0 00.53-.219z"
											clip-rule="evenodd"
										/>
									</svg>
								</button>
							</Tooltip>
						</div>
					</div>
				</div>
			</div>
		</div>
		<hr class=" dark:border-gray-700" />

		<!-- <div class="space-y-3">
			<div class="flex justify-between items-center text-sm">
				<div class="  font-medium">{$i18n.t('扩展服务地址')}</div>
			</div>
			<div class="flex w-full gap-1.5">
				<div class="flex-1 flex flex-col gap-2">
					<div class="flex w-full gap-2">
						<div class="flex-1 relative">
							<input
								class="w-full rounded-lg py-2 px-4 text-sm dark:text-gray-300 dark:bg-gray-850 outline-none"
								placeholder={$i18n.t('扩展服务地址')}
								bind:value={$NetSettings['net_host']}
								on:change={(e) => {
									$NetSettings['net_host'] = e.target.value;
								}}
							/>
						</div>
						<div class="flex">
							<Tooltip content={$i18n.t('判断服务是否可用')} className="self-start mt-0.5">
								<button
									class="self-center p-2 bg-gray-200 hover:bg-gray-300 dark:bg-gray-900 dark:hover:bg-gray-850 rounded-lg transition"
									on:click={async () => {
										let res = await is_connect();
										console.log(res);
										if (res) {
											$NetSettings['net_service_is_true'] = true;
											toast.success($i18n.t('Server connection verified'));
										} else {
											$NetSettings['net_service_is_true'] = false;
											toast.error($i18n.t('Error'));
										}
									}}
									type="button"
								>
									<svg
										xmlns="http://www.w3.org/2000/svg"
										viewBox="0 0 20 20"
										fill="currentColor"
										class="w-4 h-4"
									>
										<path
											fill-rule="evenodd"
											d="M15.312 11.424a5.5 5.5 0 01-9.201 2.466l-.312-.311h2.433a.75.75 0 000-1.5H3.989a.75.75 0 00-.75.75v4.242a.75.75 0 001.5 0v-2.43l.31.31a7 7 0 0011.712-3.138.75.75 0 00-1.449-.39zm1.23-3.723a.75.75 0 00.219-.53V2.929a.75.75 0 00-1.5 0V5.36l-.31-.31A7 7 0 003.239 8.188a.75.75 0 101.448.389A5.5 5.5 0 0113.89 6.11l.311.31h-2.432a.75.75 0 000 1.5h4.243a.75.75 0 00.53-.219z"
											clip-rule="evenodd"
										/>
									</svg>
								</button>
							</Tooltip>
						</div>
					</div>
				</div>
			</div>
		</div>
		<hr class=" dark:border-gray-700" /> -->

		{#if ENABLE_OPENAI_API !== null && ENABLE_OLLAMA_API !== null}
			<!-- OpenAI API -->
			<div class="space-y-3">
				<div class="mt-2 space-y-2 pr-1.5">
					<div class="flex justify-between items-center text-sm">
						<div class="  font-medium">{$i18n.t('OpenAI API')}</div>

						<div class="mt-1">
							<Switch
								bind:state={ENABLE_OPENAI_API}
								on:change={async () => {
									updateOpenAIConfig(localStorage.token, ENABLE_OPENAI_API);
								}}
							/>
						</div>
					</div>

					{#if ENABLE_OPENAI_API}
						<div class="flex flex-col gap-1">
							{#each OPENAI_API_BASE_URLS as url, idx}
								<div class="flex w-full gap-2">
									<div class="flex-1 relative">
										<input
											class="w-full rounded-lg py-2 px-4 {pipelineUrls[url]
												? 'pr-8'
												: ''} text-sm dark:text-gray-300 dark:bg-gray-850 outline-none"
											placeholder={$i18n.t('API Base URL')}
											bind:value={url}
											autocomplete="off"
										/>

										{#if pipelineUrls[url]}
											<div class=" absolute top-2.5 right-2.5">
												<Tooltip content="Pipelines">
													<svg
														xmlns="http://www.w3.org/2000/svg"
														viewBox="0 0 24 24"
														fill="currentColor"
														class="size-4"
													>
														<path
															d="M11.644 1.59a.75.75 0 0 1 .712 0l9.75 5.25a.75.75 0 0 1 0 1.32l-9.75 5.25a.75.75 0 0 1-.712 0l-9.75-5.25a.75.75 0 0 1 0-1.32l9.75-5.25Z"
														/>
														<path
															d="m3.265 10.602 7.668 4.129a2.25 2.25 0 0 0 2.134 0l7.668-4.13 1.37.739a.75.75 0 0 1 0 1.32l-9.75 5.25a.75.75 0 0 1-.71 0l-9.75-5.25a.75.75 0 0 1 0-1.32l1.37-.738Z"
														/>
														<path
															d="m10.933 19.231-7.668-4.13-1.37.739a.75.75 0 0 0 0 1.32l9.75 5.25c.221.12.489.12.71 0l9.75-5.25a.75.75 0 0 0 0-1.32l-1.37-.738-7.668 4.13a2.25 2.25 0 0 1-2.134-.001Z"
														/>
													</svg>
												</Tooltip>
											</div>
										{/if}
									</div>

									<SensitiveInput
										placeholder={$i18n.t('API Key')}
										bind:value={OPENAI_API_KEYS[idx]}
									/>
									<div class="self-center flex items-center">
										{#if idx === 0}
											<button
												class="px-1"
												on:click={() => {
													OPENAI_API_BASE_URLS = [...OPENAI_API_BASE_URLS, ''];
													OPENAI_API_KEYS = [...OPENAI_API_KEYS, ''];
												}}
												type="button"
											>
												<svg
													xmlns="http://www.w3.org/2000/svg"
													viewBox="0 0 16 16"
													fill="currentColor"
													class="w-4 h-4"
												>
													<path
														d="M8.75 3.75a.75.75 0 0 0-1.5 0v3.5h-3.5a.75.75 0 0 0 0 1.5h3.5v3.5a.75.75 0 0 0 1.5 0v-3.5h3.5a.75.75 0 0 0 0-1.5h-3.5v-3.5Z"
													/>
												</svg>
											</button>
										{:else}
											<button
												class="px-1"
												on:click={() => {
													OPENAI_API_BASE_URLS = OPENAI_API_BASE_URLS.filter(
														(url, urlIdx) => idx !== urlIdx
													);
													OPENAI_API_KEYS = OPENAI_API_KEYS.filter((key, keyIdx) => idx !== keyIdx);
												}}
												type="button"
											>
												<svg
													xmlns="http://www.w3.org/2000/svg"
													viewBox="0 0 16 16"
													fill="currentColor"
													class="w-4 h-4"
												>
													<path d="M3.75 7.25a.75.75 0 0 0 0 1.5h8.5a.75.75 0 0 0 0-1.5h-8.5Z" />
												</svg>
											</button>
										{/if}
									</div>

									<div class="flex">
										<Tooltip content={$i18n.t('连接验证')} className="self-start mt-0.5">
											<button
												class="self-center p-2 bg-gray-200 hover:bg-gray-300 dark:bg-gray-900 dark:hover:bg-gray-850 rounded-lg transition"
												on:click={() => {
													verifyOpenAIHandler(idx);
												}}
												type="button"
											>
												<svg
													xmlns="http://www.w3.org/2000/svg"
													viewBox="0 0 20 20"
													fill="currentColor"
													class="w-4 h-4"
												>
													<path
														fill-rule="evenodd"
														d="M15.312 11.424a5.5 5.5 0 01-9.201 2.466l-.312-.311h2.433a.75.75 0 000-1.5H3.989a.75.75 0 00-.75.75v4.242a.75.75 0 001.5 0v-2.43l.31.31a7 7 0 0011.712-3.138.75.75 0 00-1.449-.39zm1.23-3.723a.75.75 0 00.219-.53V2.929a.75.75 0 00-1.5 0V5.36l-.31-.31A7 7 0 003.239 8.188a.75.75 0 101.448.389A5.5 5.5 0 0113.89 6.11l.311.31h-2.432a.75.75 0 000 1.5h4.243a.75.75 0 00.53-.219z"
														clip-rule="evenodd"
													/>
												</svg>
											</button>
										</Tooltip>
									</div>
								</div>
								<div class=" mb-1 text-xs text-gray-400 dark:text-gray-500">
									{$i18n.t('WebUI will make requests to')}
									<span class=" text-gray-200">'{url}/models'</span>
								</div>
							{/each}
						</div>
					{/if}
				</div>
			</div>

			<hr class=" dark:border-gray-850" />

			<!-- Ollama API -->
			<div class="pr-1.5 space-y-2">
				<div class="flex justify-between items-center text-sm">
					<div class="  font-medium">{$i18n.t('Ollama API')}</div>

					<div class="mt-1">
						<Switch
							bind:state={ENABLE_OLLAMA_API}
							on:change={async () => {
								updateOllamaConfig(localStorage.token, ENABLE_OLLAMA_API);

								if (OLLAMA_BASE_URLS.length === 0) {
									OLLAMA_BASE_URLS = [''];
								}
							}}
						/>
					</div>
				</div>
				{#if ENABLE_OLLAMA_API}
					<div class="flex w-full gap-1.5">
						<div class="flex-1 flex flex-col gap-2">
							{#each OLLAMA_BASE_URLS as url, idx}
								<div class="flex gap-1.5">
									<input
										class="w-full rounded-lg py-2 px-4 text-sm dark:text-gray-300 dark:bg-gray-850 outline-none"
										placeholder={$i18n.t('Enter URL (e.g. http://localhost:11434)')}
										bind:value={url}
									/>

									<div class="self-center flex items-center">
										{#if idx === 0}
											<button
												class="px-1"
												on:click={() => {
													OLLAMA_BASE_URLS = [...OLLAMA_BASE_URLS, ''];
												}}
												type="button"
											>
												<svg
													xmlns="http://www.w3.org/2000/svg"
													viewBox="0 0 16 16"
													fill="currentColor"
													class="w-4 h-4"
												>
													<path
														d="M8.75 3.75a.75.75 0 0 0-1.5 0v3.5h-3.5a.75.75 0 0 0 0 1.5h3.5v3.5a.75.75 0 0 0 1.5 0v-3.5h3.5a.75.75 0 0 0 0-1.5h-3.5v-3.5Z"
													/>
												</svg>
											</button>
										{:else}
											<button
												class="px-1"
												on:click={() => {
													OLLAMA_BASE_URLS = OLLAMA_BASE_URLS.filter(
														(url, urlIdx) => idx !== urlIdx
													);
												}}
												type="button"
											>
												<svg
													xmlns="http://www.w3.org/2000/svg"
													viewBox="0 0 16 16"
													fill="currentColor"
													class="w-4 h-4"
												>
													<path d="M3.75 7.25a.75.75 0 0 0 0 1.5h8.5a.75.75 0 0 0 0-1.5h-8.5Z" />
												</svg>
											</button>
										{/if}
									</div>

									<div class="flex">
										<Tooltip content={$i18n.t('连接验证')} className="self-start mt-0.5">
											<button
												class="self-center p-2 bg-gray-200 hover:bg-gray-300 dark:bg-gray-900 dark:hover:bg-gray-850 rounded-lg transition"
												on:click={() => {
													verifyOllamaHandler(idx);
												}}
												type="button"
											>
												<svg
													xmlns="http://www.w3.org/2000/svg"
													viewBox="0 0 20 20"
													fill="currentColor"
													class="w-4 h-4"
												>
													<path
														fill-rule="evenodd"
														d="M15.312 11.424a5.5 5.5 0 01-9.201 2.466l-.312-.311h2.433a.75.75 0 000-1.5H3.989a.75.75 0 00-.75.75v4.242a.75.75 0 001.5 0v-2.43l.31.31a7 7 0 0011.712-3.138.75.75 0 00-1.449-.39zm1.23-3.723a.75.75 0 00.219-.53V2.929a.75.75 0 00-1.5 0V5.36l-.31-.31A7 7 0 003.239 8.188a.75.75 0 101.448.389A5.5 5.5 0 0113.89 6.11l.311.31h-2.432a.75.75 0 000 1.5h4.243a.75.75 0 00.53-.219z"
														clip-rule="evenodd"
													/>
												</svg>
											</button>
										</Tooltip>
									</div>
								</div>
							{/each}
						</div>
					</div>

					<div class="mt-2 text-xs text-gray-400 dark:text-gray-500">
						{$i18n.t('Trouble accessing Ollama?')}
						<a
							class=" text-gray-300 font-medium underline"
							href="https://github.com/open-webui/open-webui#troubleshooting"
							target="_blank"
						>
							{$i18n.t('Click here for help.')}
						</a>
					</div>
				{/if}
			</div>

			<!-- Net API -->
			<!-- <hr class=" dark:border-gray-700" />

			<div class="pr-1.5 space-y-2">
				<div class="flex justify-between items-center text-sm">
					<div class="  font-medium">{$i18n.t('NET API')}</div>

					<div class="mt-1">
						<Switch
							bind:state={$NetSettings['net_service']}
							on:change={async () => {
								console.log($NetSettings['net_service']);
								$NetSettings['net_service_is_true'] = false;
							}}
						/>
					</div>
				</div>
				{#if $NetSettings['net_service']}
					<div class="flex w-full gap-1.5">
						<div class="flex-1 flex flex-col gap-2">
							<div class="flex gap-1.5">
								<input
									class="w-full rounded-lg py-2 px-4 text-sm dark:text-gray-300 dark:bg-gray-850 outline-none"
									placeholder={$i18n.t('Enter URL (e.g. http://localhost:8888)')}
									bind:value={NET_BASE_URL}
								/>

								<div class="self-center flex items-center">
									<button
										class="px-1"
										on:click={() => {
											//NET_BASE_URLS = [...NET_BASE_URLS, ''];
											alert('新服务');
										}}
										type="button"
									>
										<svg
											xmlns="http://www.w3.org/2000/svg"
											viewBox="0 0 16 16"
											fill="currentColor"
											class="w-4 h-4"
										>
											<path
												d="M8.75 3.75a.75.75 0 0 0-1.5 0v3.5h-3.5a.75.75 0 0 0 0 1.5h3.5v3.5a.75.75 0 0 0 1.5 0v-3.5h3.5a.75.75 0 0 0 0-1.5h-3.5v-3.5Z"
											/>
										</svg>
									</button>
								</div>

								<div class="flex">
									<Tooltip content={$i18n.t('连接验证')} className="self-start mt-0.5">
										<button
											class="self-center p-2 bg-gray-200 hover:bg-gray-300 dark:bg-gray-900 dark:hover:bg-gray-850 rounded-lg transition"
											on:click={() => {
												//verifyOllamaHandler(idx);
											}}
											type="button"
										>
											<svg
												xmlns="http://www.w3.org/2000/svg"
												viewBox="0 0 20 20"
												fill="currentColor"
												class="w-4 h-4"
											>
												<path
													fill-rule="evenodd"
													d="M15.312 11.424a5.5 5.5 0 01-9.201 2.466l-.312-.311h2.433a.75.75 0 000-1.5H3.989a.75.75 0 00-.75.75v4.242a.75.75 0 001.5 0v-2.43l.31.31a7 7 0 0011.712-3.138.75.75 0 00-1.449-.39zm1.23-3.723a.75.75 0 00.219-.53V2.929a.75.75 0 00-1.5 0V5.36l-.31-.31A7 7 0 003.239 8.188a.75.75 0 101.448.389A5.5 5.5 0 0113.89 6.11l.311.31h-2.432a.75.75 0 000 1.5h4.243a.75.75 0 00.53-.219z"
													clip-rule="evenodd"
												/>
											</svg>
										</button>
									</Tooltip>
								</div>
							</div>
						</div>
					</div>

					<div class="mt-2 text-xs text-gray-400 dark:text-gray-500">
						{$i18n.t('连接新服务?')}
						<a
							class=" text-gray-300 font-medium underline"
							href="https://github.com/open-webui/open-webui#troubleshooting"
							target="_blank"
						>
							{$i18n.t('Click here for help.')}
						</a>
					</div>
				{/if}
			</div> -->
		{:else}
			<div class="flex h-full justify-center">
				<div class="my-auto">
					<Spinner className="size-6" />
				</div>
			</div>
		{/if}
	</div>

	<div class="flex justify-end pt-3 text-sm font-medium">
		<button
			class="  px-4 py-2 bg-emerald-700 hover:bg-emerald-800 text-gray-100 transition rounded-lg"
			type="submit"
		>
			{$i18n.t('Save')}
		</button>
	</div>
</form>
