using System.Text.Json.Serialization;

namespace Highever.SocialMedia.Application.Models
{
    /// <summary>
    /// Token中的菜单权限信息
    /// </summary>
    public class TokenMenuPermission
    {
        /// <summary>
        /// 菜单ID
        /// </summary>
        [JsonPropertyName("id")]
        public int MenuId { get; set; }

        /// <summary>
        /// 菜单编码
        /// </summary>
        [JsonPropertyName("code")]
        public string MenuCode { get; set; }

        /// <summary>
        /// 菜单URL路径
        /// </summary>
        [JsonPropertyName("url")]
        public string MenuUrl { get; set; }

        /// <summary>
        /// 允许的HTTP方法
        /// </summary>
        [JsonPropertyName("methods")]
        public List<string> AllowedMethods { get; set; } = new();

        /// <summary>
        /// 是否支持通配符匹配
        /// </summary>
        [JsonPropertyName("wildcard")]
        public bool IsWildcard { get; set; }
    }

    /// <summary>
    /// 压缩的权限数据结构
    /// </summary>
    public class CompressedPermissionData
    {
        /// <summary>
        /// 权限版本号
        /// </summary>
        [JsonPropertyName("v")]
        public string Version { get; set; } = "1.0";

        /// <summary>
        /// 权限数据生成时间
        /// </summary>
        [JsonPropertyName("t")]
        public long GeneratedAt { get; set; }

        /// <summary>
        /// 菜单权限列表
        /// </summary>
        [JsonPropertyName("perms")]
        public List<TokenMenuPermission> MenuPermissions { get; set; } = new();

        /// <summary>
        /// 权限哈希值
        /// </summary>
        [JsonPropertyName("hash")]
        public string Hash { get; set; }
    }
}