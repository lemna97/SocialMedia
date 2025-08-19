using Highever.SocialMedia.Application.Context;
using Highever.SocialMedia.Application.Contracts.Context;
using Highever.SocialMedia.Application.Models;
using Highever.SocialMedia.Common;
using Highever.SocialMedia.Domain.Entity;
using Highever.SocialMedia.Domain.Repository;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Text.Json;

namespace Highever.SocialMedia.API
{
    /// <summary>
    /// 基于Token内嵌权限的验证中间件
    /// </summary>
    public class TokenPermissionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly INLogger _logger;
        private readonly IServiceProvider _serviceProvider;

        public TokenPermissionMiddleware(RequestDelegate next, INLogger logger, IServiceProvider serviceProvider)
        {
            _next = next;
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // 1. 检查是否为匿名接口
                var endpoint = context.GetEndpoint();
                var isAnonymous = endpoint?.Metadata?.GetMetadata<AllowAnonymousAttribute>() != null;

                // 2. 只处理需要认证的接口
                if (!isAnonymous && context.User.Identity?.IsAuthenticated == true)
                {
                    // 3. 从作用域获取权限编码器
                    using var scope = _serviceProvider.CreateScope();
                    var permissionEncoder = scope.ServiceProvider.GetRequiredService<ITokenPermissionEncoder>();

                    // 4. 从Token中验证菜单权限
                    var hasPermission = await ValidateMenuPermissionFromTokenAsync(context, permissionEncoder);

                    if (!hasPermission)
                    {
                        await HandlePermissionDeniedAsync(context);
                        return;
                    }
                }

                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Token权限验证中间件执行异常");
                await _next(context);
            }
        }

        /// <summary>
        /// 从Token中验证菜单权限
        /// </summary>
        private async Task<bool> ValidateMenuPermissionFromTokenAsync(HttpContext context, ITokenPermissionEncoder tokenPermissionEncoder)
        {
            try
            {
                // 1. 检查管理员权限
                var roles = context.User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
                if (IsAdministrator(roles))
                {
                    return true;
                }

                // 2. 获取权限Claims
                var menuPermsClaim = context.User.FindFirst("menu_perms");
                var menuHashClaim = context.User.FindFirst("menu_hash");

                if (menuPermsClaim == null || string.IsNullOrEmpty(menuPermsClaim.Value))
                {
                    _logger.Error("Token中未找到菜单权限信息");
                    return false;
                }
                // 3. 解压权限数据
                var permissionJson = tokenPermissionEncoder.DecompressPermissionData(menuPermsClaim.Value);
                if (string.IsNullOrEmpty(permissionJson))
                {
                    _logger.Error("权限数据解压失败");
                    return false;
                }
                // 4. 反序列化权限数据
                var permissionData = JsonSerializer.Deserialize<CompressedPermissionData>(permissionJson, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                if (permissionData?.MenuPermissions == null)
                {
                    _logger.Error("权限数据格式无效");
                    return false;
                }

                // 5. 验证具体的菜单权限
                var requestPath = context.Request.Path.Value ?? "";
                var httpMethod = context.Request.Method;
                await Task.CompletedTask;
                return ValidateSpecificPermission(permissionData.MenuPermissions, requestPath, httpMethod);

            }
            catch (Exception ex)
            {
                _logger.Error(ex, "从Token验证菜单权限失败");
                return false;
            }


        }

        /// <summary>
        /// 验证具体的菜单权限
        /// </summary>
        private bool ValidateSpecificPermission(List<TokenMenuPermission> permissions, string requestPath, string httpMethod)
        {
            var normalizedPath = NormalizePath(requestPath);

            foreach (var permission in permissions)
            {
                // 1. 精确匹配
                if (string.Equals(permission.MenuUrl, normalizedPath, StringComparison.OrdinalIgnoreCase))
                {
                    return ValidateHttpMethod(permission, httpMethod);
                }

                // 2. 通配符匹配
                if (permission.IsWildcard && IsWildcardMatch(permission.MenuUrl, normalizedPath))
                {
                    return ValidateHttpMethod(permission, httpMethod);
                }

                // 3. 前缀匹配
                if (normalizedPath.StartsWith(permission.MenuUrl, StringComparison.OrdinalIgnoreCase))
                {
                    return ValidateHttpMethod(permission, httpMethod);
                }
            }

            return false;
        }

        /// <summary>
        /// 验证HTTP方法权限
        /// </summary>
        private bool ValidateHttpMethod(TokenMenuPermission permission, string httpMethod)
        {
            if (permission.AllowedMethods == null || !permission.AllowedMethods.Any())
            {
                return true;
            }

            return permission.AllowedMethods.Contains(httpMethod, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 通配符匹配
        /// </summary>
        private bool IsWildcardMatch(string pattern, string path)
        {
            if (pattern.EndsWith("*"))
            {
                var prefix = pattern[..^1];
                return path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }

        /// <summary>
        /// 处理权限拒绝
        /// </summary>
        private async Task HandlePermissionDeniedAsync(HttpContext context)
        {
            context.Response.StatusCode = 403;
            context.Response.ContentType = "application/json";

            var result = new AjaxResult<string>
            {
                code = HttpCode.无权限,
                msg = "您没有访问此资源的权限"
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(result));
        }

        /// <summary>
        /// 检查是否为管理员
        /// </summary>
        private bool IsAdministrator(List<string> roles)
        {
            return roles.Contains("1") || roles.Contains("2"); // 超级管理员和管理员
        }

        /// <summary>
        /// 标准化路径
        /// </summary>
        private string NormalizePath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return "/";

            var cleanPath = path.Split('?', '#')[0];
            if (!cleanPath.StartsWith("/"))
                cleanPath = "/" + cleanPath;
            if (cleanPath.Length > 1 && cleanPath.EndsWith("/"))
                cleanPath = cleanPath[..^1];

            return cleanPath.ToLowerInvariant();
        }
    }
}



