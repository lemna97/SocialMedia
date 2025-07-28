using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Highever.SocialMedia.API
{
    /// <summary>
    /// 全局Token参数校验
    /// </summary>
    public class AuthTokenParameterFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            operation.Parameters = operation.Parameters ?? new List<OpenApiParameter>();
            //MemberAuthorizeAttribute 自定义的身份验证特性标记
            var isAuthor = operation != null && context != null;
            if (isAuthor)
            {
                //in query header 
                operation.Parameters.Add(new OpenApiParameter()
                {
                    Name = "x-request-token",
                    In = ParameterLocation.Header, //query formData ..
                    Description = "token",
                    Required = false,
                });
                //in query header 
                operation.Parameters.Add(new OpenApiParameter()
                {
                    Name = "x-request-uuid",
                    In = ParameterLocation.Header, //query formData ..
                    Description = "唯一码",
                    Required = false,

                });
            }
        }
    }
}
