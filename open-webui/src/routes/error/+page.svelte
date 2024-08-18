<script>
	import { goto } from '$app/navigation';
	import { WEBUI_NAME, config } from '$lib/stores';
	import { onMount, getContext } from 'svelte';
	import img from '$lib/gong.jpg';

	const i18n = getContext('i18n');

	let loaded = false;

	onMount(async () => {
		if ($config) {
			await goto('/');
		}

		loaded = true;
	});
</script>

{#if loaded}
	<div class="absolute w-full h-full flex z-50">
		<div class="absolute rounded-xl w-full h-full backdrop-blur flex justify-center">
			<div class="m-auto pb-44 flex flex-col justify-center">
				<div class="max-w-md">
					<div class="text-center text-2xl font-medium z-50">
						{$i18n.t('{{webUIName}} Backend Required', { webUIName: $WEBUI_NAME })}
					</div>

					<div class=" mt-4 text-center text-sm w-full">
						{$i18n.t(
							"Oops! You're using an unsupported method (frontend only). Please serve the WebUI from the backend."
						)}

						<br class=" " />
						<br class=" " />
						<a
							class=" font-semibold underline"
							href="https://github.com/open-webui/open-webui#how-to-install-"
							target="_blank">{$i18n.t('See readme.md for instructions')}</a
						>
						{$i18n.t('or')}
						<a class=" font-semibold underline" href="https://discord.gg/5rJgQTnV4s" target="_blank"
							>{$i18n.t('join our Discord for help.')}</a
						>
					</div>

					<div class=" mt-6 mx-auto relative group w-fit">
						<button
							class="relative z-20 flex px-5 py-2 rounded-full bg-gray-100 hover:bg-gray-200 transition font-medium text-sm"
							on:click={() => {
								location.href = '/';
							}}
						>
							{$i18n.t('Check Again')}
						</button>
					</div>

					<div>
						<div class="mt-2 text-xs text-gray-400 dark:text-gray-500">
							<a
								class=" text-gray-500 dark:text-gray-300 font-medium"
								href="https://space.bilibili.com/353013779/dynamic"
								target="_blank">魔改版 {$i18n.t('Created by')} DuoYang</a
							>
						</div>
						<div class="mt-2 text-xs text-gray-400 dark:text-gray-500">
							<a
								class=" text-gray-500 dark:text-gray-300 font-medium"
								href="https://space.bilibili.com/353013779/dynamic"
								target="_blank">B站</a
							>
							<a
								class=" text-gray-500 dark:text-gray-300 font-medium"
								href="https://weibo.com/u/7880548860"
								target="_blank">微博</a
							>
						</div>
						<div class="mt-2 text-xs text-gray-400 dark:text-gray-500">
							公众号：多样分享，可搜索，fyduoyang
						</div>
						<img style="width: 200px; margin-top: 10px;" alt="gong" src={img} />
					</div>
				</div>
			</div>
		</div>
	</div>
{/if}
