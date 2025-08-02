using SqlSugar;

namespace Highever.SocialMedia.Domain.Entity
{
    /// <summary>
    /// 菜单表
    /// </summary>
    [SugarTable("system_menus")]
    public class Menus : BaseEntity
    {
        /// <summary>
        /// 父级菜单ID
        /// </summary>
        [SugarColumn(ColumnName = "parent_id", IsNullable = true)]
        public long? ParentId { get; set; } = 0;

        /// <summary>
        /// 菜单名称
        /// </summary>
        [SugarColumn(ColumnName = "name", Length = 100, IsNullable = false)]
        public string Name { get; set; }

        /// <summary>
        /// 菜单代码
        /// </summary>
        [SugarColumn(ColumnName = "code", Length = 50, IsNullable = false)]
        public string Code { get; set; }

        /// <summary>
        /// 菜单URL
        /// </summary>
        [SugarColumn(ColumnName = "url", Length = 255, IsNullable = true)]
        public string? Url { get; set; }

        /// <summary>
        /// 菜单图标
        /// </summary>
        [SugarColumn(ColumnName = "icon", Length = 50, IsNullable = true)]
        public string? Icon { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        [SugarColumn(ColumnName = "sort", IsNullable = false)]
        public int Sort { get; set; } = 0;

        /// <summary>
        /// 是否激活
        /// </summary>
        [SugarColumn(ColumnName = "is_active", IsNullable = false)]
        public bool IsActive { get; set; } = true;
    }
}