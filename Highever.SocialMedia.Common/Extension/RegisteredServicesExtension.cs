using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Text;

namespace Highever.SocialMedia.Common
{
    public static class RegisteredServicesExtension
    {
        /// <summary>
        /// 查看服务列表
        /// </summary>
        /// <param name="app"></param>
        /// <param name="services"></param>
        public static void RegisteredServicesPage(this IApplicationBuilder app, IServiceCollection services)
        {
            app.Map("/services", builder => builder.Run(async context =>
            {
                var sb = new StringBuilder();
                sb.Append("<h1>Registered Services</h1>");
                sb.Append("<table><thead>");
                sb.Append("<tr><th>Type</th><th>Lifetime</th><th>Instance</th></tr>");
                sb.Append("</thead><tbody>");
                foreach (var svc in services)
                {
                    sb.Append("<tr>");
                    sb.Append($"<td>{svc.ServiceType.FullName}</td>");
                    sb.Append($"<td>{svc.Lifetime}</td>");
                    sb.Append($"<td>{svc.ImplementationType?.FullName}</td>");
                    sb.Append("</tr>");
                }
                sb.Append("</tbody></table>");
                await context.Response.WriteAsync(sb.ToString());
            }));
        }
    }
}
