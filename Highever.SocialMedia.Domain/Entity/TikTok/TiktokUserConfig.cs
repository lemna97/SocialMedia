using SqlSugar;

namespace Highever.SocialMedia.Domain.Entity
{
    /// <summary>
    /// TikTok用户配置表
    /// </summary>
    [SugarTable("tiktok_user_config")]
    public class TiktokUserConfig
    {
        /// <summary>
        /// 主键ID
        /// </summary>
        [SugarColumn(ColumnName = "id", IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; }

        /// <summary>
        /// 用户名(用户自定义的唯一标识符)
        /// </summary>
        [SugarColumn(ColumnName = "unique_id", Length = 100, IsNullable = false)]
        public string UniqueId { get; set; }

        /// <summary>
        /// 安全用户ID(TikTok内部安全标识)
        /// </summary>
        [SugarColumn(ColumnName = "sec_uid", Length = 200, IsNullable = true)]
        public string? SecUid { get; set; } = string.Empty;

        /// <summary>
        /// 是否启用(配置状态标识)
        /// </summary>
        [SugarColumn(ColumnName = "is_active", IsNullable = false)]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// 创建时间
        /// </summary>
        [SugarColumn(ColumnName = "created_at", IsNullable = false)]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// 更新时间
        /// </summary>
        [SugarColumn(ColumnName = "updated_at", IsNullable = false)]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}