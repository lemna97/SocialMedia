using SqlSugar;

namespace Highever.SocialMedia.Domain.Entity
{
    /// <summary>
    /// 角色表
    /// </summary>
    [SugarTable("roles")]
    public class Roles : BaseEntity
    {
        /// <summary>
        /// 角色代码
        /// </summary>
        [SugarColumn(ColumnName = "code", Length = 50, IsNullable = false)]
        public string Code { get; set; }

        /// <summary>
        /// 角色名称
        /// </summary>
        [SugarColumn(ColumnName = "name", Length = 100, IsNullable = false)]
        public string Name { get; set; }

        /// <summary>
        /// 是否系统内置角色
        /// </summary>
        [SugarColumn(ColumnName = "is_sys", IsNullable = false)]
        public bool IsSys { get; set; } = false;
    }
}

