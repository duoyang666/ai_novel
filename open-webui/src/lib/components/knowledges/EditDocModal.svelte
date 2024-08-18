<script lang="ts">
	import { toast } from 'svelte-sonner';
	import { onMount, getContext } from 'svelte';
	import { edit_knowledges, get_knowledges } from '$lib/apis/net';
	import Modal from '../common/Modal.svelte';
	import TagInput from '../common/Tags/TagInput.svelte';
	import Tags from '../common/Tags.svelte';

	const i18n = getContext('i18n');

	export let show = false;
	export let selectedDoc;
	export let loadList;

	let tags = [];
	let doc = {
		title: ''
	};

	const submitHandler = async () => {
		let obj = {
			tags: tags.map((c) => c.name).join(','),
			id: doc.id,
			title: doc.title
		};
		const res = await edit_knowledges(obj).catch((error) => {
			toast.error(error);
			return null;
		});
		show = false;
		if (res) {
			loadList();
			toast.success(`编辑成功`);
		} else toast.error(`编辑失败`);		
	};

	const addTagHandler = async (tagName) => {
		if (!tags.find((tag) => tag.name === tagName) && tagName !== '') {
			tags = [...tags, { name: tagName }];
		}
	};

	const deleteTagHandler = async (tagName) => {
		tags = tags.filter((tag) => tag.name !== tagName);
	};

	onMount(() => {
		if (selectedDoc) {
			doc = JSON.parse(JSON.stringify(selectedDoc));
			console.log(doc);
			tags = doc?.tags.split(',') ?? [];
			tags = tags.map((c) => {
				return { name: c };
			});
			tags = tags.filter((c) => c.name);
		}
	});
</script>

<Modal size="sm" bind:show>
	<div>
		<div class=" flex justify-between dark:text-gray-300 px-5 pt-4">
			<div class=" text-lg font-medium self-center">{$i18n.t('Edit Doc')}</div>
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
		<div class="flex flex-col md:flex-row w-full px-5 py-4 md:space-x-4 dark:text-gray-200">
			<div class=" flex flex-col w-full sm:flex-row sm:justify-center sm:space-x-6">
				<form
					class="flex flex-col w-full"
					on:submit|preventDefault={() => {
						submitHandler();
					}}
				>
					<div class=" flex flex-col space-y-1.5">
						<div class="flex flex-col w-full">
							<div class=" mb-1 text-xs text-gray-500">{$i18n.t('Title')}</div>

							<div class="flex-1">
								<input
									class="w-full rounded-xl py-2 px-4 text-sm dark:text-gray-300 dark:bg-gray-850 outline-none"
									type="text"
									bind:value={doc.title}
									autocomplete="off"
									required
								/>
							</div>
						</div>

						<div class="flex flex-col w-full">
							<div class=" mb-2 text-xs text-gray-500">{$i18n.t('Tags')}</div>
							<Tags {tags} addTag={addTagHandler} deleteTag={deleteTagHandler} />
						</div>
					</div>

					<div class="flex justify-end pt-5 text-sm font-medium">
						<button
							class=" px-4 py-2 bg-emerald-700 hover:bg-emerald-800 text-gray-100 transition rounded-lg"
							type="submit"
						>
							{$i18n.t('Save')}
						</button>
					</div>
				</form>
			</div>
		</div>
	</div>
</Modal>

<style>
	input::-webkit-outer-spin-button,
	input::-webkit-inner-spin-button {
		/* display: none; <- Crashes Chrome on hover */
		-webkit-appearance: none;
		margin: 0; /* <-- Apparently some margin are still there even though it's hidden */
	}

	.tabs::-webkit-scrollbar {
		display: none; /* for Chrome, Safari and Opera */
	}

	.tabs {
		-ms-overflow-style: none; /* IE and Edge */
		scrollbar-width: none; /* Firefox */
	}

	input[type='number'] {
		-moz-appearance: textfield; /* Firefox */
	}
</style>
