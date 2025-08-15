namespace Highever.SocialMedia.API.Areas.TikTok.DTO
{
    /// <summary>
    /// TikTok用户查询请求
    /// </summary>
    public class TiktokUsersQueryRequest
    {
        /// <summary>
        /// 关键词（用户名或昵称，可为null或空字符串）
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
        /// 排序字段（可为null或空字符串）
        /// </summary>
        public string? OrderBy { get; set; } = "UpdatedAt";

        /// <summary>
        /// 是否升序
        /// </summary>
        public bool Ascending { get; set; } = false;
    }

    /// <summary>
    /// TikTok用户每日记录查询请求
    /// </summary>
    public class TiktokUsersDailyQueryRequest
    {
        /// <summary>
        /// 关键词（用户名或昵称，可为null或空字符串）
        /// </summary>
        public string? Keyword { get; set; }
        /// <summary>
        /// 用户的主键Id（long类型）
        /// </summary>
        public long? Id { get; set; }

        /// <summary>
        /// 页码（从1开始）
        /// </summary>
        public int? PageIndex { get; set; } = 1;

        /// <summary>
        /// 每页大小
        /// </summary>
        public int? PageSize { get; set; } = 20;

        /// <summary>
        /// 排序字段（可为null或空字符串）
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
}



