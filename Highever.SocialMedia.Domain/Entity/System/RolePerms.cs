using SqlSugar;

namespace Highever.SocialMedia.Domain.Entity
{
    /// <summary>
    /// 角色权限关联表
    /// </summary>
    [SugarTable("system_role_perms")]
    public class RolePerms
    {
        /// <summary>
        /// 角色ID
        /// </summary>
        [SugarColumn(ColumnName = "role_id", IsPrimaryKey = true, IsNullable = false)]
        public long RoleId { get; set; }

        /// <summary>
        /// 权限ID
        /// </summary>
        [SugarColumn(ColumnName = "perm_id", IsPrimaryKey = true, IsNullable = false)]
        public long PermId { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [SugarColumn(ColumnName = "created_at", IsNullable = false)]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}

