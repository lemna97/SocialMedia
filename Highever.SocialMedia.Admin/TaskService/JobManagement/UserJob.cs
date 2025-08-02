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
using Microsoft.Extensions.Options;

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

        private readonly INLogger _logger; // 添加日志记录器

        public UserJob(IServiceProvider serviceProvider, IOptions<TikhubSettings> tikTokSettings, INLogger logger)
        {
            _serviceProvider = serviceProvider;
            _tikTokSettings = tikTokSettings.Value; // 通过 .Value 获取配置值
            _rateLimitController = new RateLimitController(_tikTokSettings); 
            _logger = logger; // 注入日志记录器 
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="taskName"></param>
        /// <returns></returns>
        public async Task Execute(string taskName)
        {
            var stopwatch = Stopwatch.StartNew();
            int totalUsers = 0, successCount = 0, failedCount = 0, apiCalls = 0;
            long? taskRunId = null;
            
            try
            { 
                var activeConfigs = await repositoryTiktokUserConfig.GetListAsync(x => x.IsActive);
                
                totalUsers = activeConfigs.Count;
                
                // 关键日志：处理开始里程碑 - 入库
                _logger.TaskMilestone(taskName, "数据获取完成", null, taskRunId, new Dictionary<string, object>
                {
                    ["UserCount"] = totalUsers
                });

                // 详细日志：处理信息 - 仅文件
                _logger.TaskInfo(taskName, $"开始处理 {totalUsers} 个用户", null, taskRunId);

                // 分批处理用户
                for (int batchStart = 0; batchStart < activeConfigs.Count; batchStart += _tikTokSettings.BatchSize)
                {
                    var batch = activeConfigs.Skip(batchStart).Take(_tikTokSettings.BatchSize).ToList();
                    var batchNumber = batchStart / _tikTokSettings.BatchSize + 1;
                    
                    var batchResults = await ProcessBatchAsync(batch, batchNumber);
                    
                    successCount += batchResults.SuccessCount;
                    failedCount += batchResults.FailedCount;
                    apiCalls += batchResults.ApiCalls;
                    
                    // 详细日志：批次信息 - 仅文件
                    _logger.TaskBatchInfo(taskName, batchNumber, batch.Count, batchResults.SuccessCount, batchResults.FailedCount, taskRunId);
                    
                    // 每处理50%时记录里程碑 - 入库
                    var progressPercent = (batchStart + batch.Count) * 100 / totalUsers;
                    if (progressPercent >= 50 && batchStart * 100 / totalUsers < 50)
                    {
                        _logger.TaskMilestone(taskName, "处理进度50%", null, taskRunId, new Dictionary<string, object>
                        {
                            ["ProcessedCount"] = batchStart + batch.Count,
                            ["SuccessCount"] = successCount,
                            ["FailedCount"] = failedCount
                        });
                    }
                    
                    // 批次间延迟
                    if (batchStart + _tikTokSettings.BatchSize < activeConfigs.Count)
                    {
                        await Task.Delay(_tikTokSettings.BatchDelayMs);
                    }
                }
                
                stopwatch.Stop();
                
                // 控制台总结
                Console.WriteLine($"\n========== 任务执行完成 ==========");
                Console.WriteLine($"任务名称: {taskName}");
                Console.WriteLine($"执行时间: {stopwatch.ElapsedMilliseconds}ms");
                Console.WriteLine($"总用户数: {totalUsers}");
                Console.WriteLine($"成功数量: {successCount}");
                Console.WriteLine($"失败数量: {failedCount}");
                Console.WriteLine($"API调用: {apiCalls}");
                Console.WriteLine($"成功率: {(totalUsers > 0 ? (successCount * 100.0 / totalUsers):0):F1}%");
                Console.WriteLine($"=====================================\n");

                // 关键日志：任务完成 - 入库
                _logger.TaskComplete(taskName, stopwatch.ElapsedMilliseconds, totalUsers, successCount, failedCount, apiCalls, null, taskRunId);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                
                // 关键日志：任务错误 - 入库
                _logger.TaskError(taskName, ex, null, taskRunId);
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
        private async Task<(TikTokApiResponse? Response, string? ErrorMessage)> FetchUserProfileWithRetryAsync(string uniqueId, string? secUid = null)
        {
            int retryCount = 0;
            string? lastErrorMessage = null;
            
            while (retryCount < _tikTokSettings.MaxRetryCount)
            {
                // 等待限流控制器许可
                await _rateLimitController.WaitForPermissionAsync();
                
                try
                { 
                    var url = $"https://api.tikhub.io/api/v1/tiktok/web/fetch_user_profile?uniqueId={uniqueId}";
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
                        lastErrorMessage = "API返回空响应";
                        throw new InvalidOperationException(lastErrorMessage);
                    }

                    var apiResponse = JsonSerializer.Deserialize<TikTokApiResponse>(responseJson, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                    });

                    if (apiResponse?.Code == 200 && apiResponse.Data?.UserInfo?.User != null)
                    {
                        _rateLimitController.ReportSuccess();
                        return (apiResponse, null);
                    }
                    else if (apiResponse?.Code == 429) // Too Many Requests
                    {
                        _rateLimitController.ReportError(isRateLimited: true);
                        lastErrorMessage = "API返回429限流错误";
                        Console.WriteLine($"{lastErrorMessage}，用户: {uniqueId}");
                    }
                    else if (apiResponse?.Code == 200 && apiResponse.Data?.UserInfo?.User == null)
                    {
                        // API返回200但用户数据为空，继续重试
                        lastErrorMessage = "API返回200但用户数据为空";
                        Console.WriteLine($"{lastErrorMessage}，用户: {uniqueId}，继续重试 (第 {retryCount + 1} 次)");
                    }
                    else
                    {
                        _rateLimitController.ReportError();
                        lastErrorMessage = $"API返回错误: Code={apiResponse?.Code}";
                        Console.WriteLine($"{lastErrorMessage}, User={uniqueId}");
                    }
                }
                catch (TaskCanceledException)
                {
                    _rateLimitController.ReportError();
                    lastErrorMessage = "请求超时";
                    Console.WriteLine($"用户 {uniqueId} {lastErrorMessage} (第 {retryCount + 1} 次)");
                }
                catch (HttpRequestException ex) when (ex.Message.Contains("429"))
                {
                    _rateLimitController.ReportError(isRateLimited: true);
                    lastErrorMessage = $"遇到限流: {ex.Message}";
                    Console.WriteLine($"用户 {uniqueId} {lastErrorMessage}");
                }
                catch (Exception ex)
                {
                    _rateLimitController.ReportError();
                    lastErrorMessage = $"请求异常: {ex.Message}";
                    Console.WriteLine($"用户 {uniqueId} {lastErrorMessage} (第 {retryCount + 1} 次)");
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

            return (null, lastErrorMessage ?? "未知错误");
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
                    tiktokUser.Id = existingUser.Id;
                    await _repositoryTiktokUsers.UpdateAsync(tiktokUser);
                }
                else
                {
                    await _repositoryTiktokUsers.InsertAsync(tiktokUser);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"更新TiktokUsers失败: {ex.Message}");
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
        private async Task<(int SuccessCount, int FailedCount, int ApiCalls)> ProcessBatchAsync(
            List<TiktokUserConfig> batch, int startIndex)
        {
            var successCount = 0;
            var failureCount = 0;
            var apiCalls = 0;
            var lockObject = new object();

            var tasks = batch.Select(async (config, index) =>
            {
                var userIndex = startIndex + index;
                Console.WriteLine($"准备处理第 {userIndex} 个用户: {config.UniqueId}");

                var success = await ProcessSingleUserAsync(config);
                
                lock (lockObject)
                {
                    apiCalls++;
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
            return (successCount, failureCount, apiCalls);
        }

        /// <summary>
        /// 处理单个用户数据
        /// </summary>
        private async Task<bool> ProcessSingleUserAsync(TiktokUserConfig config)
        {
            try
            {
                var (apiResponse, errorMessage) = await FetchUserProfileWithRetryAsync(config.UniqueId, config.SecUid);
                
                if (apiResponse != null)
                {
                    try
                    {
                        await UpdateTiktokUsersAsync(apiResponse);
                        await UpdateTiktokUsersDailyAsync(apiResponse);
                        
                        // 记录成功的API调用 - 仅文件
                        _logger.TaskApiCall("UserDataSync", config.UniqueId, true, null, null);
                        return true;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"数据库操作失败，用户: {config.UniqueId}, 错误: {ex.Message}");
                        
                        // 记录数据库操作失败 - 仅文件
                        _logger.TaskApiCall("UserDataSync", config.UniqueId, false, $"EXCEPTION:数据库操作失败: {ex.Message}", null);
                        
                        // 如果需要入库，添加关键日志
                        _logger.TaskError($"UserDataSync-{config.UniqueId}", ex, null, null);
                        return false;
                    }
                }
                
                // 记录API调用失败 - 仅文件
                _logger.TaskApiCall("UserDataSync", config.UniqueId, false, $"API_FAIL:{errorMessage}", null);
                
                // 如果需要入库，可以添加里程碑日志记录失败用户
                _logger.TaskMilestone("UserDataSync", $"用户失败: {config.UniqueId}", null, null, new Dictionary<string, object>
                {
                    ["FailedUser"] = config.UniqueId,
                    ["ErrorMessage"] = errorMessage ?? "未知错误"
                });
                
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"处理用户 {config.UniqueId} 时发生错误: {ex.Message}");
                
                // 记录处理异常 - 入库
                _logger.TaskError($"UserDataSync-{config.UniqueId}", ex, null, null);
                return false;
            }
        }
    }
}
