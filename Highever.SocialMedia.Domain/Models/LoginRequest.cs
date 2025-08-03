using System.ComponentModel.DataAnnotations;

namespace Highever.SocialMedia.Domain.Models
{
    /// <summary>
    /// 登录请求模型
    /// </summary>
    public class LoginRequest
    {
        /// <summary>
        /// 用户名
        /// </summary>
        [Required(ErrorMessage = "用户名不能为空")]
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// 密码
        /// </summary>
        [Required(ErrorMessage = "密码不能为空")]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// 时间戳（可选，默认当前时间）
        /// </summary>
        public long? Timestamp { get; set; }
    }
}