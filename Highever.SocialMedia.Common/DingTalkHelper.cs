using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Highever.SocialMedia.Common
{
    public static class DingTalkHelper
    {
        /// <summary>
        /// 钉钉发送消息
        /// </summary>
        /// <param name="atMobile"></param>
        /// <param name="messageText"></param>
        /// <returns></returns>
            public static async Task SendDingMsg(List<string> atMobile, string messageText)
            {
                var webhookUrl = AppSettingConifgHelper.ReadAppSettings("conf:shopDingUrl");//"https://oapi.dingtalk.com/robot/send?access_token=99e7c701bdd7e9d53b5dcefd01b7570bfde5ce68bef1fee6cab7da3e2404ff52";
                var httpClient = new HttpClient();

                // 被@人的手机号
                var atMobiles = new List<string> {  }; // 替换为实际手机号
                atMobiles.AddRange(atMobile);
                // var messageText = "你好 @18688758429，这是来自店铺C#应用程序的测试消息。"; // 文本内容中包含@信息

                var messageJson = new
                {
                    msgtype = "text",
                    text = new
                    {
                        content = messageText
                    },
                    at = new
                    {
                        atMobiles = atMobiles,
                        isAtAll = false
                    }
                };

                var serializedJson = JsonConvert.SerializeObject(messageJson);
                var content = new StringContent(serializedJson, Encoding.UTF8, "application/json");

                try
                {
                    var response = await httpClient.PostAsync(webhookUrl, content);
                    var responseString = await response.Content.ReadAsStringAsync();

                    Console.WriteLine(responseString);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("发送消息时出错: " + ex.Message);
                }
            }
        }
}
