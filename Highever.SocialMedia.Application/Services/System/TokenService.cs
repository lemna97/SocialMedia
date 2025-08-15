using Highever.SocialMedia.Application.Contracts;
using Highever.SocialMedia.Common;
using Highever.SocialMedia.Common.Models;
using Highever.SocialMedia.Domain.Entity;
using Highever.SocialMedia.Domain.Repository;
using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Highever.SocialMedia.Application.Services.System
{

    public class TokenService : ITokenService
    {
        private readonly IRepository<Users> _userRepository;
        private readonly IRepository<UserRoles> _userUserRolesRepository;
        private readonly IRepository<Roles> _userRoleRepository;
        private readonly IRepository<UserTokenSessions> _userTokenSessionRepository;
        private readonly JwtHelper _jwtHelper;
        private readonly INLogger _logger;

        public TokenService(
            IRepository<Users> userRepository,
            IRepository<UserRoles> userUserRolesRepository,
            IRepository<UserTokenSessions> userTokenSessionRepository,
            JwtHelper jwtHelper,
            INLogger logger,
            IRepository<Roles> userRoleRepository)
        {
            _userRepository = userRepository;
            _userUserRolesRepository = userUserRolesRepository;
            _userTokenSessionRepository = userTokenSessionRepository;
            _jwtHelper = jwtHelper;
            _logger = logger;
            _userRoleRepository = userRoleRepository;
        }
        /// <summary>
        /// 生成访问令牌（用于延期）
        /// </summary>
        public string GenerateAccessToken(int userId, string? userName, List<string> roles)
        {
            return _jwtHelper.GenerateToken(userId, userName ?? string.Empty, roles);
        }
        /// <summary>
        /// 延长Token过期时间（滑动过期）
        /// </summary>
        public async Task ExtendTokenExpiryAsync(HttpContext context, JwtSecurityToken jsonToken)
        {
            try
            {
                var userIdClaim = jsonToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
                {
                    return;
                }
                
                var userRoles = await _userUserRolesRepository.QueryListAsync(ur => ur.UserId == userId);
                var roles = userRoles?.Select(r => r.RoleId.ToString()).ToList() ?? new List<string>();

                // 使用新的方法生成延期token 
                var userName = jsonToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
                var newToken = GenerateAccessToken(userId, userName, roles);

                // 设置新token到响应头
                context.Response.Headers.Add("X-Extended-Token", newToken);

                // 更新会话活动时间
                var refreshToken = GetRefreshTokenFromRequest(context);
                if (!string.IsNullOrEmpty(refreshToken))
                {
                    await UpdateTokenSessionActivityAsync(refreshToken);
                }

                _logger.Info($"用户 {userId} Token过期时间已延长");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "延长Token过期时间失败");
            }
        }
        /// <summary>
        /// 刷新Token - 支持多设备登录
        /// </summary>
        public async Task<TokenResult> RefreshTokenAsync(string refreshToken)
        {
            // 1. 验证RefreshToken格式
            if (!_jwtHelper.IsValidRefreshTokenFormat(refreshToken))
            {
                throw new UnauthorizedAccessException("刷新令牌格式无效");
            }

            // 2. 查找Token会话
            var tokenSession = await _userTokenSessionRepository.FirstOrDefaultAsync(ts =>
                ts.RefreshToken == refreshToken &&
                ts.RefreshTokenExpiry > DateTime.UtcNow &&
                ts.IsActive);

            if (tokenSession == null)
            {
                throw new UnauthorizedAccessException("刷新令牌无效或已过期");
            }

            // 3. 获取用户信息
            var user = await _userRepository.FirstOrDefaultAsync(u => u.Id == tokenSession.UserId);
            if (user == null || !user.IsActive)
            {
                throw new UnauthorizedAccessException("用户不存在或已被禁用");
            }

            // 4. 检查用户是否长时间不活跃（可选）
            if (tokenSession.LastActivityTime < DateTime.Now.AddDays(-30))
            {
                throw new UnauthorizedAccessException("会话长时间未活跃，请重新登录");
            }

            // 5. 获取用户角色
            var userRoles = await _userUserRolesRepository.QueryListAsync(ur => ur.UserId == user.Id);
            var roles = userRoles?.Select(r => r.RoleId.ToString()).ToList() ?? new List<string>();

            // 6. 生成新的Token对
            var tokenResult = _jwtHelper.RefreshToken(user.Id, user.Account, roles);

            // 7. 更新Token会话
            tokenSession.RefreshToken = tokenResult.RefreshToken;
            tokenSession.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
            tokenSession.LastActivityTime = DateTime.Now;
            tokenSession.UpdatedAt = DateTime.Now;
            await _userTokenSessionRepository.UpdateAsync(tokenSession);

            _logger.Info($"用户 {user.Id} 的Token会话 {tokenSession.Id} 已刷新");
            return tokenResult;
        }

        /// <summary>
        /// 创建新的Token会话
        /// </summary>
        public async Task<bool> CreateTokenSessionAsync(int userId, string refreshToken, string? deviceId = null, string? deviceInfo = null, string? ipAddress = null)
        {
            var tokenSession = new UserTokenSessions
            {
                UserId = userId,
                RefreshToken = refreshToken,
                RefreshTokenExpiry = DateTime.UtcNow.AddDays(7),
                DeviceId = deviceId,
                DeviceInfo = deviceInfo,
                IpAddress = ipAddress,
                LastActivityTime = DateTime.Now,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            var result = await _userTokenSessionRepository.InsertAsync(tokenSession);
            _logger.Info($"用户 {userId} 创建新Token会话，设备ID: {deviceId}");
            return result > 0;
        }

        /// <summary>
        /// 撤销用户的所有Token会话
        /// </summary>
        public async Task<bool> RevokeAllUserTokenSessionsAsync(int userId)
        {
            var tokenmodel = await _userTokenSessionRepository.GetByIdAsync(userId);
            if (tokenmodel == null) { return false; }
            tokenmodel.IsActive = false;
            tokenmodel.UpdatedAt = DateTime.Now;
            var result = await _userTokenSessionRepository.UpdateAsync(tokenmodel);
            _logger.Info($"用户 {userId} 的所有Token会话已撤销");
            return result > 0;
        }

        /// <summary>
        /// 撤销指定的Token会话
        /// </summary>
        public async Task<bool> RevokeTokenSessionAsync(string refreshToken)
        {
            var tokenSession = await _userTokenSessionRepository.FirstOrDefaultAsync(ts => ts.RefreshToken == refreshToken);
            if (tokenSession != null)
            {
                tokenSession.IsActive = false;
                tokenSession.UpdatedAt = DateTime.Now;
                await _userTokenSessionRepository.UpdateAsync(tokenSession);

                _logger.Info($"Token会话 {tokenSession.Id} 已撤销");
                return true;
            }
            return false;
        }

        /// <summary>
        /// 更新Token会话活动时间
        /// </summary>
        public async Task<bool> UpdateTokenSessionActivityAsync(string refreshToken)
        {
            var tokenSession = await _userTokenSessionRepository.FirstOrDefaultAsync(ts =>
                ts.RefreshToken == refreshToken && ts.IsActive);

            if (tokenSession != null)
            {
                tokenSession.LastActivityTime = DateTime.Now;
                tokenSession.UpdatedAt = DateTime.Now;
                await _userTokenSessionRepository.UpdateAsync(tokenSession);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 清理过期的Token会话
        /// </summary>
        public async Task<int> CleanupExpiredTokenSessionsAsync()
        {
            var expiredSessions = await _userTokenSessionRepository.QueryListAsync(ts =>
                ts.RefreshTokenExpiry <= DateTime.UtcNow ||
                ts.LastActivityTime <= DateTime.Now.AddDays(-30));

            if (expiredSessions.Any())
            {
                var result = await _userTokenSessionRepository.DeleteAsync(ts =>
                    ts.RefreshTokenExpiry <= DateTime.UtcNow ||
                    ts.LastActivityTime <= DateTime.Now.AddDays(-30));

                _logger.Info($"清理了 {result} 个过期的Token会话");
                return result;
            }
            return 0;
        }

        /// <summary>
        /// 撤销用户的刷新令牌 - 兼容旧接口
        /// </summary>
        public async Task<bool> RevokeRefreshTokenAsync(int userId)
        {
            return await RevokeAllUserTokenSessionsAsync(userId);
        }

        /// <summary>
        /// 更新用户的刷新令牌 - 兼容旧接口
        /// </summary>
        public async Task<bool> UpdateRefreshTokenAsync(int userId, string refreshToken, DateTime expiry)
        {
            var tokenSession = await _userTokenSessionRepository.FirstOrDefaultAsync(ts =>
                ts.UserId == userId && ts.IsActive);

            if (tokenSession != null)
            {
                tokenSession.RefreshToken = refreshToken;
                tokenSession.RefreshTokenExpiry = expiry;
                tokenSession.UpdatedAt = DateTime.Now;
                await _userTokenSessionRepository.UpdateAsync(tokenSession);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 验证刷新令牌是否有效
        /// </summary>
        public async Task<bool> ValidateRefreshTokenAsync(string refreshToken)
        {
            var tokenSession = await _userTokenSessionRepository.FirstOrDefaultAsync(ts =>
                ts.RefreshToken == refreshToken &&
                ts.RefreshTokenExpiry > DateTime.UtcNow &&
                ts.IsActive);

            return tokenSession != null;
        }

        /// <summary>
        /// 获取用户的刷新令牌信息 - 兼容旧接口
        /// </summary>
        public async Task<RefreshTokenInfo?> GetRefreshTokenInfoAsync(int userId)
        {
            var tokenSession = await _userTokenSessionRepository.FirstOrDefaultAsync(ts =>
                ts.UserId == userId && ts.IsActive);

            if (tokenSession != null)
            {
                return new RefreshTokenInfo
                {
                    UserId = tokenSession.UserId,
                    RefreshToken = tokenSession.RefreshToken,
                    ExpireTime = tokenSession.RefreshTokenExpiry,
                    LastActivityTime = tokenSession.LastActivityTime
                };
            }
            return null;
        }

        /// <summary>
        /// 清理过期的刷新令牌 - 兼容旧接口
        /// </summary>
        public async Task<int> CleanupExpiredTokensAsync()
        {
            return await CleanupExpiredTokenSessionsAsync();
        }

        /// <summary>
        /// 获取用户的所有活跃会话
        /// </summary>
        public async Task<List<UserTokenSessionInfo>> GetUserActiveSessionsAsync(int userId)
        {
            var sessions = await _userTokenSessionRepository.QueryListAsync(ts =>
                ts.UserId == userId && ts.IsActive);

            return sessions.Select(s => new UserTokenSessionInfo
            {
                Id = s.Id,
                UserId = s.UserId,
                DeviceId = s.DeviceId,
                DeviceInfo = s.DeviceInfo,
                IpAddress = s.IpAddress,
                LastActivityTime = s.LastActivityTime,
                CreatedAt = s.CreatedAt,
                IsCurrentSession = false // 需要在调用时设置
            }).ToList();
        }

        /// <summary>
        /// 从HTTP请求中获取RefreshToken
        /// </summary>
        public string? GetRefreshTokenFromRequest(HttpContext context)
        {
            // 1. 优先从Cookie获取
            var refreshToken = context.Request.Cookies["refresh_token"];
            if (!string.IsNullOrEmpty(refreshToken))
            {
                return refreshToken;
            }

            // 2. 从Header获取
            var headerToken = context.Request.Headers["X-Refresh-Token"].FirstOrDefault();
            if (!string.IsNullOrEmpty(headerToken))
            {
                return headerToken;
            }

            // 3. 从Form获取
            if (context.Request.HasFormContentType)
            {
                var formToken = context.Request.Form["refresh_token"].FirstOrDefault();
                if (!string.IsNullOrEmpty(formToken))
                {
                    return formToken;
                }
            }

            return null;
        }
    }
}





