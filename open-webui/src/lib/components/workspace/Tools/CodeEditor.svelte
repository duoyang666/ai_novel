<script lang="ts">
	import CodeEditor from '$lib/components/common/CodeEditor.svelte';
	import { createEventDispatcher } from 'svelte';

	const dispatch = createEventDispatcher();

	export let value = '';

	let codeEditor;
	let boilerplate = `import os
import requests
from datetime import datetime


class Tools:
    def __init__(self):
        pass

        # 使用纯Python代码添加自定义工具
        # 使用Sphinx样式的文档字符串来记录工具，它们将用于生成工具规范
        # 请参考管道项目中的function_calling_filter_pipeline.py文件以获得示例

    def get_user_name_and_email_and_id(self, __user__: dict = {}) -> str:
        """
        Get the user name, Email and ID from the user object.
        """

        # 不要在docstring中包含：_user__的param，因为它不应该显示在工具的规范中
        # 当函数调用时，会话用户对象将作为参数传递

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
        以更易于阅读的格式获取当前时间。
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
        计算一个方程的结果。
        :param equation: The equation to calculate.
        """

        # 避免在生产代码中使用eval
        # https://nedbatchelder.com/blog/201206/eval_really_is_dangerous.html
        try:
            result = eval(equation)
            return f"{equation} = {result}"
        except Exception as e:
            print(e)
            return "Invalid equation"

    def get_current_weather(self, city: str) -> str:
        """
        获取给定城市的当前天气。
        :param city: The name of the city to get the weather for.
        :return: The current weather information or an error message.
        """
        api_key = os.getenv("OPENWEATHER_API_KEY")
        if not api_key:
            return (
                "环境变量中没有设置API键 'OPENWEATHER_API_KEY'."
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

	export const formatHandler = async () => {
		if (codeEditor) {
			return await codeEditor.formatPythonCodeHandler();
		}
		return false;
	};
</script>

<CodeEditor
	bind:value
	{boilerplate}
	bind:this={codeEditor}
	on:save={() => {
		dispatch('save');
	}}
/>
