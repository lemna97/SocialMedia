using Microsoft.Extensions.DependencyInjection;

namespace Highever.SocialMedia.Common
{
    /// <summary>
    /// 容器的扩展
    /// </summary>
    public static class ServiceCollectionExtension
    {
        /// <summary>
        /// 自动迁移
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddEFAutoMigrate(this IServiceCollection services)
        {
            using (var scope = services.BuildServiceProvider().CreateScope())
            { 
            };
            return services;
        }
    }
}
