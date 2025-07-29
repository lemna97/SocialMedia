using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Highever.SocialMedia.Admin
{

    public class AutoHttpMethodOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            // 如果没有绑定 HTTP 方法，则设置默认方法
            if (context.ApiDescription.HttpMethod == null)
            {
                // 获取 Action 名称
                var actionName = context.ApiDescription.ActionDescriptor.RouteValues["action"];
                // 默认给 POST
                string defaultMethodName = "POST";

                // 根据 Action 名称前缀自动推断 HTTP 方法
                if (actionName.StartsWith("get", StringComparison.OrdinalIgnoreCase))
                {
                    defaultMethodName = "GET";
                }
                else if (actionName.StartsWith("put", StringComparison.OrdinalIgnoreCase))
                {
                    defaultMethodName = "PUT";
                }
                else if (actionName.StartsWith("delete", StringComparison.OrdinalIgnoreCase))
                {
                    defaultMethodName = "DELETE";
                }

                // 将推断结果写入 Swagger 文档元数据
                operation.Tags.Add(new OpenApiTag { Name = $"Default to {defaultMethodName}" });
            }
        }
    }

}
