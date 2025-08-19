using Highever.SocialMedia.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Volvo.Release
{
    public class RequestModel
    {
        public string ProxyIp { get; set; }
        public string ProxyUser { get; set; }
        public string ProxyPassword { get; set; }
        public string UserAgent { get; set; }
        public CookieContainer container { get; set; }
    }
    /// <summary>
    /// 
    /// </summary>
    public class ChatGPTHelper
    {
        private const  string _authorization = "Bearer sk-U1I0ecd55b86f2c6dc0b05750e567f6c8a436918d8a6x0Rl";
        public async Task<string> Chat(string prompt, string image)
        {
            string res = string.Empty;
            RequestModel requestModel = new RequestModel()
            {
                //ProxyIp = $"http://35.235.72.87:42622",
                //ProxyUser = $"Z7mKWJyM58",
                //ProxyPassword = "Dz9TVoDdiF",
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/123.0.0.0 Safari/537.36",
            };

            string receive = string.Empty;
            int retryCount = 2; // 设置重试次数

            for (int attempt = 0; attempt <= retryCount; attempt++)
            {
                try
                { 
                    HttpWebRequest request = HttpWebHelper.Getrequest("https://api.gptsapi.net/v1/chat/completions", "post", requestModel); 
                    request.Headers.Clear();
                    request.ContentType = "application/json";
                    request.Headers.Add("Sec-Fetch-Dest", "document");
                    request.Headers.Add("Sec-Fetch-Mode", "navigate");
                    request.Headers.Add("Sec-Fetch-Site", "none");
                    request.Headers.Add("Sec-Fetch-User", "?1");
                    request.Headers.Add("Authorization", _authorization); // 请确保安全存储敏感信息
                    request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/113.0.0.0 Safari/537.36";

                    prompt = prompt.Replace("\t", "");
                    string message = $"[{{\"role\":\"system\",\"content\":\"You are a helpful assistant.\"}},{{\"role\":\"user\",\"content\":[{{\"type\":\"text\",\"text\":\"{prompt.Replace("\n", "").Replace("\\\"", "'").Replace("\"", "'")}\"}},{{\"type\":\"image_url\",\"image_url\":{{\"url\":\"{image}\",\"detail\":\"high\"}}}}]}}]";
                    var content = $"{{\"model\":\"gpt-4.1\",\"temperature\": 0.5,\"messages\":{message}, \"max_tokens\": 3000}}";
                    var bs = Encoding.UTF8.GetBytes(content);

                    request.ContentLength = bs.Length;
                    using (Stream reqStream = request.GetRequestStream())
                    {
                        reqStream.Write(bs, 0, bs.Length);
                    }
                    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                    {
                        using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                        {
                            receive = await reader.ReadToEndAsync(); // 使用异步读取
                        }
                    }

                    // 如果成功则跳出循环
                    break;
                }
                catch (WebException webEx)
                {
                    Console.WriteLine($"请求失败 ({attempt + 1}/{retryCount}): {webEx.Message}");

                    // 如果是最后一次尝试，则抛出异常
                    if (attempt == retryCount)
                    {
                        throw;
                    }

                    // 可选：等待一段时间后再重试
                    await Task.Delay(1000); // 等待1秒
                }
            }

            var jobject = JsonHelper.GetJObjectDeserialize(receive);
            res = jobject["choices"][0]["message"]["content"]?.ToString();
            res = res.Replace("```json", "").Replace("```", "").Trim();
            return res;
        }

    }
}
