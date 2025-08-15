using SqlSugar;

namespace Highever.SocialMedia.Domain.Entity
{
    /// <summary>
    /// TikTok视频每日数据记录表(用于跟踪视频数据的历史变化)
    /// </summary>
    [SugarTable("tiktok_videos_daily")]
    public class TiktokVideosDaily
    {
        /// <summary>
        /// 视频ID(TikTok视频唯一标识)
        /// </summary>
        [SugarColumn(ColumnName = "id", IsPrimaryKey = true, IsNullable = false, Length = 50)]
        public string Id { get; set; }

        /// <summary>
        /// 用户ID(发布视频的用户唯一标识)
        /// </summary>
        [SugarColumn(ColumnName = "user_id", IsNullable = false, Length = 50)]
        public string UserId { get; set; }

        /// <summary>
        /// 用户名(用户自定义的唯一标识符)
        /// </summary>
        [SugarColumn(ColumnName = "unique_id", Length = 100, IsNullable = false)]
        public string UniqueId { get; set; }

        /// <summary>
        /// 用户昵称(显示名称)
        /// </summary>
        [SugarColumn(ColumnName = "nickname", Length = 200, IsNullable = true)]
        public string? Nickname { get; set; }

        /// <summary>
        /// 视频描述(视频标题或描述内容)
        /// </summary>
        [SugarColumn(ColumnName = "desc", IsNullable = true)]
        public string? Desc { get; set; }

        /// <summary>
        /// 视频创建时间戳(Unix时间戳)
        /// </summary>
        [SugarColumn(ColumnName = "create_time", IsNullable = true)]
        public long? CreateTime { get; set; }

        /// <summary>
        /// 视频时长(毫秒)
        /// </summary>
        [SugarColumn(ColumnName = "duration", IsNullable = true)]
        public int? Duration { get; set; }

        /// <summary>
        /// 播放次数(视频观看数量)
        /// </summary>
        [SugarColumn(ColumnName = "play_count", IsNullable = false)]
        public int PlayCount { get; set; } = 0;

        /// <summary>
        /// 点赞数量(用户点赞次数)
        /// </summary>
        [SugarColumn(ColumnName = "digg_count", IsNullable = false)]
        public int DiggCount { get; set; } = 0;

        /// <summary>
        /// 评论数量(用户评论次数)
        /// </summary>
        [SugarColumn(ColumnName = "comment_count", IsNullable = false)]
        public int CommentCount { get; set; } = 0;

        /// <summary>
        /// 下载次数(视频下载数量)
        /// </summary>
        [SugarColumn(ColumnName = "download_count", IsNullable = false)]
        public int DownloadCount { get; set; } = 0;

        /// <summary>
        /// 分享次数(视频分享数量)
        /// </summary>
        [SugarColumn(ColumnName = "share_count", IsNullable = false)]
        public int ShareCount { get; set; } = 0;

        /// <summary>
        /// 记录日期(数据采集日期)
        /// </summary>
        [SugarColumn(ColumnName = "record_date", IsPrimaryKey = true, IsNullable = false)]
        public DateTime RecordDate { get; set; } = DateTime.Today;

        /// <summary>
        /// 创建时间(记录首次插入时间)
        /// </summary>
        [SugarColumn(ColumnName = "created_at", IsNullable = false)]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// 更新时间(记录最后修改时间)
        /// </summary>
        [SugarColumn(ColumnName = "updated_at", IsNullable = false)]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// 粉丝数量
        /// </summary>
        [SugarColumn(ColumnName = "follower_count", IsNullable = false)]
        public int FollowerCount { get; set; } = 0;

        /// <summary>
        /// 关注数量
        /// </summary>
        [SugarColumn(ColumnName = "following_count", IsNullable = false)]
        public int FollowingCount { get; set; } = 0;

        /// <summary>
        /// 用户发布视频总数
        /// </summary>
        [SugarColumn(ColumnName = "aweme_count", IsNullable = false)]
        public int AwemeCount { get; set; } = 0;

        /// <summary>
        /// 获赞总数
        /// </summary>
        [SugarColumn(ColumnName = "total_favorited", IsNullable = false)]
        public int TotalFavorited { get; set; } = 0;

        /// <summary>
        /// 用户喜欢他人视频数量
        /// </summary>
        [SugarColumn(ColumnName = "favoriting_count", IsNullable = false)]
        public int FavoritingCount { get; set; } = 0;

        /// <summary>
        /// 收藏数量
        /// </summary>
        [SugarColumn(ColumnName = "collect_count", IsNullable = false)]
        public int CollectCount { get; set; } = 0;

        /// <summary>
        /// 转发数量
        /// </summary>
        [SugarColumn(ColumnName = "forward_count", IsNullable = false)]
        public int ForwardCount { get; set; } = 0;

        /// <summary>
        /// WhatsApp分享数量
        /// </summary>
        [SugarColumn(ColumnName = "whatsapp_share_count", IsNullable = false)]
        public int WhatsappShareCount { get; set; } = 0;

        /// <summary>
        /// 是否允许评论
        /// </summary>
        [SugarColumn(ColumnName = "allow_comment", IsNullable = false)]
        public bool AllowComment { get; set; } = false;

        /// <summary>
        /// 是否允许分享
        /// </summary>
        [SugarColumn(ColumnName = "allow_share", IsNullable = false)]
        public bool AllowShare { get; set; } = false;

        /// <summary>
        /// 是否允许下载
        /// </summary>
        [SugarColumn(ColumnName = "allow_download", IsNullable = false)]
        public bool AllowDownload { get; set; } = false;

        /// <summary>
        /// 是否允许合拍
        /// </summary>
        [SugarColumn(ColumnName = "allow_duet", IsNullable = false)]
        public bool AllowDuet { get; set; } = false;

        /// <summary>
        /// 是否允许拼接
        /// </summary>
        [SugarColumn(ColumnName = "allow_stitch", IsNullable = false)]
        public bool AllowStitch { get; set; } = false;

        /// <summary>
        /// 是否置顶
        /// </summary>
        [SugarColumn(ColumnName = "is_top", IsNullable = false)]
        public bool IsTop { get; set; } = false;

        /// <summary>
        /// 是否广告视频
        /// </summary>
        [SugarColumn(ColumnName = "is_ads", IsNullable = false)]
        public bool IsAds { get; set; } = false;

        /// <summary>
        /// 推广按钮文字
        /// </summary>
        [SugarColumn(ColumnName = "promote_icon_text", Length = 100, IsNullable = true)]
        public string? PromoteIconText { get; set; }

        /// <summary>
        /// 话题标签集合(JSON格式)
        /// </summary>
        [SugarColumn(ColumnName = "cha_names", ColumnDataType = "TEXT", IsNullable = true)]
        public string? ChaNames { get; set; }

        /// <summary>
        /// 封面标签(JSON格式)
        /// </summary>
        [SugarColumn(ColumnName = "cover_labels", ColumnDataType = "TEXT", IsNullable = true)]
        public string? CoverLabels { get; set; }

        /// <summary>
        /// AI生成内容标记类型
        /// </summary>
        [SugarColumn(ColumnName = "aigc_label_type", IsNullable = false)]
        public int AigcLabelType { get; set; } = 0;

        /// <summary>
        /// 是否AI生成
        /// </summary>
        [SugarColumn(ColumnName = "created_by_ai", IsNullable = false)]
        public bool CreatedByAi { get; set; } = false;

        /// <summary>
        /// 是否有水印
        /// </summary>
        [SugarColumn(ColumnName = "has_watermark", IsNullable = false)]
        public bool HasWatermark { get; set; } = false;

        /// <summary>
        /// 封面图片URL
        /// </summary>
        [SugarColumn(ColumnName = "cover_url", Length = 500, IsNullable = true)]
        public string? CoverUrl { get; set; }

        /// <summary>
        /// 话题标签(JSON格式)
        /// </summary>
        [SugarColumn(ColumnName = "hashtags", ColumnDataType = "TEXT", IsNullable = true)]
        public string? Hashtags { get; set; }

        /// <summary>
        /// 发布地区代码
        /// </summary>
        [SugarColumn(ColumnName = "region", Length = 10, IsNullable = true)]
        public string? Region { get; set; }
    }
}

