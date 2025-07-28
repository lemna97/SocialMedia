using System.Text.Json.Serialization;

namespace Highever.SocialMedia.OpenAI
{
    public class ChatCompletionRequest
    {
        [JsonPropertyName("model")]
        public string Model { get; set; } = "gpt-4o-2024-11-20"; // 默认使用 GPT-4 模型

        [JsonPropertyName("messages")]
        public List<object> Messages { get; set; }

        //[JsonPropertyName("temperature")]
        //public double Temperature { get; set; } = 0.7; // 可选参数

        //[JsonPropertyName("max_tokens")]
        //public int MaxTokens { get; set; } = 15000; // 可选参数，限制生成的最大 tokens 数量
    }

    #region 图文GPT请求模型 
    public class ChatMessage
    {
        [JsonPropertyName("role")]
        public string Role { get; set; } // e.g., "user"

        [JsonPropertyName("content")]
        public List<object> Content { get; set; } // 可以包含文本或图片内容
    }
    public class SystemMessage
    {
        [JsonPropertyName("role")]
        public string Role { get; set; } // e.g., "user"

        [JsonPropertyName("content")]
        public string Content { get; set; } // 可以包含文本或图片内容
    }
    public class ContentItem
    {
        [JsonPropertyName("type")]
        public string type { get; set; }

        [JsonPropertyName("text")]
        public string text { get; set; }
    }
    public class ContentImageItem
    {
        [JsonPropertyName("type")]
        public string type { get; set; }

        [JsonPropertyName("image_url")]
        public ImageUrl image_url { get; set; }
    }

    public class ImageUrl
    {
        [JsonPropertyName("url")]
        public string url { get; set; }

        //[JsonPropertyName("detail")]
        //public string detail { get; set; } = "high";
    }


    #endregion 
    #region  图文GPT响应结果

    public class ChatCompletionResponse
    {
        [JsonPropertyName("choices")]
        public List<Choice> Choices { get; set; } = new();

        public class Choice
        {
            [JsonPropertyName("message")]
            public Message Message { get; set; }
        }
    }
    public class Message
    {
        [JsonPropertyName("role")]
        public string Role { get; set; }
        
        [JsonPropertyName("content")]
        public string Content { get; set; }
    }
    #endregion

}
