using Hangfire;
using Highever.SocialMedia.Common;
using Highever.SocialMedia.Domain.Entity;
using Highever.SocialMedia.Domain.Repository;
using SQLBuilder.Core.Repositories;
using System.Text.Json;
using Highever.SocialMedia.Admin.TaskService.Models;
using System.Diagnostics;
using System.Text.Json.Serialization;
using System.Transactions;

namespace Highever.SocialMedia.Admin.TaskService
{
    public class UserJob : ITaskExecutor
    {
        // 注入
        private readonly IServiceProvider _serviceProvider;
        private readonly TikhubSettings _tikTokSettings;
        private readonly RateLimitController _rateLimitController;
        private HttpClientHelper _httpclient => _serviceProvider.GetRequiredService<HttpClientHelper>();
        private IRepository<TiktokUserConfig> repositoryTiktokUserConfig => _serviceProvider.GetRequiredService<IRepository<TiktokUserConfig>>();
        private IRepository<TiktokUsers> _repositoryTiktokUsers => _serviceProvider.GetRequiredService<IRepository<TiktokUsers>>();
        private IRepository<TiktokUsersDaily> _repositoryTiktokUsersDaily => _serviceProvider.GetRequiredService<IRepository<TiktokUsersDaily>>();

        public UserJob(IServiceProvider serviceProvider, TikhubSettings tikTokSettings)
        {
            _serviceProvider = serviceProvider;
            _tikTokSettings = tikTokSettings;
            _rateLimitController = new RateLimitController(_tikTokSettings);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="taskName"></param>
        /// <returns></returns>
        public async Task Execute(string taskName)
        {
            var stopwatch = Stopwatch.StartNew();
            var successCount = 0;
            var failureCount = 0;
            
            try
            {
                Console.WriteLine($"开始执行TikTok用户数据同步任务: {taskName}");
                Console.WriteLine($"配置: 最大并发={_tikTokSettings.MaxConcurrency}, 批次大小={_tikTokSettings.BatchSize}, 每分钟限制={_tikTokSettings.MaxRequestsPerMinute}");
                
                var activeConfigs = await GetActiveUserConfigsAsync();
                Console.WriteLine($"获取到 {activeConfigs.Count} 个活跃用户配置");

                if (!activeConfigs.Any())
                {
                    Console.WriteLine("没有找到活跃的用户配置，任务结束");
                    return;
                }

                // 使用配置中的批次大小和并发数
                for (int batchStart = 0; batchStart < activeConfigs.Count; batchStart += _tikTokSettings.BatchSize)
                {
                    var batch = activeConfigs.Skip(batchStart).Take(_tikTokSettings.BatchSize).ToList();
                    Console.WriteLine($"开始处理第 {batchStart / _tikTokSettings.BatchSize + 1} 批，共 {batch.Count} 个用户");

                    var batchResults = await ProcessBatchAsync(batch, batchStart + 1);
                    
                    successCount += batchResults.SuccessCount;
                    failureCount += batchResults.FailureCount;

                    // 批次间延迟
                    if (batchStart + _tikTokSettings.BatchSize < activeConfigs.Count)
                    {
                        Console.WriteLine($"批次完成，等待 {_tikTokSettings.BatchDelayMs}ms 后处理下一批...");
                        await Task.Delay(_tikTokSettings.BatchDelayMs);
                    }

                    GC.Collect();
                }

                stopwatch.Stop();
                Console.WriteLine($"TikTok用户数据同步任务完成！");
                Console.WriteLine($"总计: {activeConfigs.Count} 个用户, 成功: {successCount}, 失败: {failureCount}");
                Console.WriteLine($"耗时: {stopwatch.Elapsed.TotalMinutes:F2} 分钟, 平均: {stopwatch.Elapsed.TotalSeconds / activeConfigs.Count:F2} 秒/用户"); 

            }
            catch (Exception ex)
            {
                Console.WriteLine($"执行任务 {taskName} 时发生严重错误: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 获取活跃的用户配置列表
        /// </summary>
        private async Task<List<TiktokUserConfig>> GetActiveUserConfigsAsync()
        {
            try
            {
                return await repositoryTiktokUserConfig.QueryListAsync(x => x.IsActive == true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"获取用户配置时发生错误: {ex.Message}");
                return new List<TiktokUserConfig>();
            }
        }

        /// <summary>
        /// 带重试机制的API调用
        /// </summary>
        private async Task<TikTokApiResponse?> FetchUserProfileWithRetryAsync(string uniqueId, string? secUid = null)
        {
            int retryCount = 0;
            
            while (retryCount < _tikTokSettings.MaxRetryCount)
            {
                // 等待限流控制器许可
                await _rateLimitController.WaitForPermissionAsync();
                
                try
                {
                    string ApiUrl = "https://api.tikhub.io/api/v1/tiktok/app/v2/fetch_user_detail";
                    var url = $"{ApiUrl}?uniqueId={uniqueId}";
                    if (!string.IsNullOrEmpty(secUid))
                    {
                        url += $"&secUid={secUid}";
                    }

                    var headers = new Dictionary<string, string>
                    {
                        { "Authorization", _tikTokSettings.ApiToken },
                        { "User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36" }
                    };

                    var responseJson = await _httpclient.GetAsync(url, headers, TimeSpan.FromMinutes(_tikTokSettings.TimeoutMinutes));
                    
                    if (string.IsNullOrWhiteSpace(responseJson))
                    {
                        throw new InvalidOperationException("API返回空响应");
                    }

                    var apiResponse = JsonSerializer.Deserialize<TikTokApiResponse>(responseJson, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                    });

                    if (apiResponse?.Code == 200 && apiResponse.Data?.UserInfo?.User != null)
                    {
                        _rateLimitController.ReportSuccess();
                        return apiResponse;
                    }
                    else if (apiResponse?.Code == 429) // Too Many Requests
                    {
                        _rateLimitController.ReportError(isRateLimited: true);
                        Console.WriteLine($"API返回429限流错误，用户: {uniqueId}");
                    }
                    else
                    {
                        _rateLimitController.ReportError();
                        Console.WriteLine($"API返回错误: Code={apiResponse?.Code}, User={uniqueId}");
                    }
                }
                catch (TaskCanceledException)
                {
                    _rateLimitController.ReportError();
                    Console.WriteLine($"用户 {uniqueId} 请求超时 (第 {retryCount + 1} 次)");
                }
                catch (HttpRequestException ex) when (ex.Message.Contains("429"))
                {
                    _rateLimitController.ReportError(isRateLimited: true);
                    Console.WriteLine($"用户 {uniqueId} 遇到限流: {ex.Message}");
                }
                catch (Exception ex)
                {
                    _rateLimitController.ReportError();
                    Console.WriteLine($"用户 {uniqueId} 请求异常: {ex.Message} (第 {retryCount + 1} 次)");
                }
                finally
                {
                    _rateLimitController.ReleasePermission();
                }

                retryCount++;
                
                if (retryCount < _tikTokSettings.MaxRetryCount)
                {
                    var delaySeconds = Math.Min(Math.Pow(2, retryCount), _tikTokSettings.MaxDelaySeconds);
                    await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
                }
            }

            return null;
        }

        /// <summary>
        /// 更新TiktokUsers表（插入或更新）
        /// </summary>
        private async Task UpdateTiktokUsersAsync(TikTokApiResponse apiResponse)
        {
            try
            {
                var user = apiResponse.Data.UserInfo.User;
                var stats = apiResponse.Data.UserInfo.Stats;

                // 检查是否已存在该用户
                var existingUser = await _repositoryTiktokUsers.FirstOrDefaultAsync(x => x.UniqueId == user.UniqueId);

                var tiktokUser = new TiktokUsers
                {
                    Id = long.Parse(user.Id),
                    UniqueId = user.UniqueId,
                    Nickname = user.Nickname,
                    Signature = user.Signature,
                    AvatarLarge = user.AvatarLarger,
                    AvatarMedium = user.AvatarMedium,
                    AvatarThumb = user.AvatarThumb,
                    Verified = user.Verified,
                    PrivateAccount = user.PrivateAccount,
                    Language = user.Language,
                    CreateTime = user.CreateTime,
                    SecUid = user.SecUid,
                    FollowerCount = stats.FollowerCount,
                    FollowingCount = stats.FollowingCount,
                    VideoCount = stats.VideoCount,
                    HeartCount = stats.HeartCount,
                    DiggCount = stats.DiggCount,
                    FriendCount = stats.FriendCount,
                    RecordDate = DateTime.Today,
                    UpdatedAt = DateTime.Now
                };

                if (existingUser != null)
                {
                    // 更新现有记录
                    tiktokUser.CreatedAt = existingUser.CreatedAt; // 保持原创建时间
                    await _repositoryTiktokUsers.UpdateAsync(tiktokUser);
                    Console.WriteLine($"更新用户 {user.UniqueId} 的TiktokUsers记录");
                }
                else
                {
                    // 插入新记录
                    tiktokUser.CreatedAt = DateTime.Now;
                    await _repositoryTiktokUsers.InsertAsync(tiktokUser);
                    Console.WriteLine($"插入用户 {user.UniqueId} 的TiktokUsers记录");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"更新TiktokUsers表时发生错误: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 更新TiktokUsersDaily表（插入或更新当天记录）
        /// </summary>
        private async Task UpdateTiktokUsersDailyAsync(TikTokApiResponse apiResponse)
        {
            try
            {
                var user = apiResponse.Data.UserInfo.User;
                var stats = apiResponse.Data.UserInfo.Stats;
                var today = DateTime.Today;

                // 检查今天是否已有记录
                var existingDaily = await _repositoryTiktokUsersDaily.FirstOrDefaultAsync(
                    x => x.Id == long.Parse(user.Id) && x.RecordDate == today);

                var tiktokUserDaily = new TiktokUsersDaily
                {
                    Id = long.Parse(user.Id),
                    UniqueId = user.UniqueId,
                    Nickname = user.Nickname,
                    Signature = user.Signature,
                    AvatarLarge = user.AvatarLarger,
                    AvatarMedium = user.AvatarMedium,
                    AvatarThumb = user.AvatarThumb,
                    Verified = user.Verified,
                    PrivateAccount = user.PrivateAccount,
                    Language = user.Language,
                    CreateTime = user.CreateTime,
                    SecUid = user.SecUid,
                    FollowerCount = stats.FollowerCount,
                    FollowingCount = stats.FollowingCount,
                    VideoCount = stats.VideoCount,
                    HeartCount = stats.HeartCount,
                    DiggCount = stats.DiggCount,
                    FriendCount = stats.FriendCount,
                    RecordDate = today,
                    ResponseData = JsonSerializer.Serialize(apiResponse), // 保存原始JSON
                    UpdatedAt = DateTime.Now
                };

                if (existingDaily != null)
                {
                    // 更新今天的记录
                    tiktokUserDaily.CreatedAt = existingDaily.CreatedAt; // 保持原创建时间
                    await _repositoryTiktokUsersDaily.UpdateAsync(tiktokUserDaily);
                    Console.WriteLine($"更新用户 {user.UniqueId} 今日的TiktokUsersDaily记录");
                }
                else
                {
                    // 插入今天的新记录
                    tiktokUserDaily.CreatedAt = DateTime.Now;
                    await _repositoryTiktokUsersDaily.InsertAsync(tiktokUserDaily);
                    Console.WriteLine($"插入用户 {user.UniqueId} 今日的TiktokUsersDaily记录");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"更新TiktokUsersDaily表时发生错误: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 并发处理一批用户数据
        /// </summary>
        private async Task<(int SuccessCount, int FailureCount)> ProcessBatchAsync(
            List<TiktokUserConfig> batch, int startIndex)
        {
            var successCount = 0;
            var failureCount = 0;
            var lockObject = new object();

            var tasks = batch.Select(async (config, index) =>
            {
                var userIndex = startIndex + index;
                Console.WriteLine($"准备处理第 {userIndex} 个用户: {config.UniqueId}");

                var success = await ProcessSingleUserAsync(config);
                
                lock (lockObject)
                {
                    if (success)
                    {
                        successCount++;
                        Console.WriteLine($"✓ 用户 {config.UniqueId} 数据同步成功 ({userIndex})");
                    }
                    else
                    {
                        failureCount++;
                        Console.WriteLine($"✗ 用户 {config.UniqueId} 数据同步失败 ({userIndex})");
                    }
                }
            });

            await Task.WhenAll(tasks);
            return (successCount, failureCount);
        }

        /// <summary>
        /// 处理单个用户数据
        /// </summary>
        private async Task<bool> ProcessSingleUserAsync(TiktokUserConfig config)
        {
            try
            {
                var apiResponse = await FetchUserProfileWithRetryAsync(config.UniqueId, config.SecUid);
                
                if (apiResponse != null)
                {
                    using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
                    
                    await UpdateTiktokUsersAsync(apiResponse);
                    await UpdateTiktokUsersDailyAsync(apiResponse);
                    
                    scope.Complete();
                    return true;
                }
                
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"处理用户 {config.UniqueId} 时发生错误: {ex.Message}");
                return false;
            }
        }
    }
}
