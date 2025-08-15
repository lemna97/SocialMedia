using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text.Json;
using Highever.SocialMedia.Common;

namespace Highever.SocialMedia.API.Handlers
{
    /// <summary>
    /// 自定义JWT认证事件处理器
    /// 处理认证过程中的各种失败情况，返回统一的错误格式
    /// </summary>
    public class CustomAuthenticationHandler
    {
        /// <summary>
        /// 处理Token验证失败事件
        /// 触发场景：Token格式错误、签名无效、解析异常等
        /// </summary>
        /// <param name="context">认证失败上下文</param>
        /// <returns>异步任务</returns>
        public static Task HandleAuthenticationFailed(AuthenticationFailedContext context)
        {
            // 设置HTTP状态码为401未授权
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";

            // 返回统一的错误格式
            var result = new AjaxResult<string>() 
            { 
                code = HttpCode.未登录, 
                msg = "Token无效或已过期，请重新登录"
            };

            return context.Response.WriteAsync(JsonSerializer.Serialize(result));
        }

        /// <summary>
        /// 处理认证挑战事件
        /// 触发场景：访问需要认证的接口但未提供有效Token
        /// </summary>
        /// <param name="context">JWT Bearer挑战上下文</param>
        /// <returns>异步任务</returns>
        public static Task HandleChallenge(JwtBearerChallengeContext context)
        {
            // 阻止默认的挑战响应（避免返回WWW-Authenticate头）
            context.HandleResponse();
            
            // 设置HTTP状态码为401未授权
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";

            // 返回统一的错误格式
            var result = new AjaxResult<string>() 
            { 
                code = HttpCode.未登录, 
                msg = "未登录或登录已过期，请重新登录"
            };

            return context.Response.WriteAsync(JsonSerializer.Serialize(result));
        }

        /// <summary>
        /// 处理权限禁止事件
        /// 触发场景：Token有效但用户权限不足
        /// </summary>
        /// <param name="context">禁止访问上下文</param>
        /// <returns>异步任务</returns>
        public static Task HandleForbidden(ForbiddenContext context)
        {
            // 设置HTTP状态码为403禁止访问
            context.Response.StatusCode = 403;
            context.Response.ContentType = "application/json";

            // 返回统一的错误格式
            var result = new AjaxResult<string>() 
            { 
                code = HttpCode.无权限, 
                msg = "权限不足，无法访问该资源"
            };

            return context.Response.WriteAsync(JsonSerializer.Serialize(result));
        }
    }
}

