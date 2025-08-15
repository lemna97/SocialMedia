using SqlSugar;

namespace Highever.SocialMedia.Domain.Entity
{
    /// <summary>
    /// 用户Token会话表 - 支持多设备登录
    /// </summary>
    [SugarTable("user_token_sessions")]
    public class UserTokenSessions
    {
        /// <summary>
        /// 主键ID
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; }

        /// <summary>
        /// 用户ID
        /// </summary>
        [SugarColumn(ColumnName = "user_id", IsNullable = false)]
        public int UserId { get; set; }

        /// <summary>
        /// 刷新令牌
        /// </summary>
        [SugarColumn(ColumnName = "refresh_token", Length = 500, IsNullable = false)]
        public string RefreshToken { get; set; }

        /// <summary>
        /// 刷新令牌过期时间
        /// </summary>
        [SugarColumn(ColumnName = "refresh_token_expiry", IsNullable = false)]
        public DateTime RefreshTokenExpiry { get; set; }

        /// <summary>
        /// 设备标识（浏览器指纹、设备ID等）
        /// </summary>
        [SugarColumn(ColumnName = "device_id", Length = 200, IsNullable = true)]
        public string? DeviceId { get; set; }

        /// <summary>
        /// 设备信息（User-Agent等）
        /// </summary>
        [SugarColumn(ColumnName = "device_info", Length = 500, IsNullable = true)]
        public string? DeviceInfo { get; set; }

        /// <summary>
        /// IP地址
        /// </summary>
        [SugarColumn(ColumnName = "ip_address", Length = 50, IsNullable = true)]
        public string? IpAddress { get; set; }

        /// <summary>
        /// 最后活动时间
        /// </summary>
        [SugarColumn(ColumnName = "last_activity_time", IsNullable = false)]
        public DateTime LastActivityTime { get; set; }

        /// <summary>
        /// 是否活跃
        /// </summary>
        [SugarColumn(ColumnName = "is_active", IsNullable = false)]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// 创建时间
        /// </summary>
        [SugarColumn(ColumnName = "created_at", IsNullable = false)]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// 更新时间
        /// </summary>
        [SugarColumn(ColumnName = "updated_at", IsNullable = true)]
        public DateTime? UpdatedAt { get; set; }
    }
}