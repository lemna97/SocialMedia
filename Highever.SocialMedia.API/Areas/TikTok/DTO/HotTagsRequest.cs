using System.ComponentModel.DataAnnotations;

namespace Highever.SocialMedia.API.Areas.TikTok.DTO
{
    /// <summary>
    /// 热门标签查询请求
    /// </summary>
    public class HotTagsQueryRequest
    {
        /// <summary>
        /// 关键词（标签名称）
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
        /// 排序字段（rank）
        /// </summary> 
        public string? OrderBy { get; set; } = "Rank";

        /// <summary>
        /// 是否升序
        /// </summary>
        public bool Ascending { get; set; } = true;

        /// <summary>
        /// 排序类型过滤：popular=热门，new=最新
        /// </summary>
        public string? SortType { get; set; }

        /// <summary>
        /// 国家代码过滤
        /// </summary>
        public string? CountryId { get; set; }

        /// <summary>
        /// 行业ID过滤
        /// </summary>
        public long? IndustryId { get; set; }

        /// <summary>
        /// 是否推广标签过滤
        /// </summary>
        //public bool? IsPromoted { get; set; }

        /// <summary>
        /// 查询时间
        /// </summary> 
        public DateTime? QueryDate { get; set; } = DateTime.Now.Date;

        /// <summary>
        /// 最小排名
        /// </summary>
        public int? MinRank { get; set; }

        /// <summary>
        /// 最大排名
        /// </summary>
        public int? MaxRank { get; set; }
    }
}