using SqlSugar;
using System.ComponentModel.DataAnnotations;

namespace Highever.SocialMedia.Domain.Entity
{
    /// <summary>
    /// 热门标签视频实体
    /// </summary>
    [SugarTable("tiktok_hot_tags_video")]
    public class HotTagsVideo
    {
        /// <summary>
        /// 主键ID
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public long Id { get; set; }

        /// <summary>
        /// 标签ID
        /// </summary>
        [SugarColumn(ColumnName = "hashtag_id", Length = 50, IsNullable = false)]
        [Required]
        public string HashtagId { get; set; }

        /// <summary>
        /// 标签名称
        /// </summary>
        [SugarColumn(ColumnName = "hashtag_name", Length = 255, IsNullable = false)]
        [Required]
        public string HashtagName { get; set; }

        /// <summary>
        /// 国家ID
        /// </summary>
        [SugarColumn(ColumnName = "country_id", Length = 10)]
        public string CountryId { get; set; }

        /// <summary>
        /// 国家名称
        /// </summary>
        [SugarColumn(ColumnName = "country_value", Length = 100)]
        public string CountryValue { get; set; }

        /// <summary>
        /// 国家标签
        /// </summary>
        [SugarColumn(ColumnName = "country_label", Length = 50)]
        public string CountryLabel { get; set; }

        /// <summary>
        /// 行业ID
        /// </summary>
        [SugarColumn(ColumnName = "industry_id")]
        public long? IndustryId { get; set; }

        /// <summary>
        /// 行业名称
        /// </summary>
        [SugarColumn(ColumnName = "industry_value", Length = 255)]
        public string IndustryValue { get; set; }

        /// <summary>
        /// 行业标签
        /// </summary>
        [SugarColumn(ColumnName = "industry_label", Length = 100)]
        public string IndustryLabel { get; set; }

        /// <summary>
        /// 是否推广
        /// </summary>
        [SugarColumn(ColumnName = "is_promoted")]
        public bool IsPromoted { get; set; } = false;

        /// <summary>
        /// 趋势数据JSON
        /// </summary>
        [SugarColumn(ColumnName = "trend_data", ColumnDataType = "json")]
        public string TrendData { get; set; }

        /// <summary>
        /// 创作者数据JSON
        /// </summary>
        [SugarColumn(ColumnName = "creators_data", ColumnDataType = "json")]
        public string CreatorsData { get; set; }

        /// <summary>
        /// 发布数量
        /// </summary>
        [SugarColumn(ColumnName = "publish_cnt")]
        public long PublishCnt { get; set; } = 0;

        /// <summary>
        /// 视频观看量
        /// </summary>
        [SugarColumn(ColumnName = "video_views")]
        public long VideoViews { get; set; } = 0;

        /// <summary>
        /// 排名
        /// </summary>
        [SugarColumn(ColumnName = "rank")]
        public int Rank { get; set; } = 0;

        /// <summary>
        /// 排名变化
        /// </summary>
        [SugarColumn(ColumnName = "rank_diff")]
        public int RankDiff { get; set; } = 0;

        /// <summary>
        /// 排名变化类型
        /// </summary>
        [SugarColumn(ColumnName = "rank_diff_type")]
        public int RankDiffType { get; set; } = 0;

        /// <summary>
        /// 排序类型：popular=热门，new=最新
        /// </summary>
        [SugarColumn(ColumnName = "sort_type", Length = 20, IsNullable = false)]
        [Required]
        public string SortType { get; set; }

        /// <summary>
        /// 记录日期(yyyy-MM-dd)
        /// </summary>
        [SugarColumn(ColumnName = "record_date", IsNullable = false)]
        [Required]
        public DateTime RecordDate { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [SugarColumn(ColumnName = "created_at", IsOnlyIgnoreUpdate = true)]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// 更新时间
        /// </summary>
        [SugarColumn(ColumnName = "updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}