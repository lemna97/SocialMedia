using Highever.SocialMedia.Application.Contracts.Context;
using Highever.SocialMedia.Common;
using Highever.SocialMedia.Domain.Entity;
using Highever.SocialMedia.Domain.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Highever.SocialMedia.API
{
    /// <summary>
    /// Token自动刷新中间件
    /// 功能：
    /// 1. 检测Token即将过期时自动刷新
    /// 2. 更新用户最后活动时间（实现滑动过期）
    /// 3. 自动更新Cookie中的Token
    /// </summary>
    public class TokenRefreshMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IServiceProvider _serviceProvider;
        public TokenRefreshMiddleware(RequestDelegate next, IServiceProvider serviceProvider)
        {
            _next = next;
            _serviceProvider = serviceProvider;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // 检查是否为匿名接口
                var endpoint = context.GetEndpoint();
                var isAnonymous = endpoint?.Metadata?.GetMetadata<AllowAnonymousAttribute>() != null;

                // 只处理需要认证的接口
                if (!isAnonymous)
                {
                    // 获取Token
                    var token = GetTokenFromRequest(context);

                    if (!string.IsNullOrEmpty(token))
                    {
                        // 验证Token格式和有效性
                        if (IsValidTokenFormat(token))
                        {
                            // 只有Token有效且用户已认证时才处理刷新逻辑
                            if (context.User.Identity.IsAuthenticated)
                            {
                                await ProcessAuthenticatedRequestAsync(context);
                            }
                        }
                    }
                }

                await _next(context);
            }
            catch (Exception ex)
            {
                // 记录错误但不阻塞请求
                Console.WriteLine($"Token刷新中间件执行异常: {ex.Message}");
                await _next(context);
            }
        }

        /// <summary>
        /// 验证Token格式是否有效
        /// </summary>
        private bool IsValidTokenFormat(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jsonToken = tokenHandler.ReadJwtToken(token);

                // 检查Token是否已过期
                if (jsonToken.ValidTo <= DateTime.UtcNow)
                {
                    return false;
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 处理已认证的请求 - 完善版
        /// </summary>
        private async Task ProcessAuthenticatedRequestAsync(HttpContext context)
        {
            try
            {
                var token = GetTokenFromRequest(context);
                if (string.IsNullOrEmpty(token))
                {
                    return;
                }

                var tokenHandler = new JwtSecurityTokenHandler();
                var jsonToken = tokenHandler.ReadJwtToken(token);
                var remainingTime = jsonToken.ValidTo - DateTime.UtcNow;

                Console.WriteLine($"Token剩余时间: {remainingTime.TotalMinutes:F1} 分钟");

                // 如果Token已经过期，不处理刷新逻辑
                if (remainingTime <= TimeSpan.Zero)
                {
                    Console.WriteLine("Token已过期，跳过刷新处理");
                    return;
                }

                // 生命周期管理：
                // 1. Token还有30分钟过期 → 延期2小时（滑动过期）
                if (remainingTime <= TimeSpan.FromMinutes(30) && remainingTime > TimeSpan.FromMinutes(5))
                {
                    Console.WriteLine($"Token剩余 {remainingTime.TotalMinutes:F1} 分钟，触发延期机制");
                    await ExtendTokenExpiryAsync(context, jsonToken);
                }
                // 2. Token还有5分钟过期 → 使用RefreshToken生成新Token
                else if (remainingTime <= TimeSpan.FromMinutes(5))
                {
                    Console.WriteLine($"Token剩余 {remainingTime.TotalMinutes:F1} 分钟，触发刷新机制");
                    await TryRefreshTokenAsync(context, jsonToken);
                }

                // 3. 始终更新用户活动时间（实现用户活跃度跟踪）
                await UpdateUserLastActivityAsync(context);
            }
            catch (SecurityTokenException ex)
            {
                Console.WriteLine("Token安全验证失败");
            }
            catch (Exception ex)
            {
                Console.WriteLine("处理Token刷新逻辑时发生错误");
            }
        }
        /// <summary>
        /// 延长Token过期时间（滑动过期）- 延期2小时，包含最新权限
        /// </summary>
        private async Task ExtendTokenExpiryAsync(HttpContext context, JwtSecurityToken jsonToken)
        {
            try
            {
                var userIdClaim = jsonToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
                {
                    return;
                }
                using (var scope = _serviceProvider.CreateScope())
                { 
                    var userRoleRepository = scope.ServiceProvider.GetRequiredService<IRepository<UserRoles>>();
                    var _tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();
                    var permissionEncoder = scope.ServiceProvider.GetRequiredService<ITokenPermissionEncoder>();

                    var userRoles = await userRoleRepository.QueryListAsync(ur => ur.UserId == userId);
                    var roles = userRoles?.Select(r => r.RoleId.ToString()).ToList() ?? new List<string>();

                    // 获取最新权限并生成延期Token
                    var permissionClaims = await permissionEncoder.EncodeUserPermissionsAsync(userId, roles);
                    var userName = jsonToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

                    var jwtHelper = scope.ServiceProvider.GetRequiredService<JwtHelper>();
                    var extendedToken = await jwtHelper.GenerateTokenWithPermissionsAsync(userId, userName ?? string.Empty, roles, permissionClaims);

                    // 设置延期token到响应头
                    context.Response.Headers.Add("X-Extended-Token", extendedToken);

                }
                Console.WriteLine($"用户 {userId} Token已延期2小时，权限已同步更新");
            }
            catch (Exception)
            {
                Console.WriteLine("延长Token过期时间失败");
            }
        }
        /// <summary>
        /// 从请求中获取Token
        /// </summary>
        private string? GetTokenFromRequest(HttpContext context)
        {
            // 优先从Authorization头获取
            var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
            {
                return authHeader.Substring("Bearer ".Length);
            }

            // 其次从Cookie获取
            return context.Request.Cookies["auth_token"];
        }


        /// <summary>
        /// 更新用户最后活动时间
        /// </summary>
        private async Task UpdateUserLastActivityAsync(HttpContext context)
        {
            try
            {
                var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId))
                {
                    using var scope = _serviceProvider.CreateScope();
                    var userRepository = scope.ServiceProvider.GetRequiredService<IRepository<Users>>();

                    var user = await userRepository.FirstOrDefaultAsync(u => u.Id == userId);
                    if (user != null)
                    {
                        user.LastActivityTime = DateTime.Now;
                        await userRepository.UpdateAsync(user);

                        Console.WriteLine($"更新用户 {userId} 最后活动时间: {DateTime.Now}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("更新用户最后活动时间失败");
            }
        }
        /// <summary>
        /// 尝试刷新Token（仅在即将过期时）
        /// </summary>
        private async Task TryRefreshTokenAsync(HttpContext context, JwtSecurityToken jsonToken)
        {
            try
            {
                var userIdClaim = jsonToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
                {
                    return;
                }
                
                using var scope = _serviceProvider.CreateScope();
                var _tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();
                var currentRefreshToken = _tokenService.GetRefreshTokenFromRequest(context);
                
                if (string.IsNullOrEmpty(currentRefreshToken))
                {
                    Console.WriteLine($"用户 {userId} 没有找到RefreshToken，无法刷新");
                    return;
                }

                // 使用TokenService刷新Token（已包含权限同步）
                var newTokenResult = await _tokenService.RefreshTokenAsync(currentRefreshToken);

                // 设置新的Token到响应头
                context.Response.Headers.Add("X-New-Access-Token", newTokenResult.AccessToken);
                context.Response.Headers.Add("X-New-Refresh-Token", newTokenResult.RefreshToken);

                Console.WriteLine($"用户 {userId} Token刷新成功，新Token有效期2小时，权限已同步");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"刷新Token失败: {ex.Message}");
            }
        }
    }
}






















