import { getNetSettings } from '$lib/constants';
import type { Any } from '@vitest/expect';

// 封装的异步函数，用于发送请求
async function send(url, data, method = 'POST') {
	try {
		let token = ""
		// 准备请求头，如果 token 存在则添加到请求头中
		const headers = {
			Accept: 'application/json',
			'Content-Type': 'application/json',
			...(token && { authorization: `Bearer ${token}` })
		};

		// 发送请求
		const response = await fetch(url, {
			method: method,
			headers: headers,
			body: data ? JSON.stringify(data) : undefined
		});

		// 检查响应状态
		if (!response.ok) {
			// 如果响应状态码不是 2xx，抛出错误
			const errorData = await response.json();
			throw new Error(`Request failed with status ${response.status}: ${JSON.stringify(errorData)}`);
		}

		// 返回解析后的 JSON 数据
		return await response.json();
	} catch (error) {
		// 打印错误信息
		console.error('An error occurred:', error);
		// 根据需要处理错误，这里返回 null 或其他错误信息
		return null;
	}
}

//连接
export const is_connect = async () => {
	try {
		let token = ""
		let error = null;
		const res = await fetch(`${getNetSettings()['net_host']}/api/basis/connect`, {
			method: 'GET',
			headers: {
				Accept: 'application/json',
				'Content-Type': 'application/json',
				...(token && { authorization: `Bearer ${token}` })
			}
		}).then(async (res) => {
			if (!res.ok) throw await res.json();
			return res.json();
		}).catch((err) => {
			console.log(err);
			error = err;
			return null;
		});
		// console.log(res);
		return res;

		// let data = res?.data ?? [];
		// console.log(data);
		// return data;	
	} catch (error) {
		return "";
	}
};

//设置
export const get_settings = async (id: string = '') => {
	try {
		let res = await send(`${getNetSettings()['net_host']}/api/basis/get/settings?id=${id}`, null, 'GET');
		return res;
	} catch {
		return "127.0.0.1:8080";
	}
};
export const set_settings = async (config: object) => {
	try {
		let res = await send(`${getNetSettings()['net_host']}/api/basis/set/settings`, config, 'POST');
		return res;
	} catch {
		return "";
	}
};
//获取分组标签-字典
export const get_dics = async (group: string = '') => {
	try {
		let res = await send(`${getNetSettings()['net_host']}/api/dic/list?group=${group}`, null, 'GET');
		return res;
	} catch {
		return [];
	}
};

//知识库
//上传文档
export const uploadDocToDB = async (file: File, obj: Any) => {
	const data = new FormData();
	data.append('file', file);
	for (let o in obj) {
		data.append(o, obj[o]);
	}

	let token = ""
	let error = null;

	const res = await fetch(`${getNetSettings()['net_host']}/api/knowledge/upload`, {
		method: 'POST',
		headers: {
			Accept: 'application/json',
			authorization: `Bearer ${token}`
		},
		body: data
	}).then(async (res) => {
		if (!res.ok) throw await res.json();
		return res.json();
	}).catch((err) => {
		error = err.detail;
		console.log(err);
		return null;
	});

	if (error) {
		throw error;
	}

	return res;
};
//知识库列表
export const get_knowledges = async (id: string = '') => {
	try {
		let res = await send(`${getNetSettings()['net_host']}/api/knowledge/list`, null, 'GET');
		return res;
	} catch {
		return [];
	}
};
//知识库删除
export const del_knowledges = async (ids: string = '') => {
	try {
		let res = await send(`${getNetSettings()['net_host']}/api/knowledge/delete`, ids, 'POST');
		return res;
	} catch {
		return [];
	}
};
//知识库编辑
export const edit_knowledges = async (dic) => {
	try {
		let res = await send(`${getNetSettings()['net_host']}/api/knowledge/edit`, dic, 'POST');
		return res;
	} catch {
		return [];
	}
};
//知识库内容列表
export const get_knowledges_id = async (id: string = '', p: number = 0, n: number = 30) => {
	try {
		let res = await send(`${getNetSettings()['net_host']}/api/knowledge/id/list/${id}?p=${p}&n=${n}`, null, 'GET');
		return res;
	} catch {
		return [];
	}
};
//知识库内容搜索
export const get_knowledges_search = async (q: string = '', n: number = 10) => {
	try {
		let res = await send(`${getNetSettings()['net_host']}/api/knowledge/search?q=${q}&n=${n}`, null, 'GET');
		return res;
	} catch {
		return [];
	}
};
//知识库内容删除
export const del_knowledges_id = async (id: string = '') => {
	try {
		let res = await send(`${getNetSettings()['net_host']}/api/knowledge/id/delete/${id}`, null, 'GET');
		return res;
	} catch {
		return [];
	}
};
//知识库内容编辑
export const edit_knowledges_id = async (data) => {
	try {
		let res = await send(`${getNetSettings()['net_host']}/api/knowledge/id/edit`, data, 'POST');
		return res;
	} catch {
		return [];
	}
};

//对话
//在数据库中搜索
export const search_db = async (q: string = '', m: string = '') => {
	let token = ""
	let error = null;
	const res = await fetch(`${getNetSettings()['net_host']}/search/db/${q}?m=${m}`, {
		method: 'GET',
		headers: {
			Accept: 'application/json',
			authorization: `Bearer ${token}`
		},
	}).then(async (res) => {
		if (!res.ok) throw await res.json();
		return res.json();
	}).catch((err) => {
		error = err.detail;
		console.log(err);
		return null;
	});

	if (error) {
		throw error;
	}
	return res;
};
//在网络中搜索
export const search_web = async (q: string = '', m: string = '') => {
	let token = ""
	let error = null;
	const res = await fetch(`${getNetSettings()['net_host']}/search/web/${q}?m=${m}`, {
		method: 'GET',
		headers: {
			Accept: 'application/json',
			authorization: `Bearer ${token}`
		},
	}).then(async (res) => {
		if (!res.ok) throw await res.json();
		return res.json();
	}).catch((err) => {
		error = err.detail;
		console.log(err);
		return null;
	});

	if (error) {
		throw error;
	}
	return res;
};
