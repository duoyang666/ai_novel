<script>
	import { getContext, createEventDispatcher, onMount } from 'svelte';

	const i18n = getContext('i18n');

	import CodeEditor from '$lib/components/common/CodeEditor.svelte';
	import { goto } from '$app/navigation';
	import ConfirmDialog from '$lib/components/common/ConfirmDialog.svelte';

	const dispatch = createEventDispatcher();

	let formElement = null;
	let loading = false;
	let showConfirm = false;

	export let edit = false;
	export let clone = false;

	export let id = '';
	export let name = '';
	export let meta = {
		description: ''
	};
	export let content = '';

	$: if (name && !edit && !clone) {
		id = name.replace(/\s+/g, '_').toLowerCase();
	}

	let codeEditor;
	let boilerplate = `import os
import requests
from datetime import datetime


class Tools:
    def __init__(self):
        pass

    # Add your custom tools using pure Python code here, make sure to add type hints
    # Use Sphinx-style docstrings to document your tools, they will be used for generating tools specifications
    # Please refer to function_calling_filter_pipeline.py file from pipelines project for an example

    def get_user_name_and_email_and_id(self, __user__: dict = {}) -> str:
        """
        Get the user name, Email and ID from the user object.
        """

        # Do not include :param for __user__ in the docstring as it should not be shown in the tool's specification
        # The session user object will be passed as a parameter when the function is called

        print(__user__)
        result = ""

        if "name" in __user__:
            result += f"User: {__user__['name']}"
        if "id" in __user__:
            result += f" (ID: {__user__['id']})"
        if "email" in __user__:
            result += f" (Email: {__user__['email']})"

        if result == "":
            result = "User: Unknown"

        return result

    def get_current_time(self) -> str:
        """
        Get the current time in a more human-readable format.
        :return: The current time.
        """

        now = datetime.now()
        current_time = now.strftime("%I:%M:%S %p")  # Using 12-hour format with AM/PM
        current_date = now.strftime(
            "%A, %B %d, %Y"
        )  # Full weekday, month name, day, and year

        return f"Current Date and Time = {current_date}, {current_time}"

    def calculator(self, equation: str) -> str:
        """
        Calculate the result of an equation.
        :param equation: The equation to calculate.
        """

        # Avoid using eval in production code
        # https://nedbatchelder.com/blog/201206/eval_really_is_dangerous.html
        try:
            result = eval(equation)
            return f"{equation} = {result}"
        except Exception as e:
            print(e)
            return "Invalid equation"

    def get_current_weather(self, city: str) -> str:
        """
        Get the current weather for a given city.
        :param city: The name of the city to get the weather for.
        :return: The current weather information or an error message.
        """
        api_key = os.getenv("OPENWEATHER_API_KEY")
        if not api_key:
            return (
                "API key is not set in the environment variable 'OPENWEATHER_API_KEY'."
            )

        base_url = "http://api.openweathermap.org/data/2.5/weather"
        params = {
            "q": city,
            "appid": api_key,
            "units": "metric",  # Optional: Use 'imperial' for Fahrenheit
        }

        try:
            response = requests.get(base_url, params=params)
            response.raise_for_status()  # Raise HTTPError for bad responses (4xx and 5xx)
            data = response.json()

            if data.get("cod") != 200:
                return f"Error fetching weather data: {data.get('message')}"

            weather_description = data["weather"][0]["description"]
            temperature = data["main"]["temp"]
            humidity = data["main"]["humidity"]
            wind_speed = data["wind"]["speed"]

            return f"Weather in {city}: {temperature}°C"
        except requests.RequestException as e:
            return f"Error fetching weather data: {str(e)}"
`;

	const saveHandler = async () => {
		loading = true;
		dispatch('save', {
			id,
			name,
			meta,
			content
		});
	};

	const submitHandler = async () => {
		if (codeEditor) {
			const res = await codeEditor.formatPythonCodeHandler();

			if (res) {
				console.log('Code formatted successfully');
				saveHandler();
			}
		}
	};
</script>

<div class=" flex flex-col justify-between w-full overflow-y-auto h-full">
	<div class="mx-auto w-full md:px-0 h-full">
		<form
			bind:this={formElement}
			class=" flex flex-col max-h-[100dvh] h-full"
			on:submit|preventDefault={() => {
				if (edit) {
					submitHandler();
				} else {
					showConfirm = true;
				}
			}}
		>
			<div class="mb-2.5">
				<button
					class="flex space-x-1"
					on:click={() => {
						goto('/workspace/tools');
					}}
					type="button"
				>
					<div class=" self-center">
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
					<div class=" self-center font-medium text-sm">{$i18n.t('Back')}</div>
				</button>
			</div>

			<div class="flex flex-col flex-1 overflow-auto h-0 rounded-lg">
				<div class="w-full mb-2 flex flex-col gap-1.5">
					<div class="flex gap-2 w-full">
						<input
							class="w-full px-3 py-2 text-sm font-medium bg-gray-50 dark:bg-gray-850 dark:text-gray-200 rounded-lg outline-none"
							type="text"
							placeholder="工具箱名称"
							bind:value={name}
							required
						/>

						<input
							class="w-full px-3 py-2 text-sm font-medium disabled:text-gray-300 dark:disabled:text-gray-700 bg-gray-50 dark:bg-gray-850 dark:text-gray-200 rounded-lg outline-none"
							type="text"
							placeholder="工具箱ID"
							bind:value={id}
							required
							disabled={edit}
						/>
					</div>
					<input
						class="w-full px-3 py-2 text-sm font-medium bg-gray-50 dark:bg-gray-850 dark:text-gray-200 rounded-lg outline-none"
						type="text"
						placeholder="工具包说明"
						bind:value={meta.description}
						required
					/>
				</div>

				<div class="mb-2 flex-1 overflow-auto h-0 rounded-lg">
					<CodeEditor
						bind:value={content}
						bind:this={codeEditor}
						{boilerplate}
						on:save={() => {
							if (formElement) {
								formElement.requestSubmit();
							}
						}}
					/>
				</div>

				<div class="pb-3 flex justify-between">
					<div class="flex-1 pr-3">
						<div class="text-xs text-gray-500 line-clamp-2">
							<span class=" font-semibold dark:text-gray-200">Warning:</span> Tools are a function
							calling system with arbitrary code execution <br />—
							<span class=" font-medium dark:text-gray-400"
								>don't install random tools from sources you don't trust.</span
							>
						</div>
					</div>

					<button
						class="px-3 py-1.5 text-sm font-medium bg-emerald-600 hover:bg-emerald-700 text-gray-50 transition rounded-lg"
						type="submit"
					>
						{$i18n.t('Save')}
					</button>
				</div>
			</div>
		</form>
	</div>
</div>

<ConfirmDialog
	bind:show={showConfirm}
	on:confirm={() => {
		submitHandler();
	}}
>
	<div class="text-sm text-gray-500">
		<div class=" bg-yellow-500/20 text-yellow-700 dark:text-yellow-200 rounded-lg px-4 py-3">
			<div>工具有一个函数调用系统，允许任意代码执行:</div>

			<ul class=" mt-1 list-disc pl-4 text-xs">
				<li>工具有一个函数调用系统，允许任意代码执行.</li>
				<li>不要从不完全信任的来源安装工具.</li>
			</ul>
		</div>

		<div class="my-3">
			我承认我已经阅读并理解我的行为的含义。我知道
			与执行任意代码相关的风险，我已经验证了
			消息来源。
		</div>
	</div>
</ConfirmDialog>
