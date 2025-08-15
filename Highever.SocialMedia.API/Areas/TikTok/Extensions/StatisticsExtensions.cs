using Highever.SocialMedia.API.Areas.TikTok.DTO;

namespace Highever.SocialMedia.API.Areas.TikTok.Extensions
{
    /// <summary>
    /// 统计数据扩展方法
    /// </summary>
    public static class StatisticsExtensions
    {
        /// <summary>
        /// 根据类型获取增长统计
        /// </summary>
        public static GrowthStatistics? GetGrowthByType(this List<StatisticsItem> items, StatisticsType type)
        {
            return items.FirstOrDefault(x => x.Type == type)?.Growth;
        }

        /// <summary>
        /// 根据类型获取比率统计
        /// </summary>
        public static RateStatistics? GetRateByType(this List<RateStatisticsItem> items, RateType type)
        {
            return items.FirstOrDefault(x => x.Type == type)?.Rate;
        }

        /// <summary>
        /// 转换为字典格式（便于前端使用）
        /// </summary>
        public static Dictionary<string, GrowthStatistics> ToGrowthDictionary(this List<StatisticsItem> items)
        {
            return items.ToDictionary(x => x.Type.ToString(), x => x.Growth);
        }

        /// <summary>
        /// 转换为字典格式（便于前端使用）
        /// </summary>
        public static Dictionary<string, RateStatistics> ToRateDictionary(this List<RateStatisticsItem> items)
        {
            return items.ToDictionary(x => x.Type.ToString(), x => x.Rate);
        }
    }
}