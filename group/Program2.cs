using start.Default;

namespace start
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            #region api群聊
            var agents = new[]
            {
                new AiAgentConfig
                {
                    Id = "security",
                    Model = "glm-4-flash",
                    Name = "安全专家",
                    ApiEndpoint = "https://open.bigmodel.cn/api/paas/v4",
                    ApiKey = "",
                    SystemPrompt = "你是一位网络安全专家，擅长识别和解决各种安全漏洞。用简洁的技术语言回答，包含具体解决方案示例。",
                    Keywords = new[] { "安全", "漏洞", "加密" },
                    MaxHistory = 3
                },
                new AiAgentConfig
                {
                    Id = "designer",
                    Model = "glm-4-flash",
                    Name = "创意设计师",
                    ApiEndpoint = "https://open.bigmodel.cn/api/paas/v4",
                    ApiKey = "",
                    SystemPrompt = "你是一位富有创意的设计师，用生动的比喻和视觉化语言回答问题。每个回答都要包含一个创意案例。",
                    Keywords = new[] { "设计", "创意", "用户体验" },
                }
            };
            var dispatcher = new AiDispatcher(agents);

            Console.Write("请输入问题：");
            var question = Console.ReadLine();

            var responses = await dispatcher.ProcessMessageAsync(question);
            Console.WriteLine("\n最佳回答：");
            foreach (var response in responses)
            {
                Console.WriteLine($"{response.AgentId}");
                await foreach (var item in response.Res)
                {
                    var str001 = AiAgent.ToStr(item);
                    Console.Write(str001);
                }
            }
            #endregion

            Console.WriteLine("结束");
            Console.Read();
        }
    }
}
