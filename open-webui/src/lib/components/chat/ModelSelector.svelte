<!-- 模型下拉框 -->

<script lang="ts">
	import { models, showSettings, settings, user, mobile } from '$lib/stores';
	import { onMount, tick, getContext } from 'svelte';
	import { toast } from 'svelte-sonner';
	import Selector from './ModelSelector/Selector.svelte';
	import Tooltip from '../common/Tooltip.svelte';

	import { setDefaultModels } from '$lib/apis/configs';
	import { updateUserSettings } from '$lib/apis/users';
	import { NetSettings, NetApi } from '$lib/constants';

	const i18n = getContext('i18n');

	export let selectedModels = [''];
	export let disabled = false;

	export let showSetDefault = true;

	const saveDefaultModel = async () => {
		const hasEmptyModel = selectedModels.filter((it) => it === '');
		if (hasEmptyModel.length) {
			toast.error($i18n.t('Choose a model before saving...'));
			return;
		}
		//console.log(selectedModels)
		settings.set({ ...$settings, models: selectedModels });
		await updateUserSettings(localStorage.token, { ui: $settings });

		toast.success($i18n.t('Default model updated'));
	};

	$: if (selectedModels.length > 0 && $models.length > 0) {
		selectedModels = selectedModels.map((model) =>
			$models.map((m) => m.id).includes(model) ? model : ''
		);
	}
</script>

<div class="flex flex-col w-full items-center md:items-start">
	{#each selectedModels as selectedModel, selectedModelIdx}
		<div class="flex w-full max-w-fit">
			<div class="overflow-hidden w-full">
				<div class="mr-1 max-w-full">
					<Selector
						placeholder={$i18n.t('Select a model')}
						items={$models.map((model) => ({
							value: model.id,
							label: model.name,
							model: model
						}))}
						bind:value={selectedModel}
					/>
				</div>
			</div>

			{#if selectedModelIdx === 0}
				<div class="  self-center mr-2 disabled:text-gray-600 disabled:hover:text-gray-600">
					<Tooltip content={$i18n.t('Add Model')}>
						<button
							class=" "
							{disabled}
							on:click={() => {
								selectedModels = [...selectedModels, ''];
							}}
						>
							<svg
								xmlns="http://www.w3.org/2000/svg"
								fill="none"
								viewBox="0 0 24 24"
								stroke-width="2"
								stroke="currentColor"
								class="size-3.5"
							>
								<path stroke-linecap="round" stroke-linejoin="round" d="M12 6v12m6-6H6" />
							</svg>
						</button>
					</Tooltip>
				</div>
			{:else}
				<div class="  self-center disabled:text-gray-600 disabled:hover:text-gray-600 mr-2">
					<Tooltip content={$i18n.t('Remove Model')}>
						<button
							{disabled}
							on:click={() => {
								selectedModels.splice(selectedModelIdx, 1);
								selectedModels = selectedModels;
							}}
						>
							<svg
								xmlns="http://www.w3.org/2000/svg"
								fill="none"
								viewBox="0 0 24 24"
								stroke-width="2"
								stroke="currentColor"
								class="size-3.5"
							>
								<path stroke-linecap="round" stroke-linejoin="round" d="M19.5 12h-15" />
							</svg>
						</button>
					</Tooltip>
				</div>
			{/if}

			{#if selectedModelIdx === 0}
				<!-- 刷新 -->
				<div class=" self-center mr-2 disabled:text-gray-600 disabled:hover:text-gray-600">
					<Tooltip content={$i18n.t('判断服务状态')}>
						<button
							class=" "
							on:click={async () => {
								//NetApi.toggle_connect()
								await NetApi.is_connect();
								if ($NetSettings['net_service_is_true']) {
									toast.success($i18n.t('Server connection verified'));
								} else {
									toast.error($i18n.t('Error'));
								}
							}}
						>
							<svg
								xmlns="http://www.w3.org/2000/svg"
								width="16"
								height="16"
								fill="currentColor"
								class="bi bi-arrow-repeat"
								viewBox="0 0 16 16"
							>
								<path
									d="M11.534 7h3.932a.25.25 0 0 1 .192.41l-1.966 2.36a.25.25 0 0 1-.384 0l-1.966-2.36a.25.25 0 0 1 .192-.41zm-11 2h3.932a.25.25 0 0 0 .192-.41L2.692 6.23a.25.25 0 0 0-.384 0L.342 8.59A.25.25 0 0 0 .534 9z"
								/>
								<path
									fill-rule="evenodd"
									d="M8 3c-1.552 0-2.94.707-3.857 1.818a.5.5 0 1 1-.771-.636A6.002 6.002 0 0 1 13.917 7H12.9A5.002 5.002 0 0 0 8 3zM3.1 9a5.002 5.002 0 0 0 8.757 2.182.5.5 0 1 1 .771.636A6.002 6.002 0 0 1 2.083 9H3.1z"
								/>
							</svg>
						</button>
					</Tooltip>
				</div>
			{/if}
		</div>
	{/each}
</div>

{#if showSetDefault && !$mobile}
	<div class="text-left mt-0.5 ml-1 text-[0.7rem] text-gray-500">
		<button on:click={saveDefaultModel}> {$i18n.t('Set as default')}</button>
	</div>
{/if}
