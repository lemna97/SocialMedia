using Highever.SocialMedia.Common;
using Highever.SocialMedia.Common.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace Highever.SocialMedia.Application.Contracts.Context
{
    /// <summary>
    /// Token管理服务接口
    /// 负责处理JWT Token的业务逻辑，包括刷新、撤销等操作
    /// </summary>
    public interface ITokenService : ITransientDependency
    {
        /// <summary>
        /// 刷新访问令牌
        /// </summary>
        /// <param name="refreshToken">刷新令牌</param>
        /// <returns>新的Token对</returns>
        /// <exception cref="UnauthorizedAccessException">当刷新令牌无效或已过期时抛出</exception>
        Task<TokenResult> RefreshTokenAsync(string refreshToken);

        /// <summary>
        /// 撤销用户的刷新令牌
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>是否撤销成功</returns>
        Task<bool> RevokeRefreshTokenAsync(int userId);

        /// <summary>
        /// 更新用户的刷新令牌
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="refreshToken">新的刷新令牌</param>
        /// <param name="expiry">过期时间</param>
        /// <returns>是否更新成功</returns>
        Task<bool> UpdateRefreshTokenAsync(int userId, string refreshToken, DateTime expiry);

        /// <summary>
        /// 验证刷新令牌是否有效
        /// </summary>
        /// <param name="refreshToken">刷新令牌</param>
        /// <returns>是否有效</returns>
        Task<bool> ValidateRefreshTokenAsync(string refreshToken);

        /// <summary>
        /// 获取用户的刷新令牌信息
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>刷新令牌信息，如果不存在则返回null</returns>
        Task<RefreshTokenInfo?> GetRefreshTokenInfoAsync(int userId);

        /// <summary>
        /// 清理过期的刷新令牌
        /// </summary>
        /// <returns>清理的令牌数量</returns>
        Task<int> CleanupExpiredTokensAsync();

        /// <summary>
        /// 从HTTP请求中获取RefreshToken
        /// </summary>
        /// <param name="context">HTTP上下文</param>
        /// <returns>RefreshToken，如果没有找到则返回null</returns>
        string? GetRefreshTokenFromRequest(HttpContext context);

        // ========== 新增的多设备会话管理方法 ==========

        /// <summary>
        /// 创建新的Token会话
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="refreshToken">刷新令牌</param>
        /// <param name="deviceId">设备ID</param>
        /// <param name="deviceInfo">设备信息</param>
        /// <param name="ipAddress">IP地址</param>
        /// <returns>是否创建成功</returns>
        Task<bool> CreateTokenSessionAsync(int userId, string refreshToken, string? deviceId = null, string? deviceInfo = null, string? ipAddress = null);

        /// <summary>
        /// 撤销用户的所有Token会话
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>是否撤销成功</returns>
        Task<bool> RevokeAllUserTokenSessionsAsync(int userId);

        /// <summary>
        /// 撤销指定的Token会话
        /// </summary>
        /// <param name="refreshToken">刷新令牌</param>
        /// <returns>是否撤销成功</returns>
        Task<bool> RevokeTokenSessionAsync(string refreshToken);

        /// <summary>
        /// 更新Token会话活动时间
        /// </summary>
        /// <param name="refreshToken">刷新令牌</param>
        /// <returns>是否更新成功</returns>
        Task<bool> UpdateTokenSessionActivityAsync(string refreshToken);

        /// <summary>
        /// 清理过期的Token会话
        /// </summary>
        /// <returns>清理的会话数量</returns>
        Task<int> CleanupExpiredTokenSessionsAsync();

        /// <summary>
        /// 获取用户的所有活跃会话
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>用户的活跃会话列表</returns>
        Task<List<UserTokenSessionInfo>> GetUserActiveSessionsAsync(int userId); 

        /// <summary>
        /// 延长Token过期时间（滑动过期）
        /// </summary>
        Task ExtendTokenExpiryAsync(HttpContext context, JwtSecurityToken jsonToken);
    }

    /// <summary>
    /// 刷新令牌信息
    /// </summary>
    public class RefreshTokenInfo
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// 刷新令牌
        /// </summary>
        public string RefreshToken { get; set; }

        /// <summary>
        /// 过期时间
        /// </summary>
        public DateTime? ExpireTime { get; set; }

        /// <summary>
        /// 最后活动时间
        /// </summary>
        public DateTime? LastActivityTime { get; set; }

        /// <summary>
        /// 是否已过期
        /// </summary>
        public bool IsExpired => ExpireTime.HasValue && ExpireTime <= DateTime.UtcNow;
    }
}





