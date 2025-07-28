using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Highever.SocialMedia.Common
{
    public static class HostingEnvironmentException
    {
        /// <summary>
        /// 扩展静态HOST事件
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseStaticHostEnviroment(this IApplicationBuilder app)
        {
            var webHostEnvironment = app.ApplicationServices.GetRequiredService<IHostingEnvironment>();
            HostingEnvironmentHelper.Configure(webHostEnvironment);
            return app;
        }
    }
}
