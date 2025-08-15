using SqlSugar;

namespace Highever.SocialMedia.Domain.Entity
{
    /// <summary>
    /// 权限表
    /// </summary>
    [SugarTable("system_permissions")]
    public class Permissions : BaseEntity
    {
        /// <summary>
        /// 资源ID
        /// </summary>
        [SugarColumn(ColumnName = "resource_id", IsNullable = false)]
        public int ResourceId { get; set; }

        /// <summary>
        /// 操作类型
        /// </summary>
        [SugarColumn(ColumnName = "operation", Length = 10, IsNullable = false)]
        public string Operation { get; set; }

        /// <summary>
        /// 权限代码
        /// </summary>
        [SugarColumn(ColumnName = "perm_code", Length = 100, IsNullable = false)]
        public string PermCode { get; set; }
    }
}

