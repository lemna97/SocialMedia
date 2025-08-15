using Highever.SocialMedia.API.Areas.TikTok.DTO;
using Highever.SocialMedia.Application.Context;
using Highever.SocialMedia.Common;
using Highever.SocialMedia.Domain.Entity;
using Highever.SocialMedia.Domain.Repository;
using LinqKit;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;

namespace Highever.SocialMedia.API.Areas.TikTok.Controllers
{
    /// <summary>
    /// 视频相关
    /// </summary>
    [Area("Videos")]
    [Route("api/videos")]
    [ApiController]
    [ApiExplorerSettings(GroupName = nameof(SwaggerApiGroup.系统功能))]
    public class VideosController : ControllerBase
    {
        INLogger _logger => _serviceProvider.GetRequiredService<INLogger>();
        private IRepository<TiktokVideos> _repositoryTiktokVideos => _serviceProvider.GetRequiredService<IRepository<TiktokVideos>>();
        private IRepository<TiktokVideosDaily> _repositoryTiktokVideosDaily => _serviceProvider.GetRequiredService<IRepository<TiktokVideosDaily>>();
        private IDataPermissionContextService _dataPermissionContextService => _serviceProvider.GetRequiredService<IDataPermissionContextService>();

        /// <summary>
        /// 服务提供程序
        /// </summary>
        public readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="serviceProvider"></param>
        public VideosController(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// 查询TikTok视频信息（分页+关键词搜索）
        /// </summary>
        /// <param name="request">查询请求参数</param>
        /// <returns>分页视频信息</returns>
        [HttpPost("getTiktokVideos")]
        [ProducesResponseType(typeof(PageResult<TiktokVideosResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTiktokVideos([FromBody] TiktokVideosQueryRequest request)
        {
            if (request.PageIndex == null || request.PageIndex <= 0)
            {
                request.PageIndex = 1;
            }
            if (request.PageSize == null || request.PageSize <= 0)
            {
                request.PageSize = 20;
            }
            try
            {
                // 获取当前用户的数据权限上下文
                var permissionContext = _dataPermissionContextService.GetCurrentContext();
                if (permissionContext == null)
                {
                    return this.Fail("无法获取用户权限信息");
                }

                // 非管理员且没有权限，直接返回空结果
                if (!permissionContext.IsAdmin && !permissionContext.AccountConfigUniqueIds.Any())
                {
                    return this.PagedResult(new PageResult<TiktokVideosResponse>
                    {
                        Items = new List<TiktokVideosResponse>(),
                        totalCount = 0,
                        PageIndex = request.PageIndex.Value,
                        PageSize = request.PageSize.Value
                    }, "查询成功");
                }

                // 构建查询条件
                Expression<Func<TiktokVideos, bool>>? predicate = null;

                // 数据权限过滤 + 关键词搜索
                if (!permissionContext.IsAdmin)
                {
                    // 非管理员：权限过滤 + 关键词搜索
                    if (!string.IsNullOrWhiteSpace(request.Keyword))
                    {
                        var keyword = request.Keyword.Trim();
                        predicate = x => permissionContext.AccountConfigUniqueIds.Contains(x.UniqueId) &&
                                       (x.UniqueId.Contains(keyword) ||
                                        (x.Nickname != null && x.Nickname.Contains(keyword)) ||
                                        (x.Desc != null && x.Desc.Contains(keyword)));
                    }
                    else
                    {
                        predicate = x => permissionContext.AccountConfigUniqueIds.Contains(x.UniqueId);
                    }
                }
                else
                {
                    // 管理员：只有关键词搜索
                    if (!string.IsNullOrWhiteSpace(request.Keyword))
                    {
                        var keyword = request.Keyword.Trim();
                        predicate = x => x.UniqueId.Contains(keyword) ||
                                       (x.Nickname != null && x.Nickname.Contains(keyword)) ||
                                       (x.Desc != null && x.Desc.Contains(keyword));
                    }
                }

                // 视频创建时间范围过滤
                if (request.CreateTimeStart.HasValue && request.CreateTimeStart.Value > 0)
                {
                    if (predicate == null)
                        predicate = x => x.CreateTime >= request.CreateTimeStart.Value;
                    else
                        predicate = predicate.And(x => x.CreateTime >= request.CreateTimeStart.Value);
                }

                if (request.CreateTimeEnd.HasValue && request.CreateTimeEnd.Value > 0)
                {
                    if (predicate == null)
                        predicate = x => x.CreateTime <= request.CreateTimeEnd.Value;
                    else
                        predicate = predicate.And(x => x.CreateTime <= request.CreateTimeEnd.Value);
                }

                // 记录创建时间范围过滤
                if (request.CreatedStartDate.HasValue)
                {
                    var createdStartDate = request.CreatedStartDate.Value.Date;
                    if (predicate == null)
                        predicate = x => x.CreatedAt >= createdStartDate;
                    else
                        predicate = predicate.And(x => x.CreatedAt >= createdStartDate);
                }

                if (request.CreatedEndDate.HasValue)
                {
                    var createdEndDate = request.CreatedEndDate.Value.Date.AddDays(1);
                    if (predicate == null)
                        predicate = x => x.CreatedAt < createdEndDate;
                    else
                        predicate = predicate.And(x => x.CreatedAt < createdEndDate);
                }

                // 构建排序表达式
                Expression<Func<TiktokVideos, object>>? orderBy = null;
                if (!string.IsNullOrWhiteSpace(request.OrderBy))
                {
                    orderBy = request.OrderBy.ToLower() switch
                    {
                        "awemeid" => x => x.Id,
                        "uniqueid" => x => x.UniqueId,
                        "nickname" => x => x.Nickname ?? "",
                        "createtime" => x => x.CreateTime ?? 0,
                        "duration" => x => x.Duration ?? 0,
                        "playcount" => x => x.PlayCount,
                        "diggcount" => x => x.DiggCount,
                        "commentcount" => x => x.CommentCount,
                        "sharecount" => x => x.ShareCount,
                        "collectcount" => x => x.CollectCount,
                        "createdat" => x => x.CreatedAt,
                        "updatedat" => x => x.UpdatedAt,
                        _ => x => x.UpdatedAt
                    };
                }

                // 执行分页查询
                var result = await _repositoryTiktokVideos.GetPagedListAsync(
                    predicate: predicate,
                    pageIndex: request.PageIndex.Value,
                    pageSize: request.PageSize.Value,
                    orderBy: orderBy,
                    ascending: request.Ascending
                );

                // 转换为响应DTO
                var responseItems = result.Items.Select(x => new TiktokVideosResponse
                {
                    AwemeId = x.Id,
                    UniqueId = x.UniqueId,
                    Nickname = x.Nickname,
                    Desc = x.Desc,
                    CreateTime = x.CreateTime,
                    Duration = x.Duration / 1000,
                    CoverUrl = x.CoverUrl,
                    PlayUrl = x.PlayUrl,
                    PlayCount = x.PlayCount,
                    DiggCount = x.DiggCount,
                    CommentCount = x.CommentCount,
                    ShareCount = x.ShareCount,
                    CollectCount = x.CollectCount,
                    IsTop = x.IsTop,
                    IsAds = x.IsAds,
                    CreatedByAi = x.CreatedByAi,
                    ShareUrl = x.ShareUrl,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt
                }).ToList();

                var pageResult = new PageResult<TiktokVideosResponse>
                {
                    Items = responseItems,
                    totalCount = result.TotalCount,
                    PageIndex = result.PageIndex,
                    PageSize = result.PageSize
                };

                return this.PagedResult(pageResult, "查询成功");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"查询TikTok视频信息失败: {ex.Message}");
                return this.Fail("查询视频信息失败");
            }
        }

        /// <summary>
        /// 查询TikTok视频每日更新记录（分页）
        /// </summary>
        /// <param name="request">查询请求参数</param>
        /// <returns>分页每日记录信息</returns>
        [HttpPost("getTiktokVideosDaily")]
        [ProducesResponseType(typeof(PageResult<TiktokVideosDailyResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTiktokVideosDaily([FromBody] TiktokVideosDailyQueryRequest request)
        {
            if (request.PageIndex == null || request.PageIndex <= 0)
            {
                request.PageIndex = 1;
            }
            if (request.PageSize == null || request.PageSize <= 0)
            {
                request.PageSize = 20;
            }
            try
            {
                // 获取当前用户的数据权限上下文
                var permissionContext = _dataPermissionContextService.GetCurrentContext();
                if (permissionContext == null)
                {
                    return this.Fail("无法获取用户权限信息");
                }

                // 非管理员且没有权限，直接返回空结果
                if (!permissionContext.IsAdmin && !permissionContext.AccountConfigUniqueIds.Any())
                {
                    return this.PagedResult(new PageResult<TiktokVideosDailyResponse>
                    {
                        Items = new List<TiktokVideosDailyResponse>(),
                        totalCount = 0,
                        PageIndex = request.PageIndex.Value,
                        PageSize = request.PageSize.Value
                    }, "查询成功");
                }

                // 构建查询条件
                Expression<Func<TiktokVideosDaily, bool>> predicate = x => true;

                // 1. 数据权限过滤
                if (!permissionContext.IsAdmin)
                {
                    predicate = x => permissionContext.AccountConfigUniqueIds.Contains(x.UniqueId);
                }

                // 2. 关键词搜索
                if (!string.IsNullOrWhiteSpace(request.Keyword))
                {
                    var keyword = request.Keyword.Trim();
                    predicate = predicate.And(x => x.UniqueId.Contains(keyword) ||
                                                 (x.Nickname != null && x.Nickname.Contains(keyword)) ||
                                                 (x.Desc != null && x.Desc.Contains(keyword)));
                }

                // 3. 记录日期范围过滤
                if (request.StartDate.HasValue)
                {
                    var startDate = request.StartDate.Value.Date;
                    predicate = predicate.And(x => x.RecordDate >= startDate);
                }

                if (request.EndDate.HasValue)
                {
                    var endDate = request.EndDate.Value.Date.AddDays(1);
                    predicate = predicate.And(x => x.RecordDate < endDate);
                }

                // 4. 创建时间范围过滤
                if (request.CreatedStartDate.HasValue)
                {
                    var createdStartDate = request.CreatedStartDate.Value.Date;
                    predicate = predicate.And(x => x.CreatedAt >= createdStartDate);
                }

                if (request.CreatedEndDate.HasValue)
                {
                    var createdEndDate = request.CreatedEndDate.Value.Date.AddDays(1);
                    predicate = predicate.And(x => x.CreatedAt < createdEndDate);
                }

                // 构建排序表达式
                Expression<Func<TiktokVideosDaily, object>>? orderBy = null;
                if (!string.IsNullOrWhiteSpace(request.OrderBy))
                {
                    orderBy = request.OrderBy.ToLower() switch
                    {
                        "id" => x => x.Id,
                        "awemeid" => x => x.Id,
                        "uniqueid" => x => x.UniqueId,
                        "nickname" => x => x.Nickname ?? "",
                        "playcount" => x => x.PlayCount,
                        "diggcount" => x => x.DiggCount,
                        "commentcount" => x => x.CommentCount,
                        "sharecount" => x => x.ShareCount,
                        "collectcount" => x => x.CollectCount,
                        "forwardcount" => x => x.ForwardCount,
                        "recorddate" => x => x.RecordDate,
                        "createdat" => x => x.CreatedAt,
                        "updatedat" => x => x.UpdatedAt,
                        _ => x => x.RecordDate
                    };
                }

                // 执行分页查询
                var result = await _repositoryTiktokVideosDaily.GetPagedListAsync(
                    predicate: predicate,
                    pageIndex: request.PageIndex.Value,
                    pageSize: request.PageSize.Value,
                    orderBy: orderBy,
                    ascending: request.Ascending
                );

                // 转换为响应DTO
                var responseItems = result.Items.Select(x => new TiktokVideosDailyResponse
                {
                    Id = x.Id,
                    AwemeId = x.Id,
                    UniqueId = x.UniqueId,
                    Nickname = x.Nickname,
                    Desc = x.Desc,
                    PlayCount = x.PlayCount,
                    DiggCount = x.DiggCount,
                    CommentCount = x.CommentCount,
                    ShareCount = x.ShareCount,
                    CollectCount = x.CollectCount,
                    ForwardCount = x.ForwardCount,
                    IsTop = x.IsTop,
                    IsAds = x.IsAds,
                    RecordDate = x.RecordDate,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt
                }).ToList();

                var pageResult = new PageResult<TiktokVideosDailyResponse>
                {
                    Items = responseItems,
                    totalCount = result.TotalCount,
                    PageIndex = result.PageIndex,
                    PageSize = result.PageSize
                };

                return this.PagedResult(pageResult, "查询成功");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"查询TikTok视频每日记录失败: {ex.Message}");
                return this.Fail("查询每日记录失败");
            }
        }

        /// <summary>
        /// 获取TikTok视频统计信息
        /// </summary>
        /// <returns>统计信息</returns>
        [HttpGet("statistics")]
        [ProducesResponseType(typeof(AjaxResult<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetVideosStatistics()
        {
            try
            {
                // 获取当前用户的数据权限上下文
                var permissionContext = _dataPermissionContextService.GetCurrentContext();
                if (permissionContext == null)
                {
                    return this.Fail("无法获取用户权限信息");
                }

                Expression<Func<TiktokVideos, bool>>? predicate = null;
                Expression<Func<TiktokVideosDaily, bool>>? dailyPredicate = null;

                // 数据权限过滤
                if (!permissionContext.IsAdmin)
                {
                    predicate = x => permissionContext.AccountConfigUniqueIds.Contains(x.UniqueId);
                    dailyPredicate = x => permissionContext.AccountConfigUniqueIds.Contains(x.UniqueId);
                }

                var totalVideos = await _repositoryTiktokVideos.CountAsync(predicate);
                var totalDailyRecords = await _repositoryTiktokVideosDaily.CountAsync(dailyPredicate);
                var todayRecords = await _repositoryTiktokVideosDaily.CountAsync(
                    dailyPredicate?.And(x => x.RecordDate == DateTime.Today) ??
                    (x => x.RecordDate == DateTime.Today));

                return this.Success(new
                {
                    totalVideos,
                    totalDailyRecords,
                    todayRecords,
                    lastUpdateTime = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"获取视频统计信息失败: {ex.Message}");
                return this.Fail("获取统计信息失败");
            }
        }

        /// <summary>
        /// 获取视频增量统计数据
        /// </summary>
        /// <param name="request">统计请求参数</param>
        /// <returns>视频增量统计数据</returns>
        [HttpPost("getVideoStatistics")]
        [ProducesResponseType(typeof(PageResult<VideoStatisticsResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetVideoStatistics([FromBody] VideoStatisticsRequest request)
        {
            try
            {
                // 获取当前用户的数据权限上下文
                var permissionContext = _dataPermissionContextService.GetCurrentContext();
                if (permissionContext == null)
                {
                    return this.Fail("无法获取用户权限信息");
                }

                // 非管理员且没有权限，直接返回空结果
                if (!permissionContext.IsAdmin && !permissionContext.AccountConfigUniqueIds.Any())
                {
                    return this.PagedResult(new PageResult<VideoStatisticsResponse>
                    {
                        Items = new List<VideoStatisticsResponse>(),
                        totalCount = 0,
                        PageIndex = request.PageIndex,
                        PageSize = request.PageSize
                    }, "查询成功");
                }

                // 设置默认时间范围
                var startDate = request.StartDate ?? DateTime.Today.AddDays(-30);
                var endDate = request.EndDate ?? DateTime.Today;

                // 构建查询条件
                Expression<Func<TiktokVideosDaily, bool>> predicate = x =>
                    x.RecordDate >= startDate && x.RecordDate <= endDate;

                // 数据权限过滤
                if (!permissionContext.IsAdmin)
                {
                    predicate = predicate.And(x => permissionContext.AccountConfigUniqueIds.Contains(x.UniqueId));
                }

                // 特定视频过滤
                if (!string.IsNullOrWhiteSpace(request.VideoId))
                {
                    predicate = predicate.And(x => x.Id == request.VideoId);
                }

                // 特定账号过滤
                if (!string.IsNullOrWhiteSpace(request.UniqueId))
                {
                    predicate = predicate.And(x => x.UniqueId == request.UniqueId);
                }

                // 获取期间内的所有数据
                var dailyData = await _repositoryTiktokVideosDaily.QueryListAsync(predicate);

                if (!dailyData.Any())
                {
                    return this.PagedResult(new PageResult<VideoStatisticsResponse>
                    {
                        Items = new List<VideoStatisticsResponse>(),
                        totalCount = 0,
                        PageIndex = request.PageIndex,
                        PageSize = request.PageSize
                    }, "暂无数据");
                }

                // 按视频ID分组统计
                var videoGroups = dailyData.GroupBy(x => x.Id);
                var allResults = new List<VideoStatisticsResponse>();

                foreach (var group in videoGroups)
                {
                    var videoData = group.OrderBy(x => x.RecordDate).ToList();
                    var latestData = videoData.LastOrDefault();
                    var previousData = videoData.Count > 1 ? videoData[videoData.Count - 2] : null;

                    if (latestData == null) continue;

                    var statistics = new VideoStatisticsResponse
                    {
                        VideoId = group.Key,
                        UniqueId = latestData.UniqueId,
                        Nickname = latestData.Nickname,
                        Desc = latestData.Desc,
                        CreateTime = latestData.CreateTime,
                        DateRange = $"{startDate:yyyy-MM-dd} 至 {endDate:yyyy-MM-dd}",
                        PlayGrowth = CalculateVideoGrowthStatistics(
                            videoData.Select(x => x.PlayCount).ToList(),
                            latestData.PlayCount,
                            previousData?.PlayCount ?? 0
                        ),
                        DiggGrowth = CalculateVideoGrowthStatistics(
                            videoData.Select(x => x.DiggCount).ToList(),
                            latestData.DiggCount,
                            previousData?.DiggCount ?? 0
                        ),
                        CommentGrowth = CalculateVideoGrowthStatistics(
                            videoData.Select(x => x.CommentCount).ToList(),
                            latestData.CommentCount,
                            previousData?.CommentCount ?? 0
                        ),
                        ShareGrowth = CalculateVideoGrowthStatistics(
                            videoData.Select(x => x.ShareCount).ToList(),
                            latestData.ShareCount,
                            previousData?.ShareCount ?? 0
                        ),
                        CollectGrowth = CalculateVideoGrowthStatistics(
                            videoData.Select(x => x.CollectCount).ToList(),
                            latestData.CollectCount,
                            previousData?.CollectCount ?? 0
                        ),
                        ForwardGrowth = CalculateVideoGrowthStatistics(
                            videoData.Select(x => x.ForwardCount).ToList(),
                            latestData.ForwardCount,
                            previousData?.ForwardCount ?? 0
                        ),
                        DownloadGrowth = CalculateVideoGrowthStatistics(
                            videoData.Select(x => x.DownloadCount).ToList(),
                            latestData.DownloadCount,
                            previousData?.DownloadCount ?? 0
                        ),
                        LikeRate = CalculateVideoLikeRate(videoData, latestData, previousData),
                        EngagementRate = CalculateVideoEngagementRate(videoData, latestData, previousData),
                        CommentRate = CalculateVideoCommentRate(videoData, latestData, previousData),
                        ShareRate = CalculateVideoShareRate(videoData, latestData, previousData),
                        CollectRate = CalculateVideoCollectRate(videoData, latestData, previousData),
                        FollowerConversionRate = null, // 预留字段，暂时为空
                        DailyTrends = videoData.Select(x => new DailyTrend
                        {
                            Date = x.RecordDate,
                            PlayCount = x.PlayCount,
                            DiggCount = x.DiggCount,
                            CommentCount = x.CommentCount,
                            ShareCount = x.ShareCount,
                            LikeRate = x.PlayCount > 0 ? Math.Round((decimal)x.DiggCount / x.PlayCount * 100, 2) : 0,
                            EngagementRate = x.PlayCount > 0 ? Math.Round((decimal)(x.DiggCount + x.CommentCount + x.ShareCount) / x.PlayCount * 100, 2) : 0
                        }).ToList()
                    };

                    allResults.Add(statistics);
                }

                // 分页处理
                var totalCount = allResults.Count;
                var pagedResults = allResults
                    .Skip((request.PageIndex - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToList();

                var pageResult = new PageResult<VideoStatisticsResponse>
                {
                    Items = pagedResults,
                    totalCount = totalCount,
                    PageIndex = request.PageIndex,
                    PageSize = request.PageSize
                };

                return this.PagedResult(pageResult, "查询成功");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"获取视频统计数据失败: {ex.Message}");
                return this.Fail("获取统计数据失败");
            }
        }

        /// <summary>
        /// 计算视频增长统计数据
        /// </summary>
        /// <param name="values">所有值列表</param>
        /// <param name="currentValue">当前值</param>
        /// <param name="previousValue">前一个值</param>
        /// <returns>增长统计</returns>
        [HiddenAPI]
        private GrowthStatistics CalculateVideoGrowthStatistics(List<int> values, int currentValue, int previousValue)
        {
            var growth = currentValue - previousValue;
            var growthRate = previousValue > 0 ? (decimal)growth / previousValue * 100 : 0;

            return new GrowthStatistics
            {
                CurrentValue = currentValue,
                PreviousValue = previousValue,
                Growth = growth,
                GrowthRate = Math.Round(growthRate, 2),
                MaxValue = values.Any() ? values.Max() : currentValue,
                MinValue = values.Any() ? values.Min() : currentValue,
                AvgValue = values.Any() ? Math.Round((decimal)values.Average(), 2) : currentValue
            };
        }

        /// <summary>
        /// 计算视频点赞率统计
        /// </summary>
        /// <param name="videoData">视频历史数据</param>
        /// <param name="latestData">最新数据</param>
        /// <param name="previousData">前一天数据</param>
        /// <returns>点赞率统计</returns>
        [HiddenAPI]
        private RateStatistics CalculateVideoLikeRate(List<TiktokVideosDaily> videoData, TiktokVideosDaily latestData, TiktokVideosDaily? previousData)
        {
            // 计算当前点赞率（点赞数/播放数 * 100）
            var currentRate = latestData.PlayCount > 0 ?
                Math.Round((decimal)latestData.DiggCount / latestData.PlayCount * 100, 2) : 0;

            // 计算前一天点赞率
            var previousRate = previousData != null && previousData.PlayCount > 0 ?
                Math.Round((decimal)previousData.DiggCount / previousData.PlayCount * 100, 2) : 0;

            // 计算所有历史数据的点赞率
            var allRates = videoData.Where(x => x.PlayCount > 0)
                .Select(x => Math.Round((decimal)x.DiggCount / x.PlayCount * 100, 2))
                .ToList();

            return new RateStatistics
            {
                CurrentRate = currentRate,
                PreviousRate = previousRate,
                RateChange = Math.Round(currentRate - previousRate, 2),
                MaxRate = allRates.Any() ? allRates.Max() : currentRate,
                MinRate = allRates.Any() ? allRates.Min() : currentRate,
                AvgRate = allRates.Any() ? Math.Round(allRates.Average(), 2) : currentRate,
                Description = "点赞数/播放数 * 100%"
            };
        }

        /// <summary>
        /// 计算视频互动率统计
        /// </summary>
        /// <param name="videoData">视频历史数据</param>
        /// <param name="latestData">最新数据</param>
        /// <param name="previousData">前一天数据</param>
        /// <returns>互动率统计</returns>
        [HiddenAPI]
        private RateStatistics CalculateVideoEngagementRate(List<TiktokVideosDaily> videoData, TiktokVideosDaily latestData, TiktokVideosDaily? previousData)
        {
            // 计算当前互动率（(点赞+评论+分享)/播放数 * 100）
            var currentEngagement = latestData.DiggCount + latestData.CommentCount + latestData.ShareCount;
            var currentRate = latestData.PlayCount > 0 ?
                Math.Round((decimal)currentEngagement / latestData.PlayCount * 100, 2) : 0;

            // 计算前一天互动率
            var previousEngagement = previousData != null ?
                previousData.DiggCount + previousData.CommentCount + previousData.ShareCount : 0;
            var previousRate = previousData != null && previousData.PlayCount > 0 ?
                Math.Round((decimal)previousEngagement / previousData.PlayCount * 100, 2) : 0;

            // 计算所有历史数据的互动率
            var allRates = videoData.Where(x => x.PlayCount > 0)
                .Select(x =>
                {
                    var engagement = x.DiggCount + x.CommentCount + x.ShareCount;
                    return Math.Round((decimal)engagement / x.PlayCount * 100, 2);
                })
                .ToList();

            return new RateStatistics
            {
                CurrentRate = currentRate,
                PreviousRate = previousRate,
                RateChange = Math.Round(currentRate - previousRate, 2),
                MaxRate = allRates.Any() ? allRates.Max() : currentRate,
                MinRate = allRates.Any() ? allRates.Min() : currentRate,
                AvgRate = allRates.Any() ? Math.Round(allRates.Average(), 2) : currentRate,
                Description = "(点赞数+评论数+分享数)/播放数 * 100%"
            };
        }

        /// <summary>
        /// 计算视频评论率统计
        /// </summary>
        [HiddenAPI]
        private RateStatistics CalculateVideoCommentRate(List<TiktokVideosDaily> videoData, TiktokVideosDaily latestData, TiktokVideosDaily? previousData)
        {
            var currentRate = latestData.PlayCount > 0 ?
                Math.Round((decimal)latestData.CommentCount / latestData.PlayCount * 100, 2) : 0;

            var previousRate = previousData != null && previousData.PlayCount > 0 ?
                Math.Round((decimal)previousData.CommentCount / previousData.PlayCount * 100, 2) : 0;

            var allRates = videoData.Where(x => x.PlayCount > 0)
                .Select(x => Math.Round((decimal)x.CommentCount / x.PlayCount * 100, 2))
                .ToList();

            return new RateStatistics
            {
                CurrentRate = currentRate,
                PreviousRate = previousRate,
                RateChange = Math.Round(currentRate - previousRate, 2),
                MaxRate = allRates.Any() ? allRates.Max() : currentRate,
                MinRate = allRates.Any() ? allRates.Min() : currentRate,
                AvgRate = allRates.Any() ? Math.Round(allRates.Average(), 2) : currentRate,
                Description = "评论数/播放数 * 100%"
            };
        }

        /// <summary>
        /// 计算视频分享率统计
        /// </summary>
        [HiddenAPI]
        private RateStatistics CalculateVideoShareRate(List<TiktokVideosDaily> videoData, TiktokVideosDaily latestData, TiktokVideosDaily? previousData)
        {
            var currentRate = latestData.PlayCount > 0 ?
                Math.Round((decimal)latestData.ShareCount / latestData.PlayCount * 100, 2) : 0;

            var previousRate = previousData != null && previousData.PlayCount > 0 ?
                Math.Round((decimal)previousData.ShareCount / previousData.PlayCount * 100, 2) : 0;

            var allRates = videoData.Where(x => x.PlayCount > 0)
                .Select(x => Math.Round((decimal)x.ShareCount / x.PlayCount * 100, 2))
                .ToList();

            return new RateStatistics
            {
                CurrentRate = currentRate,
                PreviousRate = previousRate,
                RateChange = Math.Round(currentRate - previousRate, 2),
                MaxRate = allRates.Any() ? allRates.Max() : currentRate,
                MinRate = allRates.Any() ? allRates.Min() : currentRate,
                AvgRate = allRates.Any() ? Math.Round(allRates.Average(), 2) : currentRate,
                Description = "分享数/播放数 * 100%"
            };
        }

        /// <summary>
        /// 计算视频收藏率统计
        /// </summary>
        [HiddenAPI]
        private RateStatistics CalculateVideoCollectRate(List<TiktokVideosDaily> videoData, TiktokVideosDaily latestData, TiktokVideosDaily? previousData)
        {
            var currentRate = latestData.PlayCount > 0 ?
                Math.Round((decimal)latestData.CollectCount / latestData.PlayCount * 100, 2) : 0;

            var previousRate = previousData != null && previousData.PlayCount > 0 ?
                Math.Round((decimal)previousData.CollectCount / previousData.PlayCount * 100, 2) : 0;

            var allRates = videoData.Where(x => x.PlayCount > 0)
                .Select(x => Math.Round((decimal)x.CollectCount / x.PlayCount * 100, 2))
                .ToList();

            return new RateStatistics
            {
                CurrentRate = currentRate,
                PreviousRate = previousRate,
                RateChange = Math.Round(currentRate - previousRate, 2),
                MaxRate = allRates.Any() ? allRates.Max() : currentRate,
                MinRate = allRates.Any() ? allRates.Min() : currentRate,
                AvgRate = allRates.Any() ? Math.Round(allRates.Average(), 2) : currentRate,
                Description = "收藏数/播放数 * 100%"
            };
        }

        /// <summary>
        /// 计算账号聚合增长统计
        /// </summary>
        [HiddenAPI]
        private GrowthStatistics CalculateAccountAggregateGrowth(
            List<TiktokVideosDaily> accountData,
            Func<TiktokVideosDaily, int> valueSelector,
            DateTime startDate,
            DateTime endDate)
        {
            // 按日期分组，计算每日总和
            var dailyTotals = accountData
                .GroupBy(x => x.RecordDate)
                .ToDictionary(g => g.Key, g => g.Sum(valueSelector));

            // 获取期间内的总值
            var currentTotal = dailyTotals.Values.Sum();

            // 计算前一个周期的总值（用于计算增长率）
            var periodDays = (endDate - startDate).Days + 1;
            var previousStartDate = startDate.AddDays(-periodDays);
            var previousEndDate = startDate.AddDays(-1);

            // 获取前一周期数据
            var previousData = accountData
                .Where(x => x.RecordDate >= previousStartDate && x.RecordDate <= previousEndDate)
                .Sum(valueSelector);

            var growth = currentTotal - previousData;
            var growthRate = previousData > 0 ? (decimal)growth / previousData * 100 : 0;

            return new GrowthStatistics
            {
                CurrentValue = currentTotal,
                PreviousValue = previousData,
                Growth = growth,
                GrowthRate = Math.Round(growthRate, 2),
                MaxValue = dailyTotals.Values.Any() ? dailyTotals.Values.Max() : currentTotal,
                MinValue = dailyTotals.Values.Any() ? dailyTotals.Values.Min() : currentTotal,
                AvgValue = dailyTotals.Values.Any() ? Math.Round((decimal)dailyTotals.Values.Average(), 2) : currentTotal
            };
        }

        /// <summary>
        /// 计算账号整体互动率
        /// </summary>
        [HiddenAPI]
        private RateStatistics CalculateAccountOverallEngagementRate(List<TiktokVideosDaily> accountData)
        {
            var totalPlay = accountData.Sum(x => x.PlayCount);
            var totalEngagement = accountData.Sum(x => x.DiggCount + x.CommentCount + x.ShareCount);

            var currentRate = totalPlay > 0 ?
                Math.Round((decimal)totalEngagement / totalPlay * 100, 2) : 0;

            // 计算每日互动率的平均值
            var dailyRates = accountData
                .Where(x => x.PlayCount > 0)
                .Select(x => Math.Round((decimal)(x.DiggCount + x.CommentCount + x.ShareCount) / x.PlayCount * 100, 2))
                .ToList();

            return new RateStatistics
            {
                CurrentRate = currentRate,
                PreviousRate = 0, // 可以根据需要计算前一周期的比率
                RateChange = 0,
                MaxRate = dailyRates.Any() ? dailyRates.Max() : currentRate,
                MinRate = dailyRates.Any() ? dailyRates.Min() : currentRate,
                AvgRate = dailyRates.Any() ? Math.Round(dailyRates.Average(), 2) : currentRate,
                Description = "账号整体互动率：(总点赞+总评论+总分享)/总播放 * 100%"
            };
        }

        /// <summary>
        /// 计算账号整体点赞率
        /// </summary>
        private RateStatistics CalculateAccountOverallLikeRate(List<TiktokVideosDaily> accountData)
        {
            var totalPlay = accountData.Sum(x => x.PlayCount);
            var totalDigg = accountData.Sum(x => x.DiggCount);

            var currentRate = totalPlay > 0 ?
                Math.Round((decimal)totalDigg / totalPlay * 100, 2) : 0;

            var dailyRates = accountData
                .Where(x => x.PlayCount > 0)
                .Select(x => Math.Round((decimal)x.DiggCount / x.PlayCount * 100, 2))
                .ToList();

            return new RateStatistics
            {
                CurrentRate = currentRate,
                PreviousRate = 0,
                RateChange = 0,
                MaxRate = dailyRates.Any() ? dailyRates.Max() : currentRate,
                MinRate = dailyRates.Any() ? dailyRates.Min() : currentRate,
                AvgRate = dailyRates.Any() ? Math.Round(dailyRates.Average(), 2) : currentRate,
                Description = "账号整体点赞率：总点赞/总播放 * 100%"
            };
        }

        /// <summary>
        /// 计算视频表现分布
        /// </summary>
        [HiddenAPI]
        private VideoPerformanceDistribution CalculateVideoPerformanceDistribution(List<TiktokVideosDaily> accountData)
        {
            var videoGroups = accountData.GroupBy(x => x.Id);
            var videoPerformances = new List<decimal>();

            foreach (var video in videoGroups)
            {
                var latestData = video.OrderByDescending(x => x.RecordDate).FirstOrDefault();
                if (latestData != null && latestData.PlayCount > 0)
                {
                    var engagementRate = (decimal)(latestData.DiggCount + latestData.CommentCount + latestData.ShareCount) / latestData.PlayCount * 100;
                    videoPerformances.Add(engagementRate);
                }
            }

            return new VideoPerformanceDistribution
            {
                TotalVideos = videoGroups.Count(),
                HighPerformanceVideos = videoPerformances.Count(x => x >= 5), // 互动率>=5%
                MediumPerformanceVideos = videoPerformances.Count(x => x >= 2 && x < 5), // 2%<=互动率<5%
                LowPerformanceVideos = videoPerformances.Count(x => x < 2), // 互动率<2%
                AverageEngagementRate = videoPerformances.Any() ? Math.Round(videoPerformances.Average(), 2) : 0
            };
        }

        /// <summary>
        /// 获取账号维度的视频统计数据
        /// </summary>
        /// <param name="request">统计请求参数</param>
        /// <returns>账号维度的视频统计数据</returns>
        [HttpPost("getAccountVideoStatistics")]
        [ProducesResponseType(typeof(List<AccountVideoStatisticsResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAccountVideoStatistics([FromBody] AccountVideoStatisticsRequest request)
        {
            try
            {
                // 获取当前用户的数据权限上下文
                var permissionContext = _dataPermissionContextService.GetCurrentContext();
                if (permissionContext == null)
                {
                    return this.Fail("无法获取用户权限信息");
                }

                // 非管理员且没有权限，直接返回空结果
                if (!permissionContext.IsAdmin && !permissionContext.AccountConfigUniqueIds.Any())
                {
                    return this.Success(new List<AccountVideoStatisticsResponse>(), "查询成功");
                }

                // 设置默认时间范围
                var startDate = request.StartDate ?? DateTime.Today.AddDays(-30);
                var endDate = request.EndDate ?? DateTime.Today;

                // 构建查询条件
                Expression<Func<TiktokVideosDaily, bool>> predicate = x =>
                    x.RecordDate >= startDate && x.RecordDate <= endDate;

                // 数据权限过滤
                if (!permissionContext.IsAdmin)
                {
                    predicate = predicate.And(x => permissionContext.AccountConfigUniqueIds.Contains(x.UniqueId));
                }

                // 特定账号过滤
                if (!string.IsNullOrWhiteSpace(request.UniqueId))
                {
                    predicate = predicate.And(x => x.UniqueId == request.UniqueId);
                }

                // 获取期间内的所有数据
                var dailyData = await _repositoryTiktokVideosDaily.QueryListAsync(predicate);

                if (!dailyData.Any())
                {
                    return this.Success(new List<AccountVideoStatisticsResponse>(), "暂无数据");
                }

                // 按账号分组统计
                var accountGroups = dailyData.GroupBy(x => x.UniqueId);
                var results = new List<AccountVideoStatisticsResponse>();

                foreach (var group in accountGroups)
                {
                    var accountData = group.ToList();
                    var latestData = accountData.OrderByDescending(x => x.RecordDate).FirstOrDefault();

                    if (latestData == null) continue;

                    // 构建增量统计项列表
                    var growthStatistics = new List<StatisticsItem>
                    {
                        new StatisticsItem
                        {
                            Type = StatisticsType.Play,
                            TypeName = "播放数",
                            Growth = CalculateAccountAggregateGrowth(accountData, x => x.PlayCount, startDate, endDate)
                        },
                        new StatisticsItem
                        {
                            Type = StatisticsType.Digg,
                            TypeName = "点赞数",
                            Growth = CalculateAccountAggregateGrowth(accountData, x => x.DiggCount, startDate, endDate)
                        },
                        new StatisticsItem
                        {
                            Type = StatisticsType.Comment,
                            TypeName = "评论数",
                            Growth = CalculateAccountAggregateGrowth(accountData, x => x.CommentCount, startDate, endDate)
                        },
                        new StatisticsItem
                        {
                            Type = StatisticsType.Share,
                            TypeName = "分享数",
                            Growth = CalculateAccountAggregateGrowth(accountData, x => x.ShareCount, startDate, endDate)
                        },
                        new StatisticsItem
                        {
                            Type = StatisticsType.Collect,
                            TypeName = "收藏数",
                            Growth = CalculateAccountAggregateGrowth(accountData, x => x.CollectCount, startDate, endDate)
                        }
                    };

                    // 构建比率统计项列表
                    var rateStatistics = new List<RateStatisticsItem>
                    {
                        new RateStatisticsItem
                        {
                            Type = RateType.EngagementRate,
                            TypeName = "整体互动率",
                            Rate = CalculateAccountOverallEngagementRate(accountData)
                        },
                        new RateStatisticsItem
                        {
                            Type = RateType.LikeRate,
                            TypeName = "整体点赞率",
                            Rate = CalculateAccountOverallLikeRate(accountData)
                        }
                    };

                    var statistics = new AccountVideoStatisticsResponse
                    {
                        UniqueId = group.Key,
                        Nickname = latestData.Nickname,
                        DateRange = $"{startDate:yyyy-MM-dd} 至 {endDate:yyyy-MM-dd}",
                        VideoCount = accountData.Select(x => x.Id).Distinct().Count(),

                        // 新的集合结构
                        GrowthStatistics = growthStatistics,
                        RateStatistics = rateStatistics,

                        // 为了向后兼容，保留原有字段
                        TotalPlayGrowth = growthStatistics.First(x => x.Type == StatisticsType.Play).Growth,
                        TotalDiggGrowth = growthStatistics.First(x => x.Type == StatisticsType.Digg).Growth,
                        TotalCommentGrowth = growthStatistics.First(x => x.Type == StatisticsType.Comment).Growth,
                        TotalShareGrowth = growthStatistics.First(x => x.Type == StatisticsType.Share).Growth,
                        TotalCollectGrowth = growthStatistics.First(x => x.Type == StatisticsType.Collect).Growth,
                        OverallEngagementRate = rateStatistics.First(x => x.Type == RateType.EngagementRate).Rate,
                        OverallLikeRate = rateStatistics.First(x => x.Type == RateType.LikeRate).Rate,

                        VideoPerformanceDistribution = CalculateVideoPerformanceDistribution(accountData)
                    };

                    results.Add(statistics);
                }

                // 按总播放量排序
                results = results.OrderByDescending(x => x.TotalPlayGrowth.CurrentValue).ToList();

                return this.Success(results, "查询成功");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"获取账号视频统计数据失败: {ex.Message}");
                return this.Fail("获取统计数据失败");
            }
        }
    }
}
