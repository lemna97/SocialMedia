using Highever.SocialMedia.Common;
using System.Net.Http.Headers;
using System.Text.Json;



namespace Highever.SocialMedia.OpenAI.Services
{

    public class ChatGPTService : IChatGPTService
    {
        private readonly HttpClientHelper _httpClientHelper;
        private readonly string _apiToken;

        public ChatGPTService(HttpClientHelper httpClientHelper, string apiToken)
        {
            _httpClientHelper = httpClientHelper;
            _apiToken = apiToken;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="messages">对话消息列表</param>
        /// <returns>GPT 的回复内容</returns>
        public async Task<string> GetChatGPTResponseAsync(List<dynamic> messages)
        {
            string apiUrl = AppSettingConifgHelper.ReadAppSettings("ChatGPT:apiUrl");
            string model = AppSettingConifgHelper.ReadAppSettings("ChatGPT:model");

            var request = new ChatCompletionRequest
            {
                Model = model,
                Messages = messages,
            };
            try
            {
                // 将请求对象序列化为 JSON
                string requestBody = JsonSerializer.Serialize(request);

                // 设置 Headers，包括 Authorization
                var headers = new Dictionary<string, string>
                {
                    { "Authorization", $"{_apiToken}"}
                };

                // 发送 POST 请求 并解析
                var chatResponse = await _httpClientHelper.PostAsync<ChatCompletionResponse>(apiUrl, requestBody, headers);
                return chatResponse?.Choices.FirstOrDefault()?.Message.Content ?? "No response from ChatGPT.";
            }
            catch (Exception ex)
            {
                throw new Exception($"调用 ChatGPT 接口失败: {ex.Message}", ex);
            }
        } 

    }

}
