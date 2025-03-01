using FY.Common.Ai.OpenAI;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FY.Common.Ai
{
    public static class ComAiOpenAi
    {
        public async static IAsyncEnumerable<string> Use(string key, string baseUrl = "", OpenAIRequest request = null)
        {
            var options2 = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            };

            if (string.IsNullOrEmpty(baseUrl)) baseUrl = "https://api.openai.com/v1";
            using OpenAiClient c = new OpenAiClient(key, baseUrl);
            var req = new OpenAIRequest();
            if (request == null)
            {
                req.Model = "";
                req.Stream = request.Stream;
                req.Messages = new List<OpenAIChatMessage>();
                req.Messages.Add(new OpenAIChatMessage() { Role = "user", Content = "你是谁" });
            }
            else
            {
                req = request;
            }

            var mes2 = req.Messages.Where(c => c.Role == "user" && !string.IsNullOrEmpty(c.Content)).ToList();
            if (!mes2.Any())
            {
                yield return "";
            }
            string reqJson = JsonSerializer.Serialize(req, options2);
            //Console.WriteLine(reqJson);
            if (req.Stream)
            {
                var response = c.ChatStreamedToStr(reqJson);
                await foreach (var jsonStr in response)
                {
                    yield return jsonStr;
                }
                yield break;
            }
            else
            {
                string jsonStr = await c.ChatStr(reqJson);
                yield return jsonStr;
                yield break;
            }
        }
    }
}
