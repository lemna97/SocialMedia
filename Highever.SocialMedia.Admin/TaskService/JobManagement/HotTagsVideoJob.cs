using Highever.SocialMedia.Application.Contracts;
using Highever.SocialMedia.Common;
using Highever.SocialMedia.Domain.Entity;
using Highever.SocialMedia.Domain.Repository;
using Newtonsoft.Json;
using System.Diagnostics;

namespace Highever.SocialMedia.Admin.TaskService
{
    /// <summary>
    /// 热门标签视频任务
    /// 定时获取TikTok热门标签排行榜数据
    /// </summary>
    public class HotTagsVideoJob : ITaskExecutor
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="serviceProvider">服务提供者</param>
        public HotTagsVideoJob(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// 获取仓储实例
        /// </summary>
        private IRepository<HotTagsVideo> Repository => _serviceProvider.GetRequiredService<IRepository<HotTagsVideo>>();

        /// <summary>
        /// 获取TikTok API服务
        /// </summary>
        private ITKAPIService TKAPIService => _serviceProvider.GetRequiredService<ITKAPIService>();

        /// <summary>
        /// 获取日志服务
        /// </summary>
        private INLogger Logger => _serviceProvider.GetRequiredService<INLogger>();

        /// <summary>
        /// 标签的类型
        /// </summary>
        private readonly string[] sortTypes = { 
            TikTokHotTagsApiConstants.SortTypes.Popular, 
            TikTokHotTagsApiConstants.SortTypes.New 
        };
        /// <summary>
        /// 执行热门标签视频数据获取任务
        /// </summary>
        /// <param name="taskName">任务名称</param>
        /// <returns></returns>
        public async Task Execute(string taskName)
        {
            await _semaphore.WaitAsync();

            var stopwatch = Stopwatch.StartNew();
            var totalProcessed = 0;
            var totalSuccess = 0;
            var totalFailed = 0;
            var actualApiCalls = 0; // 修正：使用实际API调用次数
            long? taskRunId = null;
            var recordDate = DateTime.Now.Date;

            try
            {
                // 关键日志：任务开始里程碑 - 入库
                Logger.TaskMilestone(taskName, "任务开始", null, taskRunId, new Dictionary<string, object>
                {
                    ["StartTime"] = DateTime.Now,
                    ["RecordDate"] = recordDate.ToString("yyyy-MM-dd")
                });

                // 统一日志：处理信息 - 仅文件
                Logger.TaskInfo(taskName, $"开始执行热门标签视频任务", null, taskRunId);

                foreach (var sortType in sortTypes)
                {
                    // 关键日志：处理里程碑 - 入库
                    Logger.TaskMilestone(taskName, $"开始处理排序类型", null, taskRunId, new Dictionary<string, object>
                    {
                        ["SortType"] = sortType
                    });

                    try
                    {
                        // 调用API获取热门标签数据（分页获取100条）
                        var (hotTagsList, apiCallCount) = await GetHotTagsFromAPI(sortType, taskName, taskRunId, 100);
                        actualApiCalls += apiCallCount; // 修正：累加实际API调用次数
                        
                        if (hotTagsList != null && hotTagsList.Any())
                        {
                            // 统一日志：处理信息 - 仅文件
                            Logger.TaskInfo(taskName, $"排序类型 {sortType} 获取到 {hotTagsList.Count} 条数据", null, taskRunId);

                            // 转换并保存数据
                            var saveResult = await SaveHotTagsToDatabase(hotTagsList, sortType, recordDate, taskName, taskRunId);
                            
                            totalProcessed += hotTagsList.Count;
                            totalSuccess += saveResult.successCount;
                            totalFailed += saveResult.failedCount;

                            // 关键日志：处理完成里程碑 - 入库
                            Logger.TaskMilestone(taskName, $"排序类型处理完成", null, taskRunId, new Dictionary<string, object>
                            {
                                ["SortType"] = sortType,
                                ["ProcessedCount"] = hotTagsList.Count,
                                ["SuccessCount"] = saveResult.successCount,
                                ["FailedCount"] = saveResult.failedCount
                            });
                        }
                        else
                        {
                            // 关键日志：处理错误 - 入库
                            Logger.TaskError(taskName, new InvalidOperationException($"排序类型 {sortType} API返回数据为空"), null, taskRunId);
                            totalFailed++;
                        }

                        // 排序类型间隔，避免API限流
                        await Task.Delay(2000);
                    }
                    catch (Exception ex)
                    {
                        // 关键日志：处理异常 - 入库
                        Logger.TaskError($"{taskName}-{sortType}", ex, null, taskRunId);
                        totalFailed++;
                    }
                }

                stopwatch.Stop();

                // 统一日志：控制台总结
                Logger.TaskInfo(taskName, $"热门标签任务执行完成 - 执行时间: {stopwatch.ElapsedMilliseconds}ms, 总处理数: {totalProcessed}, 成功数量: {totalSuccess}, 失败数量: {totalFailed}, API调用: {actualApiCalls}, 成功率: {(totalProcessed > 0 ? (totalSuccess * 100.0 / totalProcessed) : 0):F1}%", null, taskRunId);

                // 关键日志：任务完成 - 入库
                Logger.TaskComplete(taskName, stopwatch.ElapsedMilliseconds, totalProcessed, totalSuccess, totalFailed, actualApiCalls, null, taskRunId);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                // 关键日志：任务错误 - 入库
                Logger.TaskError(taskName, ex, null, taskRunId);
                throw;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        /// <summary>
        /// 从API获取热门标签数据（支持分页）
        /// </summary>
        /// <param name="sortType">排序类型：popular=热门，new=最新</param>
        /// <param name="taskName">任务名称</param>
        /// <param name="taskRunId">任务运行ID</param>
        /// <param name="totalLimit">总共需要获取的数据条数</param>
        /// <returns>API响应数据和实际API调用次数</returns>
        private async Task<(List<HotTagItem> hotTags, int apiCallCount)> GetHotTagsFromAPI(string sortType, string taskName, long? taskRunId, int totalLimit = 100)
        {
            var allHotTags = new List<HotTagItem>();
            var pageSize = 20; // 修正：固定页面大小
            var currentPage = 1;
            var hasMore = true;
            var actualApiCalls = 0; // 实际API调用次数

            try
            {
                Logger.TaskInfo(taskName, $"开始分页获取热门标签数据 - 排序类型: {sortType}, 目标总数: {totalLimit}", null, taskRunId);

                while (allHotTags.Count < totalLimit && hasMore && currentPage <= 10)
                {
                    var apiParams = new Dictionary<string, string>
                    {
                        { "page", currentPage.ToString() },
                        { "limit", pageSize.ToString() }, // 修正：固定页面大小，不动态变化
                        { "period", TikTokHotTagsApiConstants.Periods.Day7 },
                        { "country_code", TikTokHotTagsApiConstants.CountryCodes.US },
                        { "sort_by", sortType },
                        { "industry_id", "" },
                        { "filter_by", "" }
                    };
                    //new 这个字段无效？？
                    if (sortType == TikTokHotTagsApiConstants.SortTypes.New)
                    {
                        apiParams["filter_by"] = "new_on_board";
                        apiParams["sort_by"] = "popular";
                    }
                    var (apiResponse, errorMessage) = await TKAPIService.FetchHotTagsWithRetryAsync(apiParams);
                    actualApiCalls++; // 记录实际调用次数

                    // 修正：完整的响应状态检查
                    if (!string.IsNullOrEmpty(errorMessage))
                    {
                        Logger.TaskApiCall(taskName, $"HotTags-{sortType}-Page{currentPage}", false, $"API_FAIL:{errorMessage}", taskRunId);
                        break;
                    }

                    if (apiResponse?.Code != 200)
                    {
                        Logger.TaskApiCall(taskName, $"HotTags-{sortType}-Page{currentPage}", false, $"HTTP_ERROR:{apiResponse?.Code}", taskRunId);
                        break;
                    }

                    if (apiResponse.Data?.Code != 0)
                    {
                        Logger.TaskApiCall(taskName, $"HotTags-{sortType}-Page{currentPage}", false, $"API_ERROR:{apiResponse.Data?.Msg}", taskRunId);
                        break;
                    }

                    if (apiResponse.Data?.Data?.List == null || !apiResponse.Data.Data.List.Any())
                    {
                        Logger.TaskApiCall(taskName, $"HotTags-{sortType}-Page{currentPage}", false, "API_NO_DATA", taskRunId);
                        break;
                    }

                    Logger.TaskApiCall(taskName, $"HotTags-{sortType}-Page{currentPage}", true, null, taskRunId);

                    var pageData = apiResponse.Data.Data.List;
                    allHotTags.AddRange(pageData);
                    
                    hasMore = apiResponse.Data.Data.Pagination?.HasMore ?? false;
                    
                    Logger.TaskInfo(taskName, $"第 {currentPage} 页获取成功 - 排序类型: {sortType}, 获取条数: {pageData.Count}, 累计: {allHotTags.Count}, 还有更多: {hasMore}", null, taskRunId);

                    currentPage++;

                    if (hasMore && allHotTags.Count < totalLimit)
                    {
                        await Task.Delay(1000);
                    }
                }

                var finalResult = allHotTags.Take(totalLimit).ToList();
                Logger.TaskInfo(taskName, $"分页获取完成 - 排序类型: {sortType}, 总获取条数: {finalResult.Count}/{totalLimit}, 实际API调用: {actualApiCalls}", null, taskRunId);

                return (finalResult, actualApiCalls);
            }
            catch (Exception ex)
            {
                Logger.TaskError($"{taskName}-API-{sortType}", ex, null, taskRunId);
                return (allHotTags, actualApiCalls);
            }
        }

        /// <summary>
        /// 保存热门标签数据到数据库
        /// </summary>
        /// <param name="hotTagsList">热门标签列表</param>
        /// <param name="sortType">排序类型</param>
        /// <param name="recordDate">记录日期</param>
        /// <param name="taskName">任务名称</param>
        /// <param name="taskRunId">任务运行ID</param>
        /// <returns>保存结果统计</returns>
        private async Task<(int successCount, int failedCount)> SaveHotTagsToDatabase(
            List<HotTagItem> hotTagsList,
            string sortType,
            DateTime recordDate,
            string taskName,
            long? taskRunId)
        {
            var successCount = 0;
            var failedCount = 0;

            try
            {
                // 修正：先转换实体，如果转换失败直接返回
                var hotTagsEntities = new List<HotTagsVideo>();
                foreach (var item in hotTagsList)
                {
                    try
                    {
                        var entity = ConvertToEntity(item, sortType, recordDate);
                        hotTagsEntities.Add(entity);
                    }
                    catch (Exception ex)
                    {
                        Logger.TaskError($"ConvertToEntity-{item?.HashtagId}", ex, null, taskRunId);
                        failedCount++;
                    }
                }

                if (!hotTagsEntities.Any())
                {
                    Logger.TaskError($"{taskName}-Save-{sortType}", new InvalidOperationException("没有有效的实体可以保存"), null, taskRunId);
                    return (0, hotTagsList.Count);
                }

                // 修正：改进事务处理逻辑
                var success = await Repository.ExecuteTransactionAsync(async () =>
                {
                    foreach (var entity in hotTagsEntities)
                    {
                        try
                        {
                            var existingRecord = await Repository.FirstOrDefaultAsync(x =>
                                x.HashtagId == entity.HashtagId &&
                                x.SortType == entity.SortType &&
                                x.RecordDate == entity.RecordDate);

                            if (existingRecord != null)
                            {
                                // 更新现有记录
                                existingRecord.HashtagName = entity.HashtagName;
                                existingRecord.CountryId = entity.CountryId;
                                existingRecord.CountryValue = entity.CountryValue;
                                existingRecord.CountryLabel = entity.CountryLabel;
                                existingRecord.IndustryId = entity.IndustryId;
                                existingRecord.IndustryValue = entity.IndustryValue;
                                existingRecord.IndustryLabel = entity.IndustryLabel;
                                existingRecord.IsPromoted = entity.IsPromoted;
                                existingRecord.TrendData = entity.TrendData;
                                existingRecord.CreatorsData = entity.CreatorsData;
                                existingRecord.PublishCnt = entity.PublishCnt;
                                existingRecord.VideoViews = entity.VideoViews;
                                existingRecord.Rank = entity.Rank;
                                existingRecord.RankDiff = entity.RankDiff;
                                existingRecord.RankDiffType = entity.RankDiffType;
                                existingRecord.UpdatedAt = DateTime.Now;

                                await Repository.UpdateAsync(existingRecord);
                                Logger.TaskInfo(taskName, $"更新记录: {entity.HashtagId}", null, taskRunId);
                            }
                            else
                            {
                                await Repository.InsertAsync(entity);
                                Logger.TaskInfo(taskName, $"插入新记录: {entity.HashtagId}", null, taskRunId);
                            }

                            successCount++;
                        }
                        catch (Exception ex)
                        {
                            Logger.TaskError($"SaveSingleRecord-{entity.HashtagId}", ex, null, taskRunId);
                            // 修正：在事务中，单个记录失败让整个事务失败
                            throw new InvalidOperationException($"保存记录 {entity.HashtagId} 失败: {ex.Message}", ex);
                        }
                    }

                    return true;
                });

                // 修正：事务失败时重置计数器
                if (!success)
                {
                    Logger.TaskError($"{taskName}-Save-{sortType}", new InvalidOperationException("数据库事务执行失败"), null, taskRunId);
                    failedCount = hotTagsList.Count;
                    successCount = 0;
                }
            }
            catch (Exception ex)
            {
                Logger.TaskError($"{taskName}-Save-{sortType}", ex, null, taskRunId);
                failedCount = hotTagsList.Count;
                successCount = 0;
            }

            Logger.TaskInfo(taskName, $"数据库保存完成 - 排序类型: {sortType}, 成功: {successCount}, 失败: {failedCount}", null, taskRunId);
            return (successCount, failedCount);
        }

        /// <summary>
        /// 将API数据转换为实体对象
        /// </summary>
        /// <param name="apiItem">API数据项</param>
        /// <param name="sortType">排序类型</param>
        /// <param name="recordDate">记录日期</param>
        /// <returns>热门标签视频实体</returns>
        private HotTagsVideo ConvertToEntity(HotTagItem apiItem, string sortType, DateTime recordDate)
        {
            try
            {
                if (apiItem == null)
                {
                    throw new ArgumentNullException(nameof(apiItem), "API数据项不能为空");
                }

                return new HotTagsVideo
                {
                    HashtagId = apiItem.HashtagId ?? string.Empty,
                    HashtagName = apiItem.HashtagName ?? string.Empty,
                    CountryId = apiItem.CountryInfo?.Id ?? string.Empty,
                    CountryValue = apiItem.CountryInfo?.Value ?? string.Empty,
                    CountryLabel = apiItem.CountryInfo?.Label ?? string.Empty,
                    IndustryId = apiItem.IndustryInfo?.Id,
                    IndustryValue = apiItem.IndustryInfo?.Value ?? string.Empty,
                    IndustryLabel = apiItem.IndustryInfo?.Label ?? string.Empty,
                    IsPromoted = apiItem.IsPromoted,
                    TrendData = SafeJsonSerialize(apiItem.Trend),
                    CreatorsData = SafeJsonSerialize(apiItem.Creators),
                    PublishCnt = apiItem.PublishCnt,
                    VideoViews = apiItem.VideoViews,
                    Rank = apiItem.Rank,
                    RankDiff = apiItem.RankDiff,
                    RankDiffType = apiItem.RankDiffType,
                    SortType = sortType,
                    RecordDate = recordDate,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
            }
            catch (Exception ex)
            {
                Logger.TaskError($"ConvertToEntity-{apiItem?.HashtagId}", ex, null, null);
                throw new InvalidOperationException($"转换实体失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 修正：安全的JSON序列化方法，增强异常处理
        /// </summary>
        private string SafeJsonSerialize(object obj)
        {
            if (obj == null)
            {
                return null;
            }

            try
            {
                var json = JsonConvert.SerializeObject(obj, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    DateFormatHandling = DateFormatHandling.IsoDateFormat,
                    Formatting = Formatting.None
                });
                
                return string.IsNullOrWhiteSpace(json) || json == "null" ? null : json;
            }
            catch (JsonException ex)
            {
                Logger.TaskError("JsonSerialize-JsonException", ex, null, null);
                return $"{{\"error\":\"JSON序列化失败: {ex.Message}\"}}";
            }
            catch (Exception ex)
            {
                Logger.TaskError("JsonSerialize-GeneralException", ex, null, null);
                return $"{{\"error\":\"序列化异常: {ex.Message}\"}}";
            }
        } 
    }
}

/// <summary>
/// TikTok热门标签API参数常量
/// </summary>
public static class TikTokHotTagsApiConstants
{
    /// <summary>
    /// 支持的排序类型
    /// </summary>
    public static class SortTypes
    {
        public const string Popular = "popular";    // 热门
        public const string New = "new";  // 最新 
    }
    
    /// <summary>
    /// 支持的时间周期
    /// </summary>
    public static class Periods
    {
        public const string Day1 = "1";     // 1天
        public const string Day7 = "7";     // 7天
        public const string Day30 = "30";   // 30天
        public const string Day120 = "120"; // 120天
    }
    
    /// <summary>
    /// 支持的国家代码
    /// </summary>
    public static class CountryCodes
    {
        public const string US = "US";      // 美国
        public const string CN = "CN";      // 中国
        public const string JP = "JP";      // 日本
        public const string KR = "KR";      // 韩国
    }
}










