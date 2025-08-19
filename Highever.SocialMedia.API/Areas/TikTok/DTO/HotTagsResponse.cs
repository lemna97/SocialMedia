namespace Highever.SocialMedia.API.Areas.TikTok.DTO
{
    /// <summary>
    /// 热门标签查询响应
    /// </summary>
    public class HotTagsResponse
    {
        /// <summary>
        /// 主键ID
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 标签ID
        /// </summary>
        public string HashtagId { get; set; }

        /// <summary>
        /// 标签名称
        /// </summary>
        public string HashtagName { get; set; }

        /// <summary>
        /// 国家信息
        /// </summary>
        public CountryInfo Country { get; set; }

        /// <summary>
        /// 行业信息
        /// </summary>
        public IndustryInfo Industry { get; set; }

        /// <summary>
        /// 是否推广
        /// </summary>
        public bool IsPromoted { get; set; }

        /// <summary>
        /// 趋势数据
        /// </summary>
        public object TrendData { get; set; }

        /// <summary>
        /// 创作者数据
        /// </summary>
        public object CreatorsData { get; set; }

        /// <summary>
        /// 发布数量
        /// </summary>
        public long PublishCnt { get; set; }

        /// <summary>
        /// 视频观看量
        /// </summary>
        public long VideoViews { get; set; }

        /// <summary>
        /// 排名
        /// </summary>
        public int Rank { get; set; }

        /// <summary>
        /// 排名变化
        /// </summary>
        public int RankDiff { get; set; }

        /// <summary>
        /// 排名变化类型
        /// </summary>
        public int RankDiffType { get; set; }

        /// <summary>
        /// 排名变化描述
        /// </summary>
        public string RankDiffDescription { get; set; }

        /// <summary>
        /// 排序类型
        /// </summary>
        public string SortType { get; set; }

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

    /// <summary>
    /// 国家信息
    /// </summary>
    public class CountryInfo
    {
        /// <summary>
        /// 国家ID
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 国家名称
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// 国家标签
        /// </summary>
        public string Label { get; set; }
    }

    /// <summary>
    /// 行业信息
    /// </summary>
    public class IndustryInfo
    {
        /// <summary>
        /// 行业ID
        /// </summary>
        public long? Id { get; set; }

        /// <summary>
        /// 行业名称
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// 行业标签
        /// </summary>
        public string Label { get; set; }
    }
}