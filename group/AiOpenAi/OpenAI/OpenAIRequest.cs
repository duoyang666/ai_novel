using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace FY.Common.Ai.OpenAI
{
    public class OpenAIRequest
    {
        // 必须的参数
        [JsonPropertyName("model")]
        public string Model { get; set; } // 例如 "gpt-3.5-turbo"

        [JsonPropertyName("messages")]
        public List<OpenAIChatMessage> Messages { get; set; } // 聊天消息列表，每条消息包含角色和内容

        [JsonPropertyName("prompt")]
        public string? Prompt { get; set; }


        // 可选参数
        [JsonPropertyName("max_tokens")]
        public int max_tokens { get; set; } = 4096; // 生成的tokens最大数量

        [JsonPropertyName("temperature")]
        public float Temperature { get; set; } = 0.8f; // 介于0和2之间的浮点数，较高的值会导致更多的随机性

        [JsonPropertyName("top_p")]
        public float top_p { get; set; } = 0.8f; // 可替代Temperature，取值在0到1之间，指生成结果中top-k tokens的概率之和

        /// <summary>
        ///抽样候选集的大小。例如，当设置为50时，只有前50个令牌
        ///将考虑抽样。此字段是可选的。较大的值增加随机性；
        ///较小的值增加决定论。注意：如果top_k为null或大于100，
        ///不使用top_k策略，只有top_p有效。默认值为null。
        /// </summary>
        [JsonPropertyName("top_k")]
        public int? top_k { get; set; }

        [JsonPropertyName("seed")]
        public int? Seed { get; set; }

        [JsonPropertyName("stream")]
        public bool Stream { get; set; } // "true"或"false"，表示是否以流式方式返回结果，默认为"false"

        [JsonPropertyName("stop")]
        public List<string> Stop { get; set; } // 一个token列表，当生成的文本中包含这些token时，生成将停止

        [JsonPropertyName("user_id")]
        public string? UserId { get; set; }

    }

    public class OpenAIChatMessage
    {
        [JsonPropertyName("role")]
        public string Role { get; set; } // "system", "user", 或 "assistant"

        [JsonPropertyName("content")]
        public string Content { get; set; } // 消息内容

        [JsonPropertyName("quote")]
        public string? Quote { get; set; } // 回答的引用

        [JsonPropertyName("contents")]
        public IList<OpenAIFileContent>? Contents { get; set; }
    }

    public class OpenAIFileContent
    {
        /// <summary>
        /// 消息内容类型 text image_url image
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; }

        /// <summary>
        /// 消息内容类型为 text 时候的赋值，如：图片上描述了什么
        /// </summary>
        [JsonPropertyName("text")]
        public string Text { get; set; }

        /// <summary>
        /// 消息内容类型为 image_url 时候的赋值
        /// </summary>
        [JsonPropertyName("image_url")]
        public ImageUrl? ImageUrl { get; set; }

        /// <summary>
        /// 创建文本类消息
        /// <param name="text">文本内容</param>
        /// </summary>
        public static OpenAIFileContent CreateTextContent(string text)
        {
            return new()
            {
                Type = "text",
                Text = text
            };
        }

        /// <summary>
        /// 创建图片类消息，图片url形式
        /// <param name="imageUrl">图片 url</param>
        /// <param name="detail">指定图像的详细程度。通过控制 detail 参数（该参数具有三个选项： low 、 high 或 auto ），您
        /// 可以控制模型的处理方式图像并生成其文本理解。默认情况下，模型将使用 auto 设置，
        /// 该设置将查看图像输入大小并决定是否应使用 low 或 high 设置。</param>
        /// </summary>
        public static OpenAIFileContent CreateImageUrlContent(string imageUrl, string? detail = "auto")
        {
            return new()
            {
                Type = "image_url",
                ImageUrl = new ImageUrl()
                {
                    Url = imageUrl,
                    //Detail = detail
                }
            };
        }

        /// <summary>
        /// 创建图片类消息,字节流转base64字符串形式
        /// <param name="binaryImage">The image binary data as byte array</param>
        /// <param name="imageType">图片类型，如 png,jpg</param>
        /// <param name="detail">指定图像的详细程度。</param>
        /// </summary>
        public static OpenAIFileContent CreateImageBinaryContent(
            byte[] binaryImage,
            string imageType,
            string? detail = "auto"
        )
        {
            return new()
            {
                Type = "image_url",
                ImageUrl = new ImageUrl()
                {
                    Url = string.Format(
                        "data:image/{0};base64,{1}",
                        imageType,
                        Convert.ToBase64String(binaryImage)
                    ),
                    //Detail = detail
                }
            };
        }
    }
    /// <summary>
    /// 图片消息内容对象
    /// </summary>
    public class ImageUrl
    {
        /// <summary>
        /// 图片的url地址，如：https://localhost/logo.jpg ，一般只支持 .png , .jpg .webp .gif
        /// 也可以是base64字符串,如：data:image/jpeg;base64,{base64_image}
        /// 要看底层平台具体要求 
        /// </summary>
        [JsonPropertyName("url")]
        public string Url { get; set; }

        ///// <summary>
        ///// 指定图像的细节级别。在愿景指南中了解更多信息。https://platform.openai.com/docs/guides/vision/low-or-high-fidelity-image-understanding
        ///// <para>
        ///// 指定图像的详细程度。通过控制 detail 参数（该参数具有三个选项： low 、 high 或 auto ），您
        ///// 可以控制模型的处理方式图像并生成其文本理解。默认情况下，模型将使用 auto 设置，
        ///// 该设置将查看图像输入大小并决定是否应使用 low 或 high 设置。
        ///// </para>
        ///// </summary>
        //[JsonPropertyName("detail")]
        //public string? Detail { get; set; } = "auto";

    }


    public class OpenAIEmbeddings
    {
        [JsonPropertyName("model")]
        public string Model { get; set; }

        public List<string> Input { get; set; }
    }

}
