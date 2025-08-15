using System.ComponentModel.DataAnnotations;

namespace Highever.SocialMedia.API.Areas.TikTok.DTO
{
    /// <summary>
    /// 账号增量统计请求
    /// </summary>
    public class AccountStatisticsRequest
    {
        /// <summary>
        /// 开始日期（默认30天前）
        /// </summary>
        public DateTime? StartDate { get; set; } = DateTime.Today.AddDays(-30);

        /// <summary>
        /// 结束日期（默认今天）
        /// </summary>
        public DateTime? EndDate { get; set; } = DateTime.Today;

        /// <summary>
        /// 账号UniqueId（可选，为空则查询所有有权限的账号）
        /// </summary>
        public string? UniqueId { get; set; }
    }

    /// <summary>
    /// 账号增量统计响应
    /// </summary>
    public class AccountStatisticsResponse
    {
        /// <summary>
        /// 账号UniqueId
        /// </summary>
        public string UniqueId { get; set; }

        /// <summary>
        /// 账号昵称
        /// </summary>
        public string? Nickname { get; set; }

        /// <summary>
        /// 统计日期范围
        /// </summary>
        public string DateRange { get; set; }

        /// <summary>
        /// 粉丝数增量统计 - 包含当前值、增长量、增长率等详细数据
        /// </summary>
        public GrowthStatistics FollowerGrowth { get; set; }

        /// <summary>
        /// 关注数增量统计 - 包含当前值、增长量、增长率等详细数据
        /// </summary>
        public GrowthStatistics FollowingGrowth { get; set; }

        /// <summary>
        /// 视频数增量统计 - 包含当前值、增长量、增长率等详细数据
        /// </summary>
        public GrowthStatistics VideoGrowth { get; set; }

        /// <summary>
        /// 获赞总数增量统计 - 包含当前值、增长量、增长率等详细数据
        /// </summary>
        public GrowthStatistics HeartGrowth { get; set; }

        /// <summary>
        /// 点赞数增量统计 - 包含当前值、增长量、增长率等详细数据
        /// </summary>
        public GrowthStatistics DiggGrowth { get; set; }

        /// <summary>
        /// 朋友数增量统计 - 包含当前值、增长量、增长率等详细数据
        /// </summary>
        public GrowthStatistics FriendGrowth { get; set; }

        /// <summary>
        /// 点赞率统计 - 获赞总数/视频总数，包含当前比率、变化趋势等
        /// </summary>
        public RateStatistics LikeRate { get; set; }

        /// <summary>
        /// 平均获赞数统计 - 获赞总数/视频总数，反映内容质量
        /// </summary>
        public RateStatistics AverageHeartCount { get; set; }

        /// <summary>
        /// 每日数据趋势
        /// </summary>
        public List<DailyTrend> DailyTrends { get; set; } = new();
    }

    /// <summary>
    /// 视频增量统计请求
    /// </summary>
    public class VideoStatisticsRequest
    {
        /// <summary>
        /// 开始日期（默认30天前）
        /// </summary>
        [Required]
        public DateTime? StartDate { get; set; } = DateTime.Today.AddDays(-30);

        /// <summary>
        /// 结束日期（默认今天）
        /// </summary>
        [Required]
        public DateTime? EndDate { get; set; } = DateTime.Today;

        /// <summary>
        /// 视频ID（可选）
        /// </summary>
        public string? VideoId { get; set; }

        /// <summary>
        /// 账号UniqueId（可选）
        /// </summary>
        public string? UniqueId { get; set; }

        /// <summary>
        /// 页码
        /// </summary>
        public int PageIndex { get; set; } = 1;

        /// <summary>
        /// 每页大小
        /// </summary>
        public int PageSize { get; set; } = 20;
    }

    /// <summary>
    /// 视频增量统计响应
    /// </summary>
    public class VideoStatisticsResponse
    {
        /// <summary>
        /// 视频ID
        /// </summary>
        public string VideoId { get; set; }

        /// <summary>
        /// 账号UniqueId
        /// </summary>
        public string UniqueId { get; set; }

        /// <summary>
        /// 账号昵称
        /// </summary>
        public string? Nickname { get; set; }

        /// <summary>
        /// 视频描述
        /// </summary>
        public string? Desc { get; set; }

        /// <summary>
        /// 视频创建时间
        /// </summary>
        public long? CreateTime { get; set; }

        /// <summary>
        /// 统计日期范围
        /// </summary>
        public string DateRange { get; set; }

        /// <summary>
        /// 播放数增量统计
        /// </summary>
        public GrowthStatistics PlayGrowth { get; set; }

        /// <summary>
        /// 点赞数增量统计
        /// </summary>
        public GrowthStatistics DiggGrowth { get; set; }

        /// <summary>
        /// 评论数增量统计
        /// </summary>
        public GrowthStatistics CommentGrowth { get; set; }

        /// <summary>
        /// 分享数增量统计
        /// </summary>
        public GrowthStatistics ShareGrowth { get; set; }

        /// <summary>
        /// 收藏数增量统计
        /// </summary>
        public GrowthStatistics CollectGrowth { get; set; }

        /// <summary>
        /// 转发数增量统计
        /// </summary>
        public GrowthStatistics ForwardGrowth { get; set; }

        /// <summary>
        /// 下载数增量统计
        /// </summary>
        public GrowthStatistics DownloadGrowth { get; set; }

        /// <summary>
        /// 点赞率统计（点赞数/播放数）
        /// </summary>
        public RateStatistics LikeRate { get; set; }

        /// <summary>
        /// 互动率统计（(点赞+评论+分享)/播放数）
        /// </summary>
        public RateStatistics EngagementRate { get; set; }

        /// <summary>
        /// 粉丝转换率统计（预留字段，暂时为空）
        /// </summary>
        public RateStatistics? FollowerConversionRate { get; set; }

        /// <summary>
        /// 评论率统计（评论数/播放数）
        /// </summary>
        public RateStatistics CommentRate { get; set; }

        /// <summary>
        /// 分享率统计（分享数/播放数）
        /// </summary>
        public RateStatistics ShareRate { get; set; }

        /// <summary>
        /// 收藏率统计（收藏数/播放数）
        /// </summary>
        public RateStatistics CollectRate { get; set; }

        /// <summary>
        /// 每日数据趋势
        /// </summary>
        public List<DailyTrend> DailyTrends { get; set; } = new();
    }
     
    public class GrowthStatistics
    {
        /// <summary>
        /// 当前值
        /// </summary>
        public int CurrentValue { get; set; }

        /// <summary>
        /// 前一天值
        /// </summary>
        public int PreviousValue { get; set; }

        /// <summary>
        /// 绝对增长量
        /// </summary>
        public int Growth { get; set; }

        /// <summary>
        /// 增长率（百分比）
        /// </summary>
        public decimal GrowthRate { get; set; }

        /// <summary>
        /// 期间最高值
        /// </summary>
        public int MaxValue { get; set; }

        /// <summary>
        /// 期间最低值
        /// </summary>
        public int MinValue { get; set; }

        /// <summary>
        /// 期间平均值
        /// </summary>
        public decimal AvgValue { get; set; }
    }
     
    public class RateStatistics
    {
        /// <summary>
        /// 当前比率（百分比）
        /// </summary>
        public decimal CurrentRate { get; set; }

        /// <summary>
        /// 前一天比率（百分比）
        /// </summary>
        public decimal PreviousRate { get; set; }

        /// <summary>
        /// 比率变化（百分点）
        /// </summary>
        public decimal RateChange { get; set; }

        /// <summary>
        /// 期间最高比率
        /// </summary>
        public decimal MaxRate { get; set; }

        /// <summary>
        /// 期间最低比率
        /// </summary>
        public decimal MinRate { get; set; }

        /// <summary>
        /// 期间平均比率
        /// </summary>
        public decimal AvgRate { get; set; }

        /// <summary>
        /// 计算说明
        /// </summary>
        public string? Description { get; set; }
    }

    /// <summary>
    /// 每日趋势数据
    /// </summary>
    public class DailyTrend
    {
        /// <summary>
        /// 日期
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// 粉丝数
        /// </summary>
        public int? FollowerCount { get; set; }

        /// <summary>
        /// 关注数
        /// </summary>
        public int? FollowingCount { get; set; }

        /// <summary>
        /// 视频数
        /// </summary>
        public int? VideoCount { get; set; }

        /// <summary>
        /// 获赞总数
        /// </summary>
        public int? HeartCount { get; set; }

        /// <summary>
        /// 播放数（视频专用）
        /// </summary>
        public int? PlayCount { get; set; }

        /// <summary>
        /// 点赞数（视频专用）
        /// </summary>
        public int? DiggCount { get; set; }

        /// <summary>
        /// 评论数（视频专用）
        /// </summary>
        public int? CommentCount { get; set; }

        /// <summary>
        /// 分享数（视频专用）
        /// </summary>
        public int? ShareCount { get; set; }

        /// <summary>
        /// 点赞率（视频专用）
        /// </summary>
        public decimal? LikeRate { get; set; }

        /// <summary>
        /// 互动率（视频专用）
        /// </summary>
        public decimal? EngagementRate { get; set; }
    }

    /// <summary>
    /// 账号视频统计请求
    /// </summary>
    public class AccountVideoStatisticsRequest
    {
        /// <summary>
        /// 开始日期（默认30天前）
        /// </summary>
        public DateTime? StartDate { get; set; } = DateTime.Today.AddDays(-30);

        /// <summary>
        /// 结束日期（默认今天）
        /// </summary>
        public DateTime? EndDate { get; set; } = DateTime.Today;

        /// <summary>
        /// 账号UniqueId（可选，为空则查询所有有权限的账号）
        /// </summary>
        public string? UniqueId { get; set; }
    }

    /// <summary>
    /// 账号视频统计响应（重构版）
    /// </summary>
    public class AccountVideoStatisticsResponse
    {
        /// <summary>
        /// 账号UniqueId
        /// </summary>
        public string UniqueId { get; set; }

        /// <summary>
        /// 账号昵称
        /// </summary>
        public string? Nickname { get; set; }

        /// <summary>
        /// 统计日期范围
        /// </summary>
        public string DateRange { get; set; }

        /// <summary>
        /// 视频总数
        /// </summary>
        public int VideoCount { get; set; }

        /// <summary>
        /// 增量统计项列表
        /// </summary>
        public List<StatisticsItem> GrowthStatistics { get; set; } = new();

        /// <summary>
        /// 比率统计项列表
        /// </summary>
        public List<RateStatisticsItem> RateStatistics { get; set; } = new();

        /// <summary>
        /// 视频表现分布
        /// </summary>
        public VideoPerformanceDistribution VideoPerformanceDistribution { get; set; }

        // 为了向后兼容，保留原有字段（标记为过时）
        [Obsolete("请使用GrowthStatistics集合")]
        public GrowthStatistics TotalPlayGrowth { get; set; }
        
        [Obsolete("请使用GrowthStatistics集合")]
        public GrowthStatistics TotalDiggGrowth { get; set; }
        
        [Obsolete("请使用GrowthStatistics集合")]
        public GrowthStatistics TotalCommentGrowth { get; set; }
        
        [Obsolete("请使用GrowthStatistics集合")]
        public GrowthStatistics TotalShareGrowth { get; set; }
        
        [Obsolete("请使用GrowthStatistics集合")]
        public GrowthStatistics TotalCollectGrowth { get; set; }
        
        [Obsolete("请使用RateStatistics集合")]
        public RateStatistics OverallEngagementRate { get; set; }
        
        [Obsolete("请使用RateStatistics集合")]
        public RateStatistics OverallLikeRate { get; set; }
    }

    /// <summary>
    /// 视频表现分布
    /// </summary>
    public class VideoPerformanceDistribution
    {
        /// <summary>
        /// 视频总数
        /// </summary>
        public int TotalVideos { get; set; }

        /// <summary>
        /// 高表现视频数（互动率>=5%）
        /// </summary>
        public int HighPerformanceVideos { get; set; }

        /// <summary>
        /// 中等表现视频数（2%<=互动率<5%）
        /// </summary>
        public int MediumPerformanceVideos { get; set; }

        /// <summary>
        /// 低表现视频数（互动率<2%）
        /// </summary>
        public int LowPerformanceVideos { get; set; }

        /// <summary>
        /// 平均互动率
        /// </summary>
        public decimal AverageEngagementRate { get; set; }
    }

    /// <summary>
    /// 统计项类型枚举
    /// </summary>
    public enum StatisticsType
    {
        /// <summary>
        /// 播放数
        /// </summary>
        Play = 1,
        
        /// <summary>
        /// 点赞数
        /// </summary>
        Digg = 2,
        
        /// <summary>
        /// 评论数
        /// </summary>
        Comment = 3,
        
        /// <summary>
        /// 分享数
        /// </summary>
        Share = 4,
        
        /// <summary>
        /// 收藏数
        /// </summary>
        Collect = 5,
        
        /// <summary>
        /// 转发数
        /// </summary>
        Forward = 6,
        
        /// <summary>
        /// 下载数
        /// </summary>
        Download = 7
    }

    /// <summary>
    /// 统计项
    /// </summary>
    public class StatisticsItem
    {
        /// <summary>
        /// 统计类型
        /// </summary>
        public StatisticsType Type { get; set; }
        
        /// <summary>
        /// 统计类型名称
        /// </summary>
        public string TypeName { get; set; }
        
        /// <summary>
        /// 增长统计数据
        /// </summary>
        public GrowthStatistics Growth { get; set; }
        
        /// <summary>
        /// 比率统计数据（可选，如点赞率、互动率等）
        /// </summary>
        public RateStatistics? Rate { get; set; }
    }

    /// <summary>
    /// 比率项类型枚举
    /// </summary>
    public enum RateType
    {
        /// <summary>
        /// 点赞率
        /// </summary>
        LikeRate = 1,
        
        /// <summary>
        /// 互动率
        /// </summary>
        EngagementRate = 2,
        
        /// <summary>
        /// 评论率
        /// </summary>
        CommentRate = 3,
        
        /// <summary>
        /// 分享率
        /// </summary>
        ShareRate = 4,
        
        /// <summary>
        /// 收藏率
        /// </summary>
        CollectRate = 5
    }

    /// <summary>
    /// 比率统计项
    /// </summary>
    public class RateStatisticsItem
    {
        /// <summary>
        /// 比率类型
        /// </summary>
        public RateType Type { get; set; }
        
        /// <summary>
        /// 比率类型名称
        /// </summary>
        public string TypeName { get; set; }
        
        /// <summary>
        /// 比率统计数据
        /// </summary>
        public RateStatistics Rate { get; set; }
    }
}



