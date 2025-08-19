namespace Highever.SocialMedia.Application.Contracts.Context
{
    /// <summary>
    /// 数据权限上下文
    /// </summary>
    public class DataPermissionContext
    {
        /// <summary>
        /// 当前用户ID
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// 当前用户角色列表
        /// </summary>
        public List<string> Roles { get; set; } = new List<string>();

        /// <summary>
        /// 用户配置UniqueId列表（当前用户有权限访问的配置UniqueId）
        /// </summary>
        public List<string> AccountConfigUniqueIds { get; set; } = new List<string>();
        /// <summary>
        /// 是否为管理员
        /// </summary>
        public bool IsAdmin => Roles.Contains("1") ||  Roles.Contains("2");

        /// <summary>
        /// 是否有数据权限（管理员或有配置UniqueId）
        /// </summary>
        public bool HasDataPermission => IsAdmin || AccountConfigUniqueIds.Any();

        /// <summary>
        /// 扩展属性（用于未来业务扩展）
        /// </summary>
        public Dictionary<string, object> ExtendedProperties { get; set; } = new Dictionary<string, object>();
    }
}

