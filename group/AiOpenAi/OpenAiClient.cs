using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;

namespace FY.Common.Ai;

/// <summary>
/// OpenAiClient
/// </summary>
public class OpenAiClient : IDisposable
{
    public readonly HttpClient HttpClient = null!;
    public string BaseUrl { get; set; }

    [SetsRequiredMembers]
    public OpenAiClient(string apiKey, string baseUri = "https://api.openai.com/v1", HttpClient? httpClient = null)
    {
        var handler = new HttpClientHandler
        {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
            UseDefaultCredentials = true,
            DefaultProxyCredentials = CredentialCache.DefaultCredentials,
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true,
        };
        BaseUrl = baseUri;
        HttpClient = httpClient ?? new HttpClient(handler);
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        ConfigureClient();

    }
    private void ConfigureClient()
    {
        // 模拟浏览器
        HttpClient.DefaultRequestHeaders.Add("User-Agent",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/130.0.0.0 Safari/537.36");
    }

    internal static async Task<T> ReadResponse<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (!response.IsSuccessStatusCode)
        {
            var debug = await response.Content.ReadAsStringAsync();
            Console.WriteLine(debug);
            return default;
        }

        try
        {
            var debug = await response.Content.ReadAsStringAsync();
            //Console.WriteLine(debug);
            return (await response.Content.ReadFromJsonAsync<T>(options: null, cancellationToken))!;
        }
        catch (Exception e) when (e is NotSupportedException or JsonException)
        {
            Console.WriteLine($"无法将以下json转换为: {typeof(T).Name}: {await response.Content.ReadAsStringAsync()}", e);
            return default;
        }
    }

    public async Task<T> Chat<T>(string req, CancellationToken cancellationToken = default)
    {
        var debug = "";
        try
        {
            debug = await ChatStr(req, cancellationToken);
            //Console.WriteLine(debug);
            return JsonSerializer.Deserialize<T>(debug);
        }
        catch (Exception e) when (e is NotSupportedException or JsonException)
        {
            Console.WriteLine($"无法将以下json转换为: {typeof(T).Name}: {debug}", e);
            return default;
        }
    }
    public async Task<string> ChatStr(string req, CancellationToken cancellationToken = default)
    {
        HttpRequestMessage httpRequest = new(HttpMethod.Post, @$"{BaseUrl}/chat/completions")
        {
            Content = new StringContent(req, System.Text.Encoding.UTF8, "application/json")
        };
        //CCC.InLogDebug(req);
        HttpResponseMessage response = await HttpClient.SendAsync(httpRequest, cancellationToken);
        try
        {
            // 检查是否被地区限制
            if (response.StatusCode == HttpStatusCode.Forbidden ||
                response.StatusCode == HttpStatusCode.ServiceUnavailable)
            {
                Console.WriteLine("地区被限制");
            }
            if (!response.IsSuccessStatusCode)
            {
                var debug = await response.Content.ReadAsStringAsync();
                return debug;
            }
            var debug2 = await response.Content.ReadAsStringAsync();
            return debug2;
        }
        catch (Exception e) when (e is NotSupportedException or JsonException)
        {
            return e.Message;
        }
        finally
        {
            HttpClient.Dispose();
        }
    }

    public async IAsyncEnumerable<T> ChatStreamed<T>(string req,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var res = ChatStreamedToStr(req, cancellationToken);
        await foreach (var data in res)
        {
            if (data.StartsWith("{\"code\":"))
            {
                Console.WriteLine(data);
                yield return default;
            }
            yield return JsonSerializer.Deserialize<T>(data)!;
        }
    }
    public async IAsyncEnumerable<string> ChatStreamedToStr(
        string request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        // 创建 HTTP 请求
        var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/chat/completions")
        {
            Content = new StringContent(request, Encoding.UTF8, "application/json")
        };

        httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));
        httpRequest.Headers.TryAddWithoutValidation("X-DashScope-SSE", "enable");
        httpRequest.Headers.Connection.Add("keep-alive");

        // 发送请求并保持响应对象存活
        HttpResponseMessage response = null;
        Stream streamResponse = null;
        StreamReader reader = null;

        try
        {
            response = await HttpClient.SendAsync(
                httpRequest,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);

            // 检查响应状态
            if (response.StatusCode == HttpStatusCode.Forbidden ||
                response.StatusCode == HttpStatusCode.ServiceUnavailable)
            {
                yield return "地区被限制";
                yield break;
            }

            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                Console.WriteLine("错误：" + errorMessage);
                yield return errorMessage;
                yield break;
            }

            // 获取响应流
            streamResponse = await response.Content.ReadAsStreamAsync();
            reader = new StreamReader(streamResponse, Encoding.UTF8);

            // 处理流数据
            while (!reader.EndOfStream)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                string? line = await reader.ReadLineAsync();
                if (string.IsNullOrEmpty(line)) continue;
                if (line == ": keep-alive") continue;

                if (line.StartsWith("data:"))
                {
                    yield return line["data:".Length..];
                }
                else if (line.StartsWith("{\"choices\":"))
                {
                    yield return line;
                }
                else
                {
                    Console.WriteLine(line);
                    yield return line;
                }
            }
        }
        finally
        {
            // 确保资源正确释放
            reader?.Dispose();
            streamResponse?.Dispose();
            response?.Dispose();
            httpRequest?.Dispose();
            HttpClient.Dispose();
        }
    }

    public void Dispose() => HttpClient.Dispose();
}
