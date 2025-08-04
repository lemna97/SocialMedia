using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Highever.SocialMedia.API
{

    public class AutoHttpMethodOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            // 检查是否是通过约定自动分配的HTTP方法
            var actionName = context.ApiDescription.ActionDescriptor.RouteValues["action"];
            var httpMethod = context.ApiDescription.HttpMethod;
            
            if (!string.IsNullOrEmpty(actionName) && !string.IsNullOrEmpty(httpMethod))
            {
                // 检查是否是自动推断的方法
                var expectedMethod = GetExpectedHttpMethod(actionName);
                if (httpMethod.Equals(expectedMethod, StringComparison.OrdinalIgnoreCase))
                {
                    // 添加标记表示这是自动推断的
                    //operation.Tags.Add(new OpenApiTag { Name = $"Auto-detected: {httpMethod}" });
                }
            }
        }

        private string GetExpectedHttpMethod(string actionName)
        {
            var lowerActionName = actionName.ToLower();

            if (lowerActionName.StartsWith("get") || 
                lowerActionName.Equals("index") || 
                lowerActionName.Equals("details") ||
                lowerActionName.Equals("list"))
            {
                return "GET";
            }
            else if (lowerActionName.StartsWith("put") || 
                     lowerActionName.StartsWith("update") ||
                     lowerActionName.StartsWith("edit"))
            {
                return "PUT";
            }
            else if (lowerActionName.StartsWith("delete") || 
                     lowerActionName.StartsWith("remove"))
            {
                return "DELETE";
            }
            else if (lowerActionName.StartsWith("patch"))
            {
                return "PATCH";
            }
            else
            {
                return "POST";
            }
        }
    }

}
