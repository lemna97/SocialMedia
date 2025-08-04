using System.Text.Json;
using System.Net;
using Highever.SocialMedia.Common;

namespace Highever.SocialMedia.Admin
{
    /// <summary>
    /// 异常日志中间件
    /// </summary>
    public class CustomExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<CustomExceptionHandlerMiddleware> _logger;
        private readonly IServiceProvider _serviceProvider;
        public CustomExceptionHandlerMiddleware(RequestDelegate next, ILogger<CustomExceptionHandlerMiddleware> logger, IServiceProvider serviceProvider)
        {
            _next = next;
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                // 跳过Swagger相关路径的异常处理
                if (context.Request.Path.StartsWithSegments("/swagger") || 
                    context.Request.Path.StartsWithSegments("/doc.html"))
                {
                    await _next(context);
                    return;
                }

                context.Request.EnableBuffering();
                await _next(context);
            }
            catch (Exception ex)
            {
                // 使用 HttpContext 的服务提供程序（已经是正确的作用域）
                var nLogger = context.RequestServices.GetRequiredService<INLogger>();
                nLogger.DateBaseError($"Admin异常: {ex.Message} | StackTrace: {ex.StackTrace}");
                
                // 使用ILogger记录到控制台
                _logger.LogError(ex, "Admin请求发生异常: {Message}", ex.Message);
                
                await HandleExceptionAsync(context, ex);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var message = exception.Message;
            var result = new AjaxResult<string>() { code = HttpCode.失败, msg = message }; 
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            if (message.ToLower().Contains("ssl") || message.ToLower().Contains("connection"))
            {
                result.code =HttpCode.接口请求超时;
            } 
            return context.Response.WriteAsync(JsonSerializer.Serialize(result));
        }
    }

    public static class CustomExceptionHandlerMiddlewareExtensions
    {
        public static IApplicationBuilder UseCustomExceptionHandler(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<CustomExceptionHandlerMiddleware>();
        }
    }
}
