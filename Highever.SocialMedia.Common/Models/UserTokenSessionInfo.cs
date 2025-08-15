namespace Highever.SocialMedia.Common.Models
{
    /// <summary>
    /// 用户Token会话信息
    /// </summary>
    public class UserTokenSessionInfo
    {
        /// <summary>
        /// 会话ID
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 用户ID
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// 设备ID
        /// </summary>
        public string? DeviceId { get; set; }

        /// <summary>
        /// 设备信息
        /// </summary>
        public string? DeviceInfo { get; set; }

        /// <summary>
        /// IP地址
        /// </summary>
        public string? IpAddress { get; set; }

        /// <summary>
        /// 最后活动时间
        /// </summary>
        public DateTime LastActivityTime { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 是否当前会话
        /// </summary>
        public bool IsCurrentSession { get; set; }

        /// <summary>
        /// 设备类型（可选）
        /// </summary>
        public string? DeviceType { get; set; }

        /// <summary>
        /// 地理位置（可选）
        /// </summary>
        public string? Location { get; set; }
    }
}