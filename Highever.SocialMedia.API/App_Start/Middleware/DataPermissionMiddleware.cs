using System.Security.Claims;
using Highever.SocialMedia.Application.Context;
using Microsoft.AspNetCore.Authorization;

namespace Highever.SocialMedia.API
{
    /// <summary>
    /// 数据权限中间件
    /// </summary>
    public class DataPermissionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<DataPermissionMiddleware> _logger;

        public DataPermissionMiddleware(RequestDelegate next, ILogger<DataPermissionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context,
            IDataPermissionService dataPermissionService,
            IDataPermissionContextService contextService)
        {
            try
            {
                // 检查是否为匿名接口
                var endpoint = context.GetEndpoint();
                var isAnonymous = endpoint?.Metadata?.GetMetadata<AllowAnonymousAttribute>() != null;

                // 只有非匿名接口且用户已认证时才加载数据权限
                if (!isAnonymous && context.User.Identity?.IsAuthenticated == true)
                {
                    await LoadDataPermissionAsync(context, dataPermissionService, contextService);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "加载数据权限时发生错误");
                // 不阻塞请求继续执行
            }

            await _next(context);
        }

        /// <summary>
        /// 判断当前请求是否需要处理认证逻辑
        /// </summary>
        /// <param name="context">HTTP上下文</param>
        /// <returns>是否需要处理认证</returns>
        private bool ShouldProcessAuthentication(HttpContext context)
        {
            var endpoint = context.GetEndpoint();
            if (endpoint == null)
                return false;

            // 检查是否标记了 AllowAnonymous - 如果有则不需要处理数据权限
            var allowAnonymous = endpoint.Metadata?.GetMetadata<AllowAnonymousAttribute>() != null;
            if (allowAnonymous)
            {
                _logger.LogDebug("检测到 AllowAnonymous 特性，跳过数据权限加载");
                return false;
            }

            // 由于配置了全局授权过滤器，默认所有接口都需要认证
            return true;
        }
        /// <summary>
        /// 加载数据权限
        /// </summary>
        /// <param name="context"></param>
        /// <param name="dataPermissionService"></param>
        /// <param name="contextService"></param>
        /// <returns></returns>
        private async Task LoadDataPermissionAsync(HttpContext context,
            IDataPermissionService dataPermissionService,
            IDataPermissionContextService contextService)
        {
            // 从Token中获取用户ID
            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out var userId))
            {
                return;
            }

            // 获取用户角色
            var roles = context.User.FindAll(ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();

            // 加载数据权限上下文
            var permissionContext = await dataPermissionService.LoadDataPermissionAsync(userId, roles);

            // 设置到当前请求上下文
            contextService.SetCurrentContext(permissionContext);

            _logger.LogDebug($"用户 {userId} 数据权限加载完成，配置数量: {permissionContext.AccountConfigUniqueIds.Count}");
        }
    }
}

