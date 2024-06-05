# AI拆书
# 1，把全本小说按照章节，分割成一个个文本。
# 2，定义文件夹功能

import time
from datetime import datetime
import shutil
import os
import re
from langchain.chains import LLMChain
from langchain_openai import ChatOpenAI
from langchain.prompts import (
    ChatPromptTemplate,
    MessagesPlaceholder,
    SystemMessagePromptTemplate,
    HumanMessagePromptTemplate,
    AIMessagePromptTemplate,
)


def detect_encoding(file_path):
    """
        判断文件编码
    """
    # import chardet
    # with open(file_path, 'rb') as f:
    #     while True:
    #         line = f.readline()
    #         if (len(line) > 5):
    #             encoding = chardet.detect(line)['encoding']
    #             return encoding
    return 'utf-8'


def create_or_append_file(file_path, content, mode='a'):
    """
    创建文件或在文件已存在时追加内容。

    :param file_path: 文件路径
    :param content: 要写入的内容
    :param mode: 打开文件的模式，默认为 'a'（追加模式）
    """
    file_path = os.path.abspath(file_path)

    # 确保文件所在的目录存在，如果不存在则创建
    os.makedirs(os.path.dirname(file_path), exist_ok=True)

    # 使用'with'语句确保文件最终会被关闭
    with open(file_path, mode, encoding='utf-8') as file:
        file.write(content)
        # 如果需要在追加内容后立即保存到文件，可以调用flush()方法
        file.flush()


def create_folder(folder_path):
    """
    创建文件夹，如果文件夹已存在，则不执行任何操作。

    :param folder_path: 文件夹路径
    """
    # 使用exist_ok=True，如果文件夹已存在，不会抛出异常
    folder_path = os.path.abspath(folder_path)
    os.makedirs(folder_path, exist_ok=True)


def get_filename_and_extension(file_path):
    """
    从给定的文件路径中提取文件名和后缀。

    :param file_path: 文件的完整路径
    :return: 文件名和后缀的元组
    """
    # 获取文件的基本名称（不包含目录）
    base_name = os.path.basename(file_path)
    # 分离文件名和后缀
    file_name, file_extension = os.path.splitext(base_name)

    # os.path.splitext会保留点（.），所以如果需要可以去掉
    file_extension = file_extension.lstrip('.')
    return file_name, file_extension


def delete_directory_and_contents(dir_path):
    try:
        # 使用rmtree删除目录及其所有内容
        shutil.rmtree(dir_path)
        print(f"目录及其所有内容已被删除: {dir_path}")
    except FileNotFoundError:
        # 如果目录不存在，不执行任何操作
        print(f"目录不存在，无需删除: {dir_path}")
    except Exception as e:
        # 处理其他可能的异常
        print(f"删除目录时发生错误: {e}")


def load_file(file_path, isArray=False, encoding='utf-8'):
    """
    加载文件
    """
    file_path = os.path.abspath(file_path)
    # 检查文件是否存在
    if not os.path.exists(file_path):
        return []

    encoding = detect_encoding(file_path)
    with open(file_path, 'r', encoding=encoding) as f:
        if isArray:
            return f.readlines()
        else:
            return ''.join(f.readlines())


def get_files(dir_path):
    """
    加载文件夹并返回该文件夹下全部的文件路径。

    :param dir_path: 文件夹的路径
    :return: 文件夹下所有文件的路径列表
    """
    # 检查目录是否存在
    if not os.path.exists(dir_path):
        print(f"目录不存在: {dir_path}")
        return []
    # 获取目录下的所有文件和文件夹
    dir_path = os.path.abspath(dir_path)
    entries = os.listdir(dir_path)
    # 过滤出文件，并构建完整的文件路径
    file_paths = [os.path.join(dir_path, entry) for entry in entries if os.path.isfile(
        os.path.join(dir_path, entry))]
    return file_paths


def spile_file(file_path, dir_name):
    """
    分割章节到文件
    """
    file_path = os.path.abspath(file_path)
    encoding = detect_encoding(file_path)

    path = '.'
    if dir_name:
        path = f'./{dir_name}'

    with open(file_path, 'r', encoding=encoding) as f:
        zj_w = ''
        nr_w = []
        file_name, file_extension = get_filename_and_extension(file_path)
        if file_name:
            delete_directory_and_contents(f'{path}/{file_name}')
            create_folder(f'{path}/{file_name}')

        i = 0
        for line in f:
            line = line.strip()
            if (line != ''):
                zj = re.search(
                    r'^(.*?)第(\d+|[一二三四五六七八九十百千]+)[章节段][：:]?\s+[\u4e00-\u9fa5]+.*$', line)
                if (zj is None):
                    zj = re.search(
                        r'^[0-9一二三四五六七八九十百千零]+[、，,  ：:].*$', line)

                if (zj is not None and len(line) < 60):
                    if zj_w:
                        zj_h = zj_w
                        pattern = r'(第[\d一二三四五六七八九十百千万]+[章节段](?:\s+[\\u4e00-\\u9fa5]+)*)'
                        match = re.search(pattern, zj_h)
                        if match:
                            title = match.group(1)
                            zj_h = zj_h[zj_w.index(title) + len(title):]
                            zj_h = zj_h.strip()

                        i = i+1
                        zj_str = f''.join(nr_w)
                        if not zj_str:
                            continue

                        zj_str = f'此片段的小说名称是：{file_name}，章节名称是：{zj_h}，第{i:04d}章\n\n'+zj_str
                        nr_w = []
                        create_or_append_file(
                            f'{path}/{file_name}/第{i:04d}章{zj_h}.txt', zj_str)

                    zj_w = re.sub(r'[^\w\s]+', '', line)
                else:
                    nr_w.append(str(line))
                    nr_w.append('\n')


def get_llm_open_ai(name="glm-3-turbo", key="", base="", temperature='0.95', max_tokens=1000):
    """
        获取llm ChatOpenAI
    """
    

    llama_model = ChatOpenAI(
        temperature=temperature,
        # model="glm-4",
        model=name,
        max_tokens=max_tokens,
        timeout=60*5,
        openai_api_key="49753110f6f1520ed3bcf3f4f2134736.mLuK9QFx9QzzsP49",
        openai_api_base="https://open.bigmodel.cn/api/paas/v4/"
    )
    return llama_model


def get_prompt(messages=[]):
    """
    """

    array = []
    for m in messages:
        if 'system' in m and m['system']:
            array.append(
                SystemMessagePromptTemplate.from_template(m['system']))
        if 'human' in m and m['human']:
            array.append(
                HumanMessagePromptTemplate.from_template(m['human']))
        if 'ai' in m and m['ai']:
            array.append(
                AIMessagePromptTemplate.from_template(m['ai']))

    prompt = ChatPromptTemplate(
        messages=array,
    )
    return prompt


def llm_chain(llm, prompt, memory=None, verbose=False):
    
    chain = LLMChain(llm=llm, prompt=prompt, memory=memory, verbose=verbose)
    return chain


llm = get_llm_open_ai(max_tokens=1000*6)


def chai_load(file_path, messages=[], two=[], max_tokens=1000*6):
    if not os.path.exists(file_path):
        print('文件不存在')
        return ''

    # 分割章节
    spile_file(file_path, 'dataChai/data')
    file_name, file_extension = get_filename_and_extension(file_path)

    llm = get_llm_open_ai(max_tokens=max_tokens)
    cs_shu_fx = './dataChai/'+file_name
    cs_shu = './dataChai/data/'+file_name
    cs_files = get_files(cs_shu)

    i = 0
    j = 0
    s = 0
    prompt = get_prompt(messages=[*messages, {'human': '{input}'}])
    chain = llm_chain(llm=llm, prompt=prompt, verbose=False)
    for file_path in cs_files:
        try:
            s = s+1
            cs_yj = load_file(cs_shu_fx+'/已分析.txt', isArray=True)

            # 转换为相对路径
            current_working_directory = os.getcwd()
            relative_path = os.path.relpath(
                file_path, current_working_directory)
            if any(x.strip() == relative_path for x in cs_yj):
                continue

            res_text = ''
            cs_nr = load_file(file_path)
            res = chain.invoke({'input': cs_nr})
            res_text = res['text']

            # 再度对话 容易跑偏
            for t in two:
                res = chain.invoke({'input': t})
                res_text = res['text']
                print(res_text)

            if (int(s / 250) > 0):
                j = int(s / 250)

            cs_file = cs_shu_fx+'/时间线.txt'
            if j > 0:
                cs_file = f'{cs_shu_fx}/时间线{j}.txt'

            # 手动记录标题
            cs_name, cs_extension = get_filename_and_extension(file_path)
            create_or_append_file(
                cs_file, f'\n\n--开始分析：{cs_name}--\n' + res_text+'\n--结束--\n\n')

            relative_path = os.path.relpath(
                file_path, current_working_directory)
            create_or_append_file(cs_shu_fx+'/已分析.txt',
                                  '\n'+relative_path)
            print(f'完成({datetime.now().strftime("%H:%M:%S")})————{file_path}')

            i = i+1
            if (i > 20):
                i = 0

            time.sleep(1)
        except Exception as e:
            print(f"发生错误: {e}")


cs_jc = load_file('./basics/基础.txt')
cs_sj = load_file('./basics/时间拆分.txt')
cs_bz = load_file('./basics/写作步骤.txt')
cs_qj = load_file('./basics/情节创作.txt')
messages = [{'system': cs_jc}, {'system': cs_sj},
            {'system': cs_bz}, {'system': cs_qj},
            # {'system': """通过学习你已经是一个具有独特写作天赋的作家，擅长通过细致的描述和真实的对话来创造情节和描写角色，深入分析他们的情感深度。"""}
            ]

two = [
    # """你上次的回答，是分析后的小说的章节，通过学习你已经是一个具有独特写作天赋的作家，擅长通过细致的描述和真实的对话来分析情节和分析角色，深入分析他们的情感深度。
    # 现在你需要再次进行详细的分析，把人物变得更加的丰满，通过人物的性格和设定，分析出事件中的铺垫和伏笔，并且把剧情解析的更加的透彻。
    # 每个人物的设定和分析，不少于30个字。每个事件分析，不少于50个字。当前剧情分析，不少于200个字。
    # 继续按照上次的输出格式，再次分析。
    # # """,

    # """重复分析上次的提问。""",
]

files = get_files('./dataChai')
for file_path in files:
    file_size = os.path.getsize(file_path)
    if file_size > 1024*1024*30:
        print('超出限制的文本大小')

    chai_load(file_path, messages, two)
