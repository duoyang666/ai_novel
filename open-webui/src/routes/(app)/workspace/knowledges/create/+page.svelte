<!-- 知识库内容列表 -->

<script>
	import { toast } from 'svelte-sonner';
	import { goto } from '$app/navigation';
	import { knowledgeids } from '$lib/stores';
	import {
		get_knowledges_id,
		get_knowledges_search,
		del_knowledges_id,
		edit_knowledges_id
	} from '$lib/apis/net';
	import { settings, user, config, models, tools } from '$lib/stores';
	import { page } from '$app/stores';
	import { onMount, tick, getContext } from 'svelte';
	import Modal from '$lib/components/common/Modal.svelte';

	const i18n = getContext('i18n');
	let loading = false;
	let show = false;
	let data = {};

	//let knowledge;
	//$: knowledge = $knowledgeids.filter((doc) => doc);

	let query = '';
	onMount(async () => {
		window.addEventListener('message', async (event) => {
			//检查消息的来源是否为允许的来源
			if (
				!['https://openwebui.com', 'https://www.openwebui.com', 'http://localhost:5173'].includes(
					event.origin
				)
			)
				return;

			// const model = JSON.parse(event.data);
			// console.log(model);
		});

		// if (window.opener ?? false) {
		// 	window.opener.postMessage('loaded', '*');
		// }

		await loadList();
	});

	let p = 0;
	let s = 30;
	let loadList = async () => {
		try {
			let id = $page.url.searchParams.get('id');
			let res = await get_knowledges_id(id, p, s);
			if (res) {
				res.forEach((r) => {
					let o = Object.keys(r.value);
					if (o && o.length == 1) r.key = o[0];
					else if (o && o.length > 1) r.key = o[o.length - 1];
				});
				knowledgeids.set(res);
			} else knowledgeids.set([]);
		} catch (error) {
			knowledgeids.set([]);
		}
		//document.getElementById("div0001").innerHTML=Math.random(0,1)
	};

	let search = async (q) => {
		try {
			let res = await get_knowledges_search(q);
			if (res) {
				res.forEach((r) => {
					let o = Object.keys(r.value);
					if (o && o.length == 1) r.key = o[0];
					else if (o && o.length > 1) r.key = o[o.length - 1];
				});
				knowledgeids.set(res);
			} else knowledgeids.set([]);
		} catch (error) {
			console.log(error);
			knowledgeids.set([]);
		}
	};
</script>

<div class="w-full max-h-full">
	<button
		class="flex space-x-1 pb-6"
		on:click={() => {
			goto('/workspace/knowledges');
		}}
	>
		<div class="self-center">
			<svg
				xmlns="http://www.w3.org/2000/svg"
				viewBox="0 0 20 20"
				fill="currentColor"
				class="w-4 h-4"
			>
				<path
					fill-rule="evenodd"
					d="M17 10a.75.75 0 01-.75.75H5.612l4.158 3.96a.75.75 0 11-1.04 1.08l-5.5-5.25a.75.75 0 010-1.08l5.5-5.25a.75.75 0 111.04 1.08L5.612 9.25H16.25A.75.75 0 0117 10z"
					clip-rule="evenodd"
				/>
			</svg>
		</div>
		<div class="self-center font-medium text-sm">{$i18n.t('Back')}</div>

		<div class="self-center">
			第{p + 1}页
		</div>
	</button>

	<div class=" flex w-full space-x-2">
		<div class="flex flex-1">
			<div class=" self-center ml-1 mr-3">
				<svg
					xmlns="http://www.w3.org/2000/svg"
					viewBox="0 0 20 20"
					fill="currentColor"
					class="w-4 h-4"
				>
					<path
						fill-rule="evenodd"
						d="M9 3.5a5.5 5.5 0 100 11 5.5 5.5 0 000-11zM2 9a7 7 0 1112.452 4.391l3.328 3.329a.75.75 0 11-1.06 1.06l-3.329-3.328A7 7 0 012 9z"
						clip-rule="evenodd"
					/>
				</svg>
			</div>
			<input
				class=" w-full text-sm pr-4 py-1 rounded-r-xl outline-none bg-transparent"
				bind:value={query}
				on:change={async (e) => {
					console.log(e.target.value);
					await search(e.target.value);
				}}
				placeholder={$i18n.t('Search Documents')}
			/>
		</div>

		<!-- <div>
			<button
				class=" px-2 py-2 rounded-xl border border-gray-200 dark:border-gray-600 dark:border-0 hover:bg-gray-100 dark:bg-gray-800 dark:hover:bg-gray-700 transition font-medium text-sm flex items-center space-x-1"
				on:click={() => {

				}}
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
		</div> -->
	</div>

	<hr class="my-3 dark:border-gray-850" />

	<div class="w-full">
		<div
			class="relative w-full flex gap-2 snap-x snap-mandatory md:snap-none overflow-x-auto tabs"
			id="suggestions-container"
		>
			{#each $knowledgeids as doc, docIdx}
				<div class="snap-center shrink-0">
					<div style="margin-bottom: 10px;">
						<select bind:value={doc.key}>
							{#each Object.keys(doc.value) as key}
								<option value={key} class="bg-gray-100 dark:bg-gray-700">{key}</option>
							{/each}
						</select>
					</div>

					<div
						class="flex flex-col flex-1 shrink-0 justify-between p-1 px-3 bg-gray-50 hover:bg-gray-100 dark:bg-gray-850 dark:hover:bg-gray-800 rounded-3xl transition group"
					>
						<div class="flex flex-col text-left">
							{#if doc.value[doc.key]}
								<!-- <div
									class="font-medium dark:text-gray-300 dark:group-hover:text-gray-200 transition">
									{doc.title}
								</div> -->
								<div
									style="overflow:auto;height:420px;white-space: pre-line;"
									class="flex flex-col w-64"
								>
									{doc.value[doc.key]}
								</div>
							{/if}
						</div>
					</div>

					<!-- 按钮 -->
					<div class="w-full flex">
						<button
							class="text-sm px-2 py-2 dark:text-gray-300 dark:hover:text-white hover:bg-black/5 dark:hover:bg-white/5 rounded-xl"
							type="button"
							on:click={async () => {
								show = true;
								data = doc;
							}}
						>
							<svg
								xmlns="http://www.w3.org/2000/svg"
								width="16"
								height="16"
								fill="currentColor"
								class="bi bi-pencil-square"
								viewBox="0 0 16 16"
							>
								<path
									d="M15.502 1.94a.5.5 0 0 1 0 .706L14.459 3.69l-2-2L13.502.646a.5.5 0 0 1 .707 0l1.293 1.293zm-1.75 2.456-2-2L4.939 9.21a.5.5 0 0 0-.121.196l-.805 2.414a.25.25 0 0 0 .316.316l2.414-.805a.5.5 0 0 0 .196-.12l6.813-6.814z"
								/>
								<path
									fill-rule="evenodd"
									d="M1 13.5A1.5 1.5 0 0 0 2.5 15h11a1.5 1.5 0 0 0 1.5-1.5v-6a.5.5 0 0 0-1 0v6a.5.5 0 0 1-.5.5h-11a.5.5 0 0 1-.5-.5v-11a.5.5 0 0 1 .5-.5H9a.5.5 0 0 0 0-1H2.5A1.5 1.5 0 0 0 1 2.5v11z"
								/>
							</svg>
						</button>

						<button
							class="text-sm px-2 py-2 dark:text-gray-300 dark:hover:text-white hover:bg-black/5 dark:hover:bg-white/5 rounded-xl"
							type="button"
							on:click={async () => {
								await del_knowledges_id(doc.id);
								await loadList();
							}}
						>
							<svg
								xmlns="http://www.w3.org/2000/svg"
								width="16"
								height="16"
								fill="currentColor"
								class="bi bi-trash"
								viewBox="0 0 16 16"
							>
								<path
									d="M5.5 5.5A.5.5 0 0 1 6 6v6a.5.5 0 0 1-1 0V6a.5.5 0 0 1 .5-.5zm2.5 0a.5.5 0 0 1 .5.5v6a.5.5 0 0 1-1 0V6a.5.5 0 0 1 .5-.5zm3 .5a.5.5 0 0 0-1 0v6a.5.5 0 0 0 1 0V6z"
								/>
								<path
									fill-rule="evenodd"
									d="M14.5 3a1 1 0 0 1-1 1H13v9a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V4h-.5a1 1 0 0 1-1-1V2a1 1 0 0 1 1-1H6a1 1 0 0 1 1-1h2a1 1 0 0 1 1 1h3.5a1 1 0 0 1 1 1v1zM4.118 4 4 4.059V13a1 1 0 0 0 1 1h6a1 1 0 0 0 1-1V4.059L11.882 4H4.118zM2.5 3V2h11v1h-11z"
								/>
							</svg>
						</button>
					</div>
				</div>
			{/each}
		</div>
	</div>

	<div class="w-full flex">
		<button
			class="text-sm px-2 py-2 dark:text-gray-300 dark:hover:text-white hover:bg-black/5 dark:hover:bg-white/5 rounded-xl"
			type="button"
			on:click={() => {
				p--;
				if (p <= 0) p = 0;
				loadList();
			}}
		>
			<svg
				xmlns="http://www.w3.org/2000/svg"
				width="16"
				height="16"
				fill="currentColor"
				class="bi bi-arrow-left"
				viewBox="0 0 16 16"
			>
				<path
					fill-rule="evenodd"
					d="M15 8a.5.5 0 0 0-.5-.5H2.707l3.147-3.146a.5.5 0 1 0-.708-.708l-4 4a.5.5 0 0 0 0 .708l4 4a.5.5 0 0 0 .708-.708L2.707 8.5H14.5A.5.5 0 0 0 15 8z"
				/>
			</svg>
		</button>

		<button
			class="text-sm px-2 py-2 dark:text-gray-300 dark:hover:text-white hover:bg-black/5 dark:hover:bg-white/5 rounded-xl"
			type="button"
			on:click={() => {
				p++;
				loadList();
			}}
		>
			<svg
				xmlns="http://www.w3.org/2000/svg"
				width="16"
				height="16"
				fill="currentColor"
				class="bi bi-arrow-right"
				viewBox="0 0 16 16"
			>
				<path
					fill-rule="evenodd"
					d="M1 8a.5.5 0 0 1 .5-.5h11.793l-3.147-3.146a.5.5 0 0 1 .708-.708l4 4a.5.5 0 0 1 0 .708l-4 4a.5.5 0 0 1-.708-.708L13.293 8.5H1.5A.5.5 0 0 1 1 8z"
				/>
			</svg>
		</button>
	</div>
</div>

<Modal size="sm" bind:show>
	<div>
		<div class=" flex justify-between dark:text-gray-300 px-5 pt-4">
			<div class=" text-lg font-medium self-center">{$i18n.t('Edit')}</div>
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
			<div class="flex flex-col w-full space-y-6">
				<textarea bind:value={data.value[data.key]} style="width: 100%; height: 280px;" />

				<button
					class="px-4 py-2 bg-emerald-700 hover:bg-emerald-800 text-gray-100 transition rounded-lg"
					type="submit"
					on:click={async () => {
						let obj = {};
						obj[data['id']] = data['value'];

						let res = await edit_knowledges_id(obj);
						if (res) {
							// let ks = $knowledgeids.filter((c) => c.id == data.id);
							// console.log(ks)

							knowledgeids.set([...$knowledgeids]);
							toast.success('成功');
						} else toast.success('失败');

						show = false;
					}}
				>
					{$i18n.t('Save')}
				</button>
			</div>
		</div>
	</div>
</Modal>
