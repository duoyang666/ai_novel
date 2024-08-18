import { browser, dev } from '$app/environment';
import { is_connect } from './apis/net';
// import { version } from '../../package.json';
import { writable } from 'svelte/store';


// 1. 创建一个类型为number的writable store
const count = writable(0);
// 2. 订阅store，获取当前值
count.subscribe(value => {
	console.log(`当前计数为: ${value}`);
});
// 3. 更新store的值
count.set(1); // 将计数设置为1
// 4. 增加计数
count.update(n => n + 1); // 将计数增加1
// 5. 监听store的变化
const unsubscribe = count.subscribe(value => {
	console.log(`当前计数为: ${value}`);
});
// 取消订阅
unsubscribe();

//net的服务端
export const NetSettings = writable({
	net_host: dev ? `http://${location.hostname}:8888` : location.origin,
	net_service: false,
	net_service_is_true: false,
	model: { "is_net": false },//选中的模型

});
export function getNetSettings() {
	let obj;
	NetSettings.subscribe(value => {
		obj = value;
	});
	return obj;
}
export const NetApi = {
	is_connect: async () => {
		let obj = getNetSettings();
		// if (obj.model && obj.model['is_net']) {
		// 	obj['net_service'] = true;
		// } else {
		// 	obj['net_service'] = false;
		// }
		// if (obj.model && obj.model['info'] && obj.model['info']['base_model_id'] == 'duoyang.net') {
		// 	obj['net_service'] = true;
		// } else {
		// 	obj['net_service'] = false;
		// }

		obj['net_service'] = true;
		console.log(obj)
		if (obj['net_service']) {
			let res = await is_connect();
			if (res) {
				obj['net_service_is_true'] = true;
				obj['net_service'] = true;
			}
			else {
				obj['net_service_is_true'] = false;
				obj['net_service'] = false;
			}
		}
		NetSettings.set({ ...obj });


	},
	toggle_connect: async () => {
		let obj = getNetSettings();
		obj['net_service'] = !obj['net_service']
		NetSettings.set({ ...obj });
		await NetApi.is_connect();
	}
};
export const net_service = writable(false);

export const APP_NAME = writable('Open WebUI');
// let WEBUI_HOSTNAME2 = browser ? (dev ? `${location.hostname}:8081` : ``) : '';
// let WEBUI_BASE_URL2 = browser ? (dev ? `http://${WEBUI_HOSTNAME2}` : ``) : ``;


let WEBUI_HOSTNAME2 = `${location.hostname}:8080`;
let WEBUI_BASE_URL2 = `http://${WEBUI_HOSTNAME2}`;

export const WEBUI_HOSTNAME = WEBUI_HOSTNAME2;
export const WEBUI_BASE_URL = WEBUI_BASE_URL2;
export const WEBUI_API_BASE_URL = `${WEBUI_BASE_URL2}/api/v1`;

export const OLLAMA_API_BASE_URL = `${WEBUI_BASE_URL2}/ollama`;
export const OPENAI_API_BASE_URL = `${WEBUI_BASE_URL2}/openai`;
export const AUDIO_API_BASE_URL = `${WEBUI_BASE_URL2}/audio/api/v1`;
export const IMAGES_API_BASE_URL = `${WEBUI_BASE_URL2}/images/api/v1`;
export const RAG_API_BASE_URL = `${WEBUI_BASE_URL2}/rag/api/v1`;

export const WEBUI_OBJ = {
	WEBUI2_HOSTNAME: WEBUI_HOSTNAME,
	WEBUI2_BASE_URL: WEBUI_BASE_URL,
	WEBUI2_API_BASE_URL: WEBUI_API_BASE_URL,
	OLLAMA2_API_BASE_URL: OLLAMA_API_BASE_URL,
	OPENAI2_API_BASE_URL: OPENAI_API_BASE_URL,
	AUDIO2_API_BASE_URL: AUDIO_API_BASE_URL,
	IMAGES2_API_BASE_URL: IMAGES_API_BASE_URL,
	RAG2_API_BASE_URL: RAG_API_BASE_URL,
	SEARCH2_URL: "https://cn.bing.com/search?q=",
}

export const WEBUI_VERSION = APP_VERSION;
export const WEBUI_BUILD_HASH = APP_BUILD_HASH;
export const REQUIRED_OLLAMA_VERSION = '0.1.16';

export const SUPPORTED_FILE_TYPE = [
	'application/epub+zip',
	'application/pdf',
	'text/plain',
	'text/csv',
	'text/xml',
	'text/html',
	'text/x-python',
	'text/css',
	'application/vnd.openxmlformats-officedocument.wordprocessingml.document',
	'application/octet-stream',
	'application/x-javascript',
	'text/markdown',
	'audio/mpeg',
	'audio/wav'
];

export const SUPPORTED_FILE_EXTENSIONS = [
	'md',
	'rst',
	'go',
	'py',
	'java',
	'sh',
	'bat',
	'ps1',
	'cmd',
	'js',
	'ts',
	'css',
	'cpp',
	'hpp',
	'h',
	'c',
	'cs',
	'htm',
	'html',
	'sql',
	'log',
	'ini',
	'pl',
	'pm',
	'r',
	'dart',
	'dockerfile',
	'env',
	'php',
	'hs',
	'hsc',
	'lua',
	'nginxconf',
	'conf',
	'm',
	'mm',
	'plsql',
	'perl',
	'rb',
	'rs',
	'db2',
	'scala',
	'bash',
	'swift',
	'vue',
	'svelte',
	'doc',
	'docx',
	'pdf',
	'csv',
	'txt',
	'xls',
	'xlsx',
	'pptx',
	'ppt',
	'msg'
];

// Source: https://kit.svelte.dev/docs/modules#$env-static-public
// This feature, akin to $env/static/private, exclusively incorporates environment variables
// that are prefixed with config.kit.env.publicPrefix (usually set to PUBLIC_).
// Consequently, these variables can be securely exposed to client-side code.
