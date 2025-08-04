using Highever.SocialMedia.Application;
using Highever.SocialMedia.Common;
using Highever.SocialMedia.SqlSugar;
using Highever.SocialMedia.MongoDB;
using Highever.SocialMedia.OpenAI;
using SqlSugar;

namespace Highever.SocialMedia.API
{
    /// <summary>
    /// 可以使用 scrutor 进行批量注入，更简单方便
    /// </summary>
    public static class IocRegister
    {
        /// <summary>
        /// 依赖注入
        /// </summary>
        /// <param name="services"></param>
        public static IServiceCollection Register(this IServiceCollection services)
        {
            // 注册 IHttpClientFactory
            services.AddHttpClient();
            // 注册 HttpClientHelper
            services.AddScoped<HttpClientHelper>();
            /*
             *  配置文件
             */
            IConfiguration configuration = new ConfigurationBuilder()
.AddJsonFile("appsettings.json")
.Build();
            services.AddSingleton(new AppSettingConifgHelper(configuration)); 
            //日志注入
            services.AddScoped<INLogger, NLogAdapter>();
            
            // 注册JwtHelper
            services.AddScoped<JwtHelper>();
            
            //注入ORM  
            services.AddSqlSugar();
            
            services.AddMongoDB();  
            //AI
            services.AddChatGPT();
            //注入业务
            services.AddApplicationServices();

            //业务层的事件
            services.AddApplicationEventBusServices();

            return services;
        }
    }
}
