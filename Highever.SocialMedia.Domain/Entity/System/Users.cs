using SqlSugar;

namespace Highever.SocialMedia.Domain.Entity
{
    /// <summary>
    /// 用户账号信息表
    /// </summary>
    [SugarTable("system_users")]
    public class Users : BaseEntity
    {
        /// <summary>
        /// 账号
        /// </summary>
        [SugarColumn(ColumnName = "account", Length = 50, IsNullable = false)]
        public string Account { get; set; }

        /// <summary>
        /// 密码哈希
        /// </summary>
        [SugarColumn(ColumnName = "password_hash", Length = 255, IsNullable = false)]
        public string PasswordHash { get; set; }

        /// <summary>
        /// 显示名称
        /// </summary>
        [SugarColumn(ColumnName = "display_name", Length = 100, IsNullable = false)]
        public string DisplayName { get; set; }

        /// <summary>
        /// 是否激活
        /// </summary>
        [SugarColumn(ColumnName = "is_active", IsNullable = false)]
        public bool IsActive { get; set; } = true;
    }
}

