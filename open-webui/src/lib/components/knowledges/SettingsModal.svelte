<script>
	import { toast } from 'svelte-sonner';
	import { onMount, getContext, tick } from 'svelte';
	import Modal from '../common/Modal.svelte';
	import { get_settings, set_settings } from '$lib/apis/net';

	const i18n = getContext('i18n');

	export let show = false;

	let setStrs = '。,；';
	let size = 256;
	let selectedTab = 'general';

	onMount(async () => {
		try {
			let res = await get_settings('WebUi_Spile');
			console.log(res);
			if (res) {
				res = JSON.parse(res);
				setStrs = res.setStrs;
				size = res.size;
			}
		} catch (error) {}
	});
</script>

<Modal bind:show>
	<div>
		<div class=" flex justify-between dark:text-gray-300 px-5 pt-4">
			<div class=" text-lg font-medium self-center">{$i18n.t('Document Settings')}</div>
			<button
				class="self-center"
				on:click={() => {
					show = false;
				}}
			>
				<svg
					xmlns="http://www.w3.org/2000/svg"
					viewBox="0 0 20 20"
					fill="currentColor"
					class="w-5 h-5"
				>
					<path
						d="M6.28 5.22a.75.75 0 00-1.06 1.06L8.94 10l-3.72 3.72a.75.75 0 101.06 1.06L10 11.06l3.72 3.72a.75.75 0 101.06-1.06L11.06 10l3.72-3.72a.75.75 0 00-1.06-1.06L10 8.94 6.28 5.22z"
					/>
				</svg>
			</button>
		</div>

		<div class="flex flex-col md:flex-row w-full p-4 md:space-x-4">
			<div
				class="tabs flex flex-row overflow-x-auto space-x-1 md:space-x-0 md:space-y-1 md:flex-col flex-1 md:flex-none md:w-40 dark:text-gray-200 text-xs text-left mb-3 md:mb-0"
			>
				<button
					class="px-2.5 py-2.5 min-w-fit rounded-lg flex-1 md:flex-none flex text-right transition {selectedTab ===
					'general'
						? 'bg-gray-200 dark:bg-gray-700'
						: ' hover:bg-gray-300 dark:hover:bg-gray-800'}"
					on:click={() => {
						selectedTab = 'general';
					}}
				>
					<div class=" self-center mr-2">
						<svg
							xmlns="http://www.w3.org/2000/svg"
							viewBox="0 0 16 16"
							fill="currentColor"
							class="w-4 h-4"
						>
							<path
								fill-rule="evenodd"
								d="M6.955 1.45A.5.5 0 0 1 7.452 1h1.096a.5.5 0 0 1 .497.45l.17 1.699c.484.12.94.312 1.356.562l1.321-1.081a.5.5 0 0 1 .67.033l.774.775a.5.5 0 0 1 .034.67l-1.08 1.32c.25.417.44.873.561 1.357l1.699.17a.5.5 0 0 1 .45.497v1.096a.5.5 0 0 1-.45.497l-1.699.17c-.12.484-.312.94-.562 1.356l1.082 1.322a.5.5 0 0 1-.034.67l-.774.774a.5.5 0 0 1-.67.033l-1.322-1.08c-.416.25-.872.44-1.356.561l-.17 1.699a.5.5 0 0 1-.497.45H7.452a.5.5 0 0 1-.497-.45l-.17-1.699a4.973 4.973 0 0 1-1.356-.562L4.108 13.37a.5.5 0 0 1-.67-.033l-.774-.775a.5.5 0 0 1-.034-.67l1.08-1.32a4.971 4.971 0 0 1-.561-1.357l-1.699-.17A.5.5 0 0 1 1 8.548V7.452a.5.5 0 0 1 .45-.497l1.699-.17c.12-.484.312-.94.562-1.356L2.629 4.107a.5.5 0 0 1 .034-.67l.774-.774a.5.5 0 0 1 .67-.033L5.43 3.71a4.97 4.97 0 0 1 1.356-.561l.17-1.699ZM6 8c0 .538.212 1.026.558 1.385l.057.057a2 2 0 0 0 2.828-2.828l-.058-.056A2 2 0 0 0 6 8Z"
								clip-rule="evenodd"
							/>
						</svg>
					</div>
					<div class=" self-center">{$i18n.t('General')}</div>
				</button>
			</div>
			<div class="flex-1 md:min-h-[380px]">
				{#if selectedTab === 'general'}
					<div class="space-y-3 pr-1.5 overflow-y-scroll h-[24rem] max-h-[25rem]">
						<div class="space-y-3">
							<div class="flex justify-between items-center text-sm">
								<div class="  font-medium">{$i18n.t('分隔符')}</div>
							</div>
							<div class="flex w-full gap-1.5">
								<input
									class=" w-full rounded-lg py-2 px-4 text-sm dark:text-gray-300 dark:bg-gray-850 outline-none"
									bind:value={setStrs}
								/>
							</div>
						</div>
						<div class="space-y-3">
							<div class="flex justify-between items-center text-sm">
								<div class="  font-medium">{$i18n.t('大小')}</div>
							</div>
							<div class="flex w-full gap-1.5">
								<input
									class=" w-full rounded-lg py-2 px-4 text-sm dark:text-gray-300 dark:bg-gray-850 outline-none"
									type="number"
									min="128"
									max="16000"
									step="1"
									bind:value={size}
								/>
							</div>
						</div>
					</div>
				{/if}
			</div>
		</div>

		<div class="flex flex-col md:flex-row w-full p-4 md:space-x-4">
			<button
				class=" px-4 py-2 bg-emerald-700 hover:bg-emerald-800 text-gray-100 transition rounded-lg"
				on:click={async () => {
					if (!setStrs || setStrs == '') setStrs = '。';
					if (!size || size < 128) size = 128;

					let values = { setStrs: setStrs, size: size };
					let res = await set_settings({
						ClassName: 'WebUi_Spile',
						JsonData: JSON.stringify(values)
					});
					if (res) toast.success('成功');
					else toast.error('失败');
				}}
			>
				{$i18n.t('Save')}
			</button>
		</div>
	</div>
</Modal>
