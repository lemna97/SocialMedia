namespace Highever.SocialMedia.API.Areas.TikTok.DTO
{
    /// <summary>
    /// TikTok视频查询请求
    /// </summary>
    public class TiktokVideosQueryRequest
    {
        /// <summary>
        /// 关键词（用户名、昵称或视频描述）
        /// </summary>
        public string? Keyword { get; set; }

        /// <summary>
        /// 页码（从1开始）
        /// </summary>
        public int? PageIndex { get; set; } = 1;

        /// <summary>
        /// 每页大小
        /// </summary>
        public int? PageSize { get; set; } = 20;

        /// <summary>
        /// 排序字段
        /// </summary>
        public string? OrderBy { get; set; } = "UpdatedAt";

        /// <summary>
        /// 是否升序
        /// </summary>
        public bool Ascending { get; set; } = false;

        /// <summary>
        /// 视频创建开始时间（Unix时间戳，可为null或0）
        /// </summary>
        public long? CreateTimeStart { get; set; }

        /// <summary>
        /// 视频创建结束时间（Unix时间戳，可为null或0）
        /// </summary>
        public long? CreateTimeEnd { get; set; }

        /// <summary>
        /// 记录创建开始时间（可为null或空字符串）
        /// </summary>
        public DateTime? CreatedStartDate { get; set; }

        /// <summary>
        /// 记录创建结束时间（可为null或空字符串）
        /// </summary>
        public DateTime? CreatedEndDate { get; set; }
    }

    /// <summary>
    /// TikTok视频每日记录查询请求
    /// </summary>
    public class TiktokVideosDailyQueryRequest
    {
        /// <summary>
        /// 关键词（用户名、昵称或视频描述）
        /// </summary>
        public string? Keyword { get; set; }

        /// <summary>
        /// 页码（从1开始）
        /// </summary>
        public int? PageIndex { get; set; } = 1;

        /// <summary>
        /// 每页大小
        /// </summary>
        public int? PageSize { get; set; } = 20;

        /// <summary>
        /// 排序字段
        /// </summary>
        public string? OrderBy { get; set; } = "RecordDate";

        /// <summary>
        /// 是否升序
        /// </summary>
        public bool Ascending { get; set; } = false;

        /// <summary>
        /// 记录日期开始时间（可为null或空字符串）
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// 记录日期结束时间（可为null或空字符串）
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// 创建时间开始时间（可为null或空字符串）
        /// </summary>
        public DateTime? CreatedStartDate { get; set; }

        /// <summary>
        /// 创建时间结束时间（可为null或空字符串）
        /// </summary>
        public DateTime? CreatedEndDate { get; set; }
    }

    /// <summary>
    /// TikTok视频响应DTO
    /// </summary>
    public class TiktokVideosResponse
    {
        /// <summary>
        /// 视频ID
        /// </summary>
        public string AwemeId { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string UniqueId { get; set; }

        /// <summary>
        /// 用户昵称
        /// </summary>
        public string? Nickname { get; set; }

        /// <summary>
        /// 视频描述
        /// </summary>
        public string? Desc { get; set; }

        /// <summary>
        /// 视频创建时间戳
        /// </summary>
        public long? CreateTime { get; set; }

        /// <summary>
        /// 视频时长（秒）
        /// </summary>
        public double? Duration { get; set; }

        /// <summary>
        /// 视频封面图URL
        /// </summary>
        public string? CoverUrl { get; set; }

        /// <summary>
        /// 视频播放URL
        /// </summary>
        public string? PlayUrl { get; set; }

        /// <summary>
        /// 播放次数
        /// </summary>
        public int PlayCount { get; set; }

        /// <summary>
        /// 点赞数
        /// </summary>
        public int DiggCount { get; set; }

        /// <summary>
        /// 评论数
        /// </summary>
        public int CommentCount { get; set; }

        /// <summary>
        /// 分享数
        /// </summary>
        public int ShareCount { get; set; }

        /// <summary>
        /// 收藏数
        /// </summary>
        public int CollectCount { get; set; }

        /// <summary>
        /// 是否置顶
        /// </summary>
        public bool IsTop { get; set; }

        /// <summary>
        /// 是否广告
        /// </summary>
        public bool IsAds { get; set; }

        /// <summary>
        /// 是否AI创作
        /// </summary>
        public bool CreatedByAi { get; set; }

        /// <summary>
        /// 分享链接
        /// </summary>
        public string? ShareUrl { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// TikTok视频每日记录响应DTO
    /// </summary>
    public class TiktokVideosDailyResponse
    {
        /// <summary>
        /// 主键ID
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 视频ID
        /// </summary>
        public string AwemeId { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string UniqueId { get; set; }

        /// <summary>
        /// 用户昵称
        /// </summary>
        public string? Nickname { get; set; }

        /// <summary>
        /// 视频描述
        /// </summary>
        public string? Desc { get; set; }

        /// <summary>
        /// 播放次数
        /// </summary>
        public int PlayCount { get; set; }

        /// <summary>
        /// 点赞数
        /// </summary>
        public int DiggCount { get; set; }

        /// <summary>
        /// 评论数
        /// </summary>
        public int CommentCount { get; set; }

        /// <summary>
        /// 分享数
        /// </summary>
        public int ShareCount { get; set; }

        /// <summary>
        /// 收藏数
        /// </summary>
        public int CollectCount { get; set; }

        /// <summary>
        /// 转发数
        /// </summary>
        public int ForwardCount { get; set; }

        /// <summary>
        /// 是否置顶
        /// </summary>
        public bool IsTop { get; set; }

        /// <summary>
        /// 是否广告
        /// </summary>
        public bool IsAds { get; set; }

        /// <summary>
        /// 记录日期
        /// </summary>
        public DateTime RecordDate { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime UpdatedAt { get; set; }
    }
}


