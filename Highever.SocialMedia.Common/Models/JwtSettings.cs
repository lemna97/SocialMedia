namespace Highever.SocialMedia.Common.Model
{
    /// <summary>
    /// JWT配置
    /// </summary>
    public class JwtSettings
    {
        /// <summary>
        /// 密钥
        /// </summary>
        public string SecretKey { get; set; } = string.Empty;

        /// <summary>
        /// 发行者
        /// </summary>
        public string Issuer { get; set; } = string.Empty;

        /// <summary>
        /// 受众
        /// </summary>
        public string Audience { get; set; } = string.Empty;

        /// <summary>
        /// 过期时间（分钟）
        /// </summary>
        public int ExpiryMinutes { get; set; } = 60;

        /// <summary>
        /// 刷新令牌过期时间（天）
        /// </summary>
        public int RefreshTokenExpiryDays { get; set; } = 7;
    }
}