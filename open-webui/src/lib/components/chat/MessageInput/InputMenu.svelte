<script lang="ts">
	import { DropdownMenu } from 'bits-ui';
	import { flyAndScale } from '$lib/utils/transitions';
	import { getContext } from 'svelte';

	import Dropdown from '$lib/components/common/Dropdown.svelte';
	import Tooltip from '$lib/components/common/Tooltip.svelte';
	import DocumentArrowUpSolid from '$lib/components/icons/DocumentArrowUpSolid.svelte';
	import Switch from '$lib/components/common/Switch.svelte';
	import GlobeAltSolid from '$lib/components/icons/GlobeAltSolid.svelte';
	import { config } from '$lib/stores';
	import WrenchSolid from '$lib/components/icons/WrenchSolid.svelte';

	const i18n = getContext('i18n');

	export let uploadFilesHandler: Function;

	export let selectedToolIds: string[] = [];
	export let webSearchEnabled: boolean;
	export let knowSearchEnabled: boolean;
	export let tools = {};
	export let onClose: Function;

	$: tools = Object.fromEntries(
		Object.keys(tools).map((toolId) => [
			toolId,
			{
				...tools[toolId],
				enabled: selectedToolIds.includes(toolId)
			}
		])
	);

	let show = false;
</script>

<Dropdown
	bind:show
	on:change={(e) => {
		if (e.detail === false) {
			onClose();
		}
	}}
>
	<Tooltip content={$i18n.t('More')}>
		<slot />
	</Tooltip>

	<div slot="content">
		<DropdownMenu.Content
			class="w-full max-w-[200px] rounded-xl px-1 py-1  border-gray-300/30 dark:border-gray-700/50 z-50 bg-white dark:bg-gray-850 dark:text-white shadow"
			sideOffset={15}
			alignOffset={-8}
			side="top"
			align="start"
			transition={flyAndScale}
		>
			{#if Object.keys(tools).length > 0}
				<div class="  max-h-28 overflow-y-auto scrollbar-hidden">
					{#each Object.keys(tools) as toolId}
						<div
							class="flex gap-2 items-center px-3 py-2 text-sm font-medium cursor-pointer rounded-xl"
						>
							<div class="flex-1 flex items-center gap-2">
								<WrenchSolid />
								<Tooltip content={tools[toolId]?.description ?? ''} className="flex-1">
									<div class=" line-clamp-1">{tools[toolId].name}</div>
								</Tooltip>
							</div>

							<Switch
								bind:state={tools[toolId].enabled}
								on:change={(e) => {
									selectedToolIds = e.detail
										? [...selectedToolIds, toolId]
										: selectedToolIds.filter((id) => id !== toolId);
								}}
							/>
						</div>
					{/each}
				</div>

				<hr class="border-gray-100 dark:border-gray-800 my-1" />
			{/if}

			<div class="flex gap-2 items-center px-3 py-2 text-sm font-medium cursor-pointer rounded-xl">
				<div class="flex-1 flex items-center gap-2">
					<svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" class="bi bi-book" viewBox="0 0 16 16">
						<path d="M1 2.828c.885-.37 2.154-.769 3.388-.893 1.33-.134 2.458.063 3.112.752v9.746c-.935-.53-2.12-.603-3.213-.493-1.18.12-2.37.461-3.287.811V2.828zm7.5-.141c.654-.689 1.782-.886 3.112-.752 1.234.124 2.503.523 3.388.893v9.923c-.918-.35-2.107-.692-3.287-.81-1.094-.111-2.278-.039-3.213.492V2.687zM8 1.783C7.015.936 5.587.81 4.287.94c-1.514.153-3.042.672-3.994 1.105A.5.5 0 0 0 0 2.5v11a.5.5 0 0 0 .707.455c.882-.4 2.303-.881 3.68-1.02 1.409-.142 2.59.087 3.223.877a.5.5 0 0 0 .78 0c.633-.79 1.814-1.019 3.222-.877 1.378.139 2.8.62 3.681 1.02A.5.5 0 0 0 16 13.5v-11a.5.5 0 0 0-.293-.455c-.952-.433-2.48-.952-3.994-1.105C10.413.809 8.985.936 8 1.783z"/>
					  </svg>
					<div class=" line-clamp-1">知识库搜索</div>
				</div>

				<Switch bind:state={knowSearchEnabled} />
			</div>

			<hr class="border-gray-100 dark:border-gray-800 my-1" />

			{#if $config?.features?.enable_web_search}
				<div
					class="flex gap-2 items-center px-3 py-2 text-sm font-medium cursor-pointer rounded-xl"
				>
					<div class="flex-1 flex items-center gap-2">
						<GlobeAltSolid />
						<div class=" line-clamp-1">{$i18n.t('Web Search')}</div>
					</div>

					<Switch bind:state={webSearchEnabled} />
				</div>

				<hr class="border-gray-100 dark:border-gray-800 my-1" />
			{/if}

			<DropdownMenu.Item
				class="flex gap-2 items-center px-3 py-2 text-sm  font-medium cursor-pointer hover:bg-gray-50 dark:hover:bg-gray-800  rounded-xl"
				on:click={() => {
					uploadFilesHandler();
				}}
			>
				<DocumentArrowUpSolid />
				<div class=" line-clamp-1">{$i18n.t('Upload Files')}</div>
			</DropdownMenu.Item>
		</DropdownMenu.Content>
	</div>
</Dropdown>
