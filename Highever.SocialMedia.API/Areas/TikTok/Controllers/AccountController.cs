using Highever.SocialMedia.API.Areas.TikTok.DTO;
using Highever.SocialMedia.Application.Contracts.Context;
using Highever.SocialMedia.Common;
using Highever.SocialMedia.Domain.Entity;
using Highever.SocialMedia.Domain.Repository;
using LinqKit;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;

namespace Highever.SocialMedia.API.Areas.TikTok.Controllers
{
    /// <summary>
    /// 账号相关
    /// </summary>
    [Area("Account")]
    [Route("api/account")]
    [ApiController]
    [ApiExplorerSettings(GroupName = nameof(SwaggerApiGroup.系统功能))]
    public class AccountController : ControllerBase
    {
        INLogger _logger => _serviceProvider.GetRequiredService<INLogger>();
        private IRepository<TiktokUsers> _repositoryTiktokUsers => _serviceProvider.GetRequiredService<IRepository<TiktokUsers>>();
        private IRepository<TiktokUsersDaily> _repositoryTiktokUsersDaily => _serviceProvider.GetRequiredService<IRepository<TiktokUsersDaily>>();
        private IRepository<AccountConfig> _repositoryAccountConfig => _serviceProvider.GetRequiredService<IRepository<AccountConfig>>();
        private IDataPermissionContextService _dataPermissionContextService => _serviceProvider.GetRequiredService<IDataPermissionContextService>();

        /// <summary>
        /// 服务提供程序
        /// </summary>
        public readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="serviceProvider"></param>
        public AccountController(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// 查询TikTok用户账号信息（分页+关键词搜索）
        /// </summary>
        /// <param name="request">查询请求参数</param>
        /// <returns>分页用户信息</returns>
        [HttpPost("getTiktokUsers")]
        [ProducesResponseType(typeof(PageResult<TiktokUsers>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTiktokUsers([FromBody] TiktokUsersQueryRequest request)
        {
            if (request.PageIndex == null || request.PageIndex <= 0)
            {
                request.PageIndex = 1;
            }
            if (request.PageSize == null || request.PageSize <= 0)
            {
                request.PageSize  = 20;
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
                    return this.PagedResult(new PageResult<TiktokUsers>
                    {
                        Items = new List<TiktokUsers>(),
                        totalCount = 0,
                        PageIndex = request.PageIndex.Value,
                        PageSize = request.PageSize.Value
                    }, "查询成功");
                }

                // 构建查询条件
                Expression<Func<TiktokUsers, bool>>? predicate = null;

                // 数据权限过滤 + 关键词搜索
                if (!permissionContext.IsAdmin)
                {
                    // 非管理员：权限过滤 + 关键词搜索
                    if (!string.IsNullOrWhiteSpace(request.Keyword))
                    {
                        var keyword = request.Keyword.Trim();
                        predicate = x => permissionContext.AccountConfigUniqueIds.Contains(x.UniqueId) &&
                                       (x.UniqueId.Contains(keyword) ||
                                        (x.Nickname != null && x.Nickname.Contains(keyword)));
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
                                       (x.Nickname != null && x.Nickname.Contains(keyword));
                    }
                }

                // 构建排序表达式
                Expression<Func<TiktokUsers, object>>? orderBy = null;
                if (!string.IsNullOrWhiteSpace(request.OrderBy))
                {
                    orderBy = request.OrderBy.ToLower() switch
                    {
                        "uniqueid" => x => x.UniqueId,
                        "nickname" => x => x.Nickname ?? "",
                        "followercount" => x => x.FollowerCount,
                        "videocount" => x => x.VideoCount,
                        "heartcount" => x => x.HeartCount, 
                        "updatedat" => x => x.UpdatedAt,
                        _ => x => x.UpdatedAt
                    };
                }
                else
                {
                    // 默认排序
                    orderBy = x => x.UpdatedAt;
                }

                // 执行分页查询
                var result = await _repositoryTiktokUsers.GetPagedListAsync(
                    predicate: predicate,
                    pageIndex: request.PageIndex.Value,
                    pageSize: request.PageSize.Value,
                    orderBy: orderBy,
                    ascending: request.Ascending
                );

                var pageResult = new PageResult<TiktokUsers>
                {
                    Items = result.Items,
                    totalCount = result.TotalCount,
                    PageIndex = result.PageIndex,
                    PageSize = result.PageSize
                };

                return this.PagedResult(pageResult, "查询成功");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"查询TikTok用户信息失败: {ex.Message}");
                return this.Fail("查询用户信息失败");
            }
        }

        /// <summary>
        /// 查询TikTok用户每日更新记录（分页）
        /// </summary>
        /// <param name="request">查询请求参数</param>
        /// <returns>分页每日记录信息</returns>
        [HttpPost("getTiktokUsersDaily")]

        [ProducesResponseType(typeof(PageResult<TiktokUsersDaily>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTiktokUsersDaily([FromBody] TiktokUsersDailyQueryRequest request)
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
                    return this.PagedResult(new PageResult<TiktokUsersDaily>
                    {
                        Items = new List<TiktokUsersDaily>(),
                        totalCount = 0,
                        PageIndex = request.PageIndex.Value,
                        PageSize = request.PageSize.Value
                    }, "查询成功");
                }

                // 构建查询条件
                Expression<Func<TiktokUsersDaily, bool>> predicate = x => true;
                if (request.Id != null && request.Id.IsNullOrEmpty())
                {

                    predicate = predicate.And(x => x.Id == request.Id);
                }

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
                                                 (x.Nickname != null && x.Nickname.Contains(keyword)));
                }

                // 3. 记录日期范围过滤
                if (request.StartDate != null && request.StartDate.HasValue)
                {
                    var startDate = request.StartDate.Value.Date;
                    predicate = predicate.And(x => x.RecordDate >= startDate);
                }

                if (request.EndDate != null && request.EndDate.HasValue)
                {
                    var endDate = request.EndDate.Value.Date.AddDays(1);
                    predicate = predicate.And(x => x.RecordDate < endDate);
                }

                // 4. 创建时间范围过滤
                if (request.CreatedStartDate != null && request.CreatedStartDate.HasValue)
                {
                    var createdStartDate = request.CreatedStartDate.Value.Date;
                    predicate = predicate.And(x => x.CreatedAt >= createdStartDate);
                }

                if (request.CreatedEndDate != null && request.CreatedEndDate.HasValue)
                {
                    var createdEndDate = request.CreatedEndDate.Value.Date.AddDays(1);
                    predicate = predicate.And(x => x.CreatedAt < createdEndDate);
                }

                // 构建排序表达式
                Expression<Func<TiktokUsersDaily, object>>? orderBy = null;
                if (!string.IsNullOrWhiteSpace(request.OrderBy))
                {
                    orderBy = request.OrderBy.ToLower() switch
                    {
                        "id" => x => x.Id,
                        "uniqueid" => x => x.UniqueId,
                        "nickname" => x => x.Nickname ?? "",
                        "followercount" => x => x.FollowerCount,
                        "followingcount" => x => x.FollowingCount,
                        "videocount" => x => x.VideoCount,
                        "heartcount" => x => x.HeartCount,
                        "recorddate" => x => x.RecordDate,
                        "createdat" => x => x.CreatedAt,
                        "updatedat" => x => x.UpdatedAt,
                        _ => x => x.RecordDate
                    };
                }
                else
                {
                    // 默认排序
                    orderBy = x => x.RecordDate;
                }

                // 执行分页查询
                var result = await _repositoryTiktokUsersDaily.GetPagedListAsync(
                    predicate: predicate,
                    pageIndex: request.PageIndex.Value,
                    pageSize: request.PageSize.Value,
                    orderBy: orderBy,
                    ascending: request.Ascending
                );

                var pageResult = new PageResult<TiktokUsersDaily>
                {
                    Items = result.Items,
                    totalCount = result.TotalCount,
                    PageIndex = result.PageIndex,
                    PageSize = result.PageSize
                };

                return this.PagedResult(pageResult, "查询成功");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"查询TikTok用户每日记录失败: {ex.Message}");
                return this.Fail("查询每日记录失败");
            }
        }

        /// <summary>
        /// 获取TikTok用户统计信息
        /// </summary>
        /// <returns>统计信息</returns>
        [HttpGet("users/statistics")]
        [Obsolete]
        public async Task<IActionResult> GetUsersStatistics()
        {
            try
            {
                var totalUsers = await _repositoryTiktokUsers.CountAsync();
                var totalDailyRecords = await _repositoryTiktokUsersDaily.CountAsync();
                var todayRecords = await _repositoryTiktokUsersDaily.CountAsync(x => x.RecordDate == DateTime.Today);

                return this.Success(new
                {
                    totalUsers,
                    totalDailyRecords,
                    todayRecords,
                    lastUpdateTime = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"获取统计信息失败: {ex.Message}");
                return this.Fail("获取统计信息失败");
            }
        }

        /// <summary>
        /// 获取账号增量统计数据 - 修复版本
        /// </summary>
        [HttpPost("getAccountStatistics")]
        [ProducesResponseType(typeof(List<AccountStatisticsResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAccountStatistics([FromBody] AccountStatisticsRequest request)
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
                    return this.Success(new List<AccountStatisticsResponse>(), "查询成功");
                }

                // 设置默认时间范围
                var startDate = request.StartDate ?? DateTime.Today.AddDays(-30);
                var endDate = request.EndDate ?? DateTime.Today;

                // 构建查询条件
                Expression<Func<TiktokUsersDaily, bool>> predicate = x =>
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
                var dailyData = await _repositoryTiktokUsersDaily.QueryListAsync(predicate);

                if (!dailyData.Any())
                {
                    return this.Success(new List<AccountStatisticsResponse>(), "暂无数据");
                }

                // 按账号分组统计
                var accountGroups = dailyData.GroupBy(x => x.UniqueId);
                var results = new List<AccountStatisticsResponse>();

                foreach (var group in accountGroups)
                {
                    var accountData = group.OrderBy(x => x.RecordDate).ToList();
                    var latestData = accountData.LastOrDefault();
                    var firstData = accountData.FirstOrDefault(); // 修复：使用期间开始数据

                    if (latestData == null || firstData == null) continue;

                    var statistics = new AccountStatisticsResponse
                    {
                        UniqueId = group.Key,
                        Nickname = latestData.Nickname,
                        DateRange = $"{startDate:yyyy-MM-dd} 至 {endDate:yyyy-MM-dd}",
                        
                        // 修复：使用期间开始和结束数据计算增长
                        FollowerGrowth = CalculateGrowthStatistics(
                            accountData.Select(x => x.FollowerCount).ToList(),
                            latestData.FollowerCount,
                            firstData.FollowerCount // 使用期间开始值
                        ),
                        FollowingGrowth = CalculateGrowthStatistics(
                            accountData.Select(x => x.FollowingCount).ToList(),
                            latestData.FollowingCount,
                            firstData.FollowingCount
                        ),
                        VideoGrowth = CalculateGrowthStatistics(
                            accountData.Select(x => x.VideoCount).ToList(),
                            latestData.VideoCount,
                            firstData.VideoCount
                        ),
                        HeartGrowth = CalculateGrowthStatistics(
                            accountData.Select(x => x.HeartCount).ToList(),
                            latestData.HeartCount,
                            firstData.HeartCount
                        ),
                        DiggGrowth = CalculateGrowthStatistics(
                            accountData.Select(x => x.DiggCount).ToList(),
                            latestData.DiggCount,
                            firstData.DiggCount
                        ),
                        FriendGrowth = CalculateGrowthStatistics(
                            accountData.Select(x => x.FriendCount).ToList(),
                            latestData.FriendCount,
                            firstData.FriendCount
                        ),
                        
                        // 点赞率计算保持不变（已经是正确的）
                        LikeRate = CalculateAccountLikeRate(accountData, latestData, firstData),
                        
                        // 删除重复的平均获赞数，或者改名为其他指标
                        AverageHeartCount = CalculateAccountEngagementTrend(accountData, latestData, firstData),
                        
                        DailyTrends = accountData.Select(x => new DailyTrend
                        {
                            Date = x.RecordDate,
                            FollowerCount = x.FollowerCount,
                            FollowingCount = x.FollowingCount,
                            VideoCount = x.VideoCount,
                            HeartCount = x.HeartCount,
                            LikeRate = x.VideoCount > 0 ? Math.Round((decimal)x.HeartCount / x.VideoCount, 2) : 0
                        }).ToList()
                    };

                    results.Add(statistics);
                }

                return this.Success(results, "查询成功");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"获取账号统计数据失败: {ex.Message}");
                return this.Fail("获取统计数据失败");
            }
        }

        /// <summary>
        /// 计算增长统计数据 - 修复版本
        /// </summary>
        /// <param name="values">期间内所有值列表</param>
        /// <param name="currentValue">期间结束值</param>
        /// <param name="startValue">期间开始值</param>
        /// <returns>增长统计</returns>
        [HiddenAPI]
        private GrowthStatistics CalculateGrowthStatistics(List<int> values, int currentValue, int startValue)
        {
            var growth = currentValue - startValue; // 修复：期间总增长
            var growthRate = startValue > 0 ? (decimal)growth / startValue * 100 : 0; // 修复：基于期间开始值

            return new GrowthStatistics
            {
                CurrentValue = currentValue,
                PreviousValue = startValue, // 改名：实际是期间开始值
                Growth = growth,
                GrowthRate = Math.Round(growthRate, 2),
                MaxValue = values.Any() ? values.Max() : currentValue,
                MinValue = values.Any() ? values.Min() : currentValue,
                AvgValue = values.Any() ? Math.Round((decimal)values.Average(), 2) : currentValue
            };
        }

        /// <summary>
        /// 计算账号每条视频平均获赞数
        /// </summary>
        /// <param name="accountData">账号历史数据</param>
        /// <param name="latestData">最新数据</param>
        /// <param name="previousData">前一天数据</param>
        /// <returns>点赞率统计</returns>
        [HiddenAPI]
        private RateStatistics CalculateAccountLikeRate(List<TiktokUsersDaily> accountData, TiktokUsersDaily latestData, TiktokUsersDaily? previousData)
        {
            // 计算当前点赞率（平均每视频获赞数）
            var currentRate = latestData.VideoCount > 0 ?
                Math.Round((decimal)latestData.HeartCount / latestData.VideoCount, 2) : 0;

            // 计算前一天点赞率
            var previousRate = previousData != null && previousData.VideoCount > 0 ?
                Math.Round((decimal)previousData.HeartCount / previousData.VideoCount, 2) : 0;

            // 计算所有历史数据的点赞率
            var allRates = accountData.Where(x => x.VideoCount > 0)
                .Select(x => Math.Round((decimal)x.HeartCount / x.VideoCount, 2))
                .ToList();

            return new RateStatistics
            {
                CurrentRate = currentRate,
                PreviousRate = previousRate,
                RateChange = Math.Round(currentRate - previousRate, 2),
                MaxRate = allRates.Any() ? allRates.Max() : currentRate,
                MinRate = allRates.Any() ? allRates.Min() : currentRate,
                AvgRate = allRates.Any() ? Math.Round(allRates.Average(), 2) : currentRate,
                Description = "点赞率：平均每条视频获赞数"
            };
        }

        /// <summary>
        /// 计算账号互动趋势统计
        /// </summary>
        /// <param name="accountData">账号历史数据</param>
        /// <param name="latestData">最新数据</param>
        /// <param name="firstData">期间开始数据</param>
        /// <returns>互动趋势统计</returns>
        [HiddenAPI]
        private RateStatistics CalculateAccountEngagementTrend(List<TiktokUsersDaily> accountData, TiktokUsersDaily latestData, TiktokUsersDaily firstData)
        {
            // 计算当前互动效率（获赞增长/视频增长）
            var videoGrowth = latestData.VideoCount - firstData.VideoCount;
            var heartGrowth = latestData.HeartCount - firstData.HeartCount;
            
            var currentRate = videoGrowth > 0 ? 
                Math.Round((decimal)heartGrowth / videoGrowth, 2) : 0;

            // 计算历史平均互动效率
            var dailyEfficiencies = new List<decimal>();
            for (int i = 1; i < accountData.Count; i++)
            {
                var prevDay = accountData[i - 1];
                var currDay = accountData[i];
                var dailyVideoGrowth = currDay.VideoCount - prevDay.VideoCount;
                var dailyHeartGrowth = currDay.HeartCount - prevDay.HeartCount;
                
                if (dailyVideoGrowth > 0)
                {
                    dailyEfficiencies.Add(Math.Round((decimal)dailyHeartGrowth / dailyVideoGrowth, 2));
                }
            }

            return new RateStatistics
            {
                CurrentRate = currentRate,
                PreviousRate = 0,
                RateChange = 0,
                MaxRate = dailyEfficiencies.Any() ? dailyEfficiencies.Max() : currentRate,
                MinRate = dailyEfficiencies.Any() ? dailyEfficiencies.Min() : currentRate,
                AvgRate = dailyEfficiencies.Any() ? Math.Round(dailyEfficiencies.Average(), 2) : currentRate,
                Description = "互动效率：新增获赞数/新增视频数（反映内容质量趋势）"
            };
        }

    }
}
