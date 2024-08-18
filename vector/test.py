# 文本文件向量化
# 实验文本向量化的速度

from langchain.embeddings import HuggingFaceEmbeddings
from langchain.vectorstores import Chroma
import time
import os


def chroma_vector_add(file_path, dir, c_name="vectordb", model_name="./m3e"):
    """
    文本文件向量化 不拆分会把全部内容插入到 提示词中 所以还是必须拆分
    """
    model_kwargs = {'device': 'cpu'}
    # encode_kwargs = {'normalize_embeddings': False}
    embeddings_hf = HuggingFaceEmbeddings(
        model_name=model_name, model_kwargs=model_kwargs)
    if (not os.path.exists(dir)):
        os.makedirs(dir, exist_ok=True)

    start_time = time.time()

    # txt读取
    from langchain.document_loaders import TextLoader
    docs_txt = ''
    try:
        loader_txt = TextLoader(r''+file_path, encoding='utf-8')
        docs_txt = loader_txt.load()
    except Exception as e:
        try:
            loader_txt = TextLoader(r''+file_path, encoding='gbk')
            docs_txt = loader_txt.load()
        except Exception as e:
            print(f"编码错误（{file_path}）: {e}")
            return

    # 导入文本分割器
    from langchain.text_splitter import RecursiveCharacterTextSplitter
    text_splitter_txt = RecursiveCharacterTextSplitter(
        chunk_size=500,
        chunk_overlap=0,
        separators=["\n", " ", "。", "，"],
    )
    documents_txt = text_splitter_txt.split_documents(docs_txt)

    # 注意from_documents每次运行都是把数据添加进去
    vectordb = Chroma.from_documents(documents=documents_txt, collection_name=c_name,
                                     embedding=embeddings_hf, persist_directory=dir)
    vectordb.persist()  # 持久化更改到磁盘
    end_time = time.time()
    run_time = end_time - start_time
    print(f"量化时间：{run_time} 秒,{int(run_time/60)} 分钟。{file_path}")


# # 215KB 量化时间：50.44206881523132 秒,0 分钟。
# chroma_vector_add("vector/白马啸西风.txt", "vector/215KB",
#                   model_name="D:\code\python\model\m3e")

# # 431KB 量化时间：92.91256904602051 秒,1 分钟。
# chroma_vector_add("vector/431KB.txt", "vector/431KB",
#                   model_name="D:\code\python\model\m3e")

# # 862KB 量化时间：186.25255155563354 秒,3 分钟。
# chroma_vector_add("vector/862KB.txt", "vector/862KB",
#                   model_name="D:\code\python\model\m3e")

# # 2.65MB 量化时间：664.8874490261078 秒,11 分钟。
# chroma_vector_add("vector/射雕英雄传.txt", "vector/2.65MB",
#                   model_name="D:\code\python\model\m3e")


# 实验搜索
embeddings_hf = HuggingFaceEmbeddings(model_name="D:\code\python\model\m3e")
vectordb_load = Chroma(
    persist_directory="vector/215KB",
    embedding_function=embeddings_hf,
    collection_name="vectordb"
)
docs = vectordb_load.similarity_search("打起来了")
print(docs)
