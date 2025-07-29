using SqlSugar;

namespace Highever.SocialMedia.Domain.Entity
{
    /// <summary>
    /// 角色菜单关联表
    /// </summary>
    [SugarTable("menu_perms")]
    public class MenuPerms
    {
        /// <summary>
        /// 角色ID
        /// </summary>
        [SugarColumn(ColumnName = "role_id", IsPrimaryKey = true, IsNullable = false)]
        public long RoleId { get; set; }

        /// <summary>
        /// 菜单ID
        /// </summary>
        [SugarColumn(ColumnName = "menu_id", IsPrimaryKey = true, IsNullable = false)]
        public long MenuId { get; set; }
    }
}