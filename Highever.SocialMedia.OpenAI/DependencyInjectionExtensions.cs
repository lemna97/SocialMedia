using Highever.SocialMedia.Common;
using Highever.SocialMedia.OpenAI;
using Highever.SocialMedia.OpenAI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Highever.SocialMedia.SqlSugar
{
    /// <summary>
    /// SqlSugar 模块化注入
    /// </summary>
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddChatGPT(this IServiceCollection services)
        {
            #region 检查 IHttpClientFactory 是否已注册
            // 判断是否已经注册了 AddHttpClient
            if (!services.Any(service => service.ServiceType == typeof(IHttpClientFactory)))
            {
                // 如果没有注册，则注册 AddHttpClient
                services.AddHttpClient();
            }
            #endregion

            #region 检查 HttpClientHelper 是否已注册
            // 判断是否已经注册了 HttpClientHelper
            if (!services.Any(service => service.ServiceType == typeof(HttpClientHelper)))
            {
                // 如果没有注册，则注册 HttpClientHelper
                services.AddScoped<HttpClientHelper>();
            }
            #endregion
            services.AddScoped<IChatGPTService>(provider =>
            {
                var configuration = provider.GetRequiredService<IConfiguration>();
                var httpClientHelper = provider.GetRequiredService<HttpClientHelper>(); 
                // 主库连接字符串
                string apiToken = AppSettingConifgHelper.ReadAppSettings("ChatGPT:OpenAIApiToken"); 
                // 初始化并返回 SqlSugar 上下文
                return new ChatGPTService(httpClientHelper, apiToken);
            }); 
            return services;
        }

    }
}


