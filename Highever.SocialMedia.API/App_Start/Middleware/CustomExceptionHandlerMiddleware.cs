using System.Text.Json;
using System.Net;
using Highever.SocialMedia.Common;

namespace Highever.SocialMedia.API
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
                // 一定要在任何读取 Body 之前先 EnableBuffering
                context.Request.EnableBuffering();

                // 如果你想记录 Body，就按下面流程做：
                // using var reader = new StreamReader(context.Request.Body, leaveOpen: true);
                // var bodyText = await reader.ReadToEndAsync();
                // _logger.LogDebug("RequestBody: {body}", bodyText);
                // context.Request.Body.Position = 0;
                await _next(context);
            }
            catch (Exception ex)
            {
                //_serviceProvider.GetRequiredService<NLogger>().DateBaseError(ex.Message);
                _logger.LogError(ex, ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var message = exception.Message;

            var result = new AjaxResult<string>() {  httpCode = HttpCode.失败, msg = message }; 
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            if (message.ToLower().Contains("ssl") || message.ToLower().Contains("connection"))
            {
                result.httpCode =HttpCode.接口请求超时;
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
