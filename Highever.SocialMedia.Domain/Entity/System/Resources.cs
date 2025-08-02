using SqlSugar;

namespace Highever.SocialMedia.Domain.Entity
{
    /// <summary>
    /// 资源表
    /// </summary>
    [SugarTable("system_resources")]
    public class Resources : BaseEntity
    {
        /// <summary>
        /// 表名/接口标识
        /// </summary>
        [SugarColumn(ColumnName = "name", Length = 100, IsNullable = false)]
        public string Name { get; set; }

        /// <summary>
        /// 资源显示名称
        /// </summary>
        [SugarColumn(ColumnName = "display_name", Length = 100, IsNullable = false)]
        public string DisplayName { get; set; }
    }
}

