
namespace start.Default
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using System.Linq;
    using System.Text.Json;
    using FY.Common.Ai;
    using FY.Common.Ai.OpenAI;
    using System.Text;

    public class AiAgentConfig
    {
        public string Id { get; set; }
        public string Model { get; set; }
        public string Name { get; set; }
        public string ApiEndpoint { get; set; }
        public string ApiKey { get; set; }
        public string SystemPrompt { get; set; }
        public string[] Keywords { get; set; }
        public float ResponseWeight { get; set; } = 1.0f;
        public Dictionary<string, string> CustomHeaders { get; set; } = new();
        public List<OpenAIChatMessage> History { get; set; } = new List<OpenAIChatMessage>();
        public int MaxHistory { get; set; } = 5;
        public float CapacityThreshold { get; set; } = 0.7f;
    }

    public class AiDispatcher
    {
        private readonly List<AiAgent> _agents;

        public AiDispatcher(IEnumerable<AiAgentConfig> configs)
        {
            _agents = configs.Select(c => new AiAgent(c)).ToList();
        }

        public async Task<List<ChatResponse>> ProcessMessageAsync(string message)
        {
            // 第一阶段：快速筛选
            var candidates = _agents
                .Where(a => a.ShouldRespond(message))
                .ToList();

            // 第二阶段：智能排序
            var scoredAgents = await Task.WhenAll(
                candidates.Select(async a => new
                {
                    Agent = a,
                    Score = await a.CalculateRelevanceAsync(message)
                })
            );

            // 选择Top3响应者
            //.Where(c => c.Score > c.Agent.Config.CapacityThreshold)
            var selected = scoredAgents
                .OrderByDescending(x => x.Score)
                .Take(3)
                .Select(x => x.Agent);

            // 并行获取响应
            var responses = await Task.WhenAll(
                selected.Select(a => a.GenerateResponseAsync(message))
            );

            return responses.ToList();
        }
    }

    public class AiAgent
    {
        public AiAgentConfig Config { get; }
        public AiAgent(AiAgentConfig config)
        {
            Config = config;
        }

        /// <summary>
        /// 关键词匹配
        /// </summary>
        public bool ShouldRespond(string message)
        {
            // 关键词匹配 + 容量检查
            return Config.Keywords.Any(k =>
                message.Contains(k, StringComparison.OrdinalIgnoreCase))
                || new Random().NextDouble() < Config.CapacityThreshold;
        }

        public async Task<float> CalculateRelevanceAsync(string message)
        {
            try
            {
                int i = 0;
                StringBuilder text = new StringBuilder();
                Config.History.ForEach(c =>
                {
                    i++;
                    if (i < Config.MaxHistory)
                    {
                        if (c.Role == "user") text.AppendLine("用户：\n" + c.Content);
                        if (c.Role == "assistant") text.AppendLine("回答：\n" + c.Content);
                        text.AppendLine();
                    }
                });

                string test = "{\r\n    \"Reasoning\": \"评估理由(中文)\",\r\n    \"KeywordScore\": 0.0,\r\n    \"ExpertiseScore\": 0.0,\r\n    \"ContextScore\": 0.0,\r\n    \"PriorityBoost\": 0.0\r\n}";
                // 构建结构化提示词
                string evaluationPrompt = $"""
【智能路由评估任务】
请根据以下信息评估AI代理对当前消息的响应适合度：

<代理配置>
ID: {Config.Id}
名称: {Config.Name}
系统角色: {Config.SystemPrompt}
关键词: {string.Join(", ", Config.Keywords)}
</代理配置>

<对话上下文>
{text}
</对话上下文>

<当前消息>
{message}
</当前消息>

【评估维度】
请按以下标准给出0.0-1.0的评分：
1. 关键词匹配度 (0.3权重)
2. 领域专业度 (0.4权重)
3. 上下文连贯性 (0.2权重)
4. 优先级加成 (0.1权重)

【输出格式要求】
{test}
""";
                var request = new OpenAIRequest
                {
                    Model = Config.Model,
                    Messages = new List<OpenAIChatMessage>()
                    {
                        new OpenAIChatMessage { Role = "user", Content = evaluationPrompt }
                    },
                    Stream = false,
                    Temperature = 0.2f,
                };
                var response = ComAiOpenAi.Use(Config.ApiKey, Config.ApiEndpoint, request);
                await foreach (var item in response)
                {
                    return ProcessEvaluationResponse(item);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return 0f;
        }

        // 辅助方法：处理评估响应
        private float ProcessEvaluationResponse(string jsonResponse)
        {
            try
            {
                using var doc = JsonDocument.Parse(jsonResponse);
                var result = doc.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content");

                var scores = JsonSerializer.Deserialize<EvaluationScores>(result.ToString());
                var total = (scores.KeywordScore * 0.3f +
                       scores.ExpertiseScore * 0.4f +
                       scores.ContextScore * 0.2f +
                       scores.PriorityBoost * 0.1f) *
                       Config.ResponseWeight;
                //Console.WriteLine($"评估结果：{total}");
                return total;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return 0f;
            }
        }

        // 评估结果数据结构
        private record EvaluationScores(
            string Reasoning,
            float KeywordScore,
            float ExpertiseScore,
            float ContextScore,
            float PriorityBoost);

        /// <summary>
        /// 开始请求
        /// </summary>
        public async Task<ChatResponse> GenerateResponseAsync(string message)
        {
            try
            {
                var request = new OpenAIRequest
                {
                    Model = Config.Model,
                    Messages = new List<OpenAIChatMessage>()
                    {
                        new OpenAIChatMessage { Role = "system", Content = Config.SystemPrompt },
                        new OpenAIChatMessage { Role = "user", Content = message }
                    },
                    Stream = true
                };
                var response = ComAiOpenAi.Use(Config.ApiKey, Config.ApiEndpoint, request);
                return new ChatResponse(Config.Id, response);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new ChatResponse(Config.Id, GetStringsAsync());
            }
        }
        public static async IAsyncEnumerable<string> GetStringsAsync()
        {
            yield return "[错误]";
        }

        public static string ToStr(string jsonStr)
        {
            try
            {
                if (string.IsNullOrEmpty(jsonStr)) return "";
                if (jsonStr.Trim() == "[DONE]") return jsonStr.Trim();
                var obj = JsonSerializer.Deserialize<OpenAIResponse>(jsonStr);
                string str = obj?.choices?[0].message?.content;
                if (string.IsNullOrEmpty(str))
                    str = obj?.choices?[0].delta?.content;
                if (string.IsNullOrEmpty(str))
                    str = obj?.choices?[0].delta?.reasoning_content;
                if (string.IsNullOrEmpty(str) && obj?.choices != null)
                    return "[DONE]";
                return str;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }

    public record ChatResponse(string AgentId, IAsyncEnumerable<string> Res);
}
