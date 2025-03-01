using System.Text.Json.Serialization;

namespace FY.Common.Ai.OpenAI
{
    public class OpenAIResponse
    {
        public string id { get; set; }
        [JsonPropertyName("object")]
        public string obj { get; set; }
        public long created { get; set; }
        public string model { get; set; }
        public string system_fingerprint { get; set; }
        public ChatUsage? usage { get; set; }
        public List<ChoicesModel> choices { get; set; }
    }
    public class ChoicesModel
    {
        public string finish_reason { get; set; } = "";
        public int index { get; set; } = 0;

        public OpenAIMessage delta { get; set; }
        public OpenAIMessage message { get; set; }
    }
    public class OpenAIMessage
    {
        public string role { get; set; }

        public string content { get; set; }

        public string reasoning_content { get; set; }
    }
    public class OpenAIEmbeddingsRes
    {
        [JsonPropertyName("object")]
        public string Object { get; set; } = "list";

        public List<EmbeddingsData> data { get; set; } = new List<EmbeddingsData>();

        public string model { get; set; }

        public ChatUsage usage { get; set; } = new ChatUsage();
    }

    public class ChatUsage
    {
        /// <summary>
        /// Output token count of generated text.
        /// </summary>
        [JsonPropertyName("completion_tokens")]
        public int OutputTokens { get; set; }

        /// <summary>
        /// Input token count of messages.
        /// </summary>
        [JsonPropertyName("prompt_tokens")]
        public int InputTokens { get; set; }

        /// <summary>
        /// 总Token数
        /// </summary>
        [JsonPropertyName("total_tokens")]
        public int TotalTokens { get; set; }
    }
    public class EmbeddingsData
    {
        [JsonPropertyName("object")]
        public string Object { get; set; } = "embedding";

        public List<double> embedding { get; set; } = new List<double>();

        public int index { get; set; }
    }
}
