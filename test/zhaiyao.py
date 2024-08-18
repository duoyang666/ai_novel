# 阿里摘要


from http import HTTPStatus
from dashscope import Generation


# bailian-summary
def call_with_stream():
    messages = [
        {'role': 'user', 'content': '如何做西红柿炖牛腩？'}]
    responses = Generation.call("qwen-plus-0206",
                                messages=messages,
                                result_format='message',  # 设置输出为'message'格式
                                stream=True,  # 设置输出方式为流式输出
                                incremental_output=True,  # 增量式流式输出
                                api_key='sk-a668d031c59944139ec4bad640f5841e',
                                )
    for response in responses:
        if response.status_code == HTTPStatus.OK:
            print(response.output.choices[0]['message']['content'], end='')
        else:
            print('Request id: %s, Status code: %s, error code: %s, error message: %s' % (
                response.request_id, response.status_code,
                response.code, response.message
            ))


if __name__ == '__main__':
    call_with_stream()
