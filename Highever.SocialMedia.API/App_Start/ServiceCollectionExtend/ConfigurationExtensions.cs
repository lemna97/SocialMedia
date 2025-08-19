using Highever.SocialMedia.Common.Model;

namespace Highever.SocialMedia.API
{
    /// <summary>
    /// 配置服务注入扩展
    /// </summary>
    public static class ConfigurationExtensions
    {
        /// <summary>
        /// 注册所有配置服务
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static IServiceCollection AddConfigurations(this IServiceCollection services, IConfiguration configuration)
        {
            // JWT配置
            services.Configure<JwtSettings>(configuration.GetSection("JwtSettings")); 
            
            // 可以继续添加其他配置... 
            
            return services;
        }

        /// <summary>
        /// 注册需要直接注入的配置实例（用于向后兼容）
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static IServiceCollection AddLegacyConfigurations(this IServiceCollection services, IConfiguration configuration)
        {
            // JWT设置 - 保持现有的直接注入方式（向后兼容）
            var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>();
            if (jwtSettings != null)
            {
                services.AddSingleton(jwtSettings);
            }

            return services;
        }
    }
}