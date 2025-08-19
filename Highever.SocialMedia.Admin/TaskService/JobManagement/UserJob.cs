using Highever.SocialMedia.Application.Contracts;
using Highever.SocialMedia.Common;
using Highever.SocialMedia.Domain.Entity;
using Highever.SocialMedia.Domain.Repository;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Text.Json;

namespace Highever.SocialMedia.Admin.TaskService
{
    public class UserJob : ITaskExecutor
    {
        // 注入
        private readonly IServiceProvider _serviceProvider;
        private readonly TikhubSettings _tikTokSettings; 
        private readonly INLogger _logger;
        private readonly ITKAPIService  _tKAPIService;


        // 使用信号量控制并发数，设置为1确保串行处理
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1); 
        private HttpClientHelper _httpclient => _serviceProvider.GetRequiredService<HttpClientHelper>();
        private IRepository<AccountConfig> repositoryAccountConfig => _serviceProvider.GetRequiredService<IRepository<AccountConfig>>();
        private IRepository<TiktokUsers> _repositoryTiktokUsers => _serviceProvider.GetRequiredService<IRepository<TiktokUsers>>();
        private IRepository<TiktokUsersDaily> _repositoryTiktokUsersDaily => _serviceProvider.GetRequiredService<IRepository<TiktokUsersDaily>>();

        public UserJob(IServiceProvider serviceProvider, IOptions<TikhubSettings> tikTokSettings, INLogger logger, ITKAPIService tKAPIService)
        {
            _serviceProvider = serviceProvider;
            _tikTokSettings = tikTokSettings.Value;
            _logger = logger; // 注入日志记录器 
            _tKAPIService = tKAPIService;
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
                var activeConfigs = await repositoryAccountConfig.GetListAsync(x => x.IsActive);
                totalUsers = activeConfigs.Count;

                // 关键日志：处理开始里程碑 - 入库
                _logger.TaskMilestone(taskName, "数据获取完成", null, taskRunId, new Dictionary<string, object>
                {
                    ["UserCount"] = totalUsers
                });

                // 详细日志：处理信息 - 仅文件
                _logger.TaskInfo(taskName, $"开始处理 {totalUsers} 个用户", null, taskRunId);

                // 直接循环处理每个用户，使用信号量确保一次只处理一个
                for (int i = 0; i < activeConfigs.Count; i++)
                {
                    var config = activeConfigs[i];
                    var userIndex = i + 1;

                    Console.WriteLine($"准备处理第 {userIndex}/{totalUsers} 个用户: {config.UniqueId}");

                    var success = await ProcessSingleUserAsync(config);

                    apiCalls++;
                    if (success)
                    {
                        successCount++;
                        Console.WriteLine($"✓ 用户 {config.UniqueId} 数据同步成功 ({userIndex}/{totalUsers})");
                    }
                    else
                    {
                        failedCount++;
                        Console.WriteLine($"✗ 用户 {config.UniqueId} 数据同步失败 ({userIndex}/{totalUsers})");
                    }

                    // 每处理50%时记录里程碑 - 入库
                    var progressPercent = userIndex * 100 / totalUsers;
                    if (progressPercent >= 50 && (userIndex - 1) * 100 / totalUsers < 50)
                    {
                        _logger.TaskMilestone(taskName, "处理进度50%", null, taskRunId, new Dictionary<string, object>
                        {
                            ["ProcessedCount"] = userIndex,
                            ["SuccessCount"] = successCount,
                            ["FailedCount"] = failedCount
                        });
                    }

                    // 用户间延迟
                    if (i < activeConfigs.Count - 1)
                    {
                        await Task.Delay(_tikTokSettings.RequestIntervalMs);
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
                Console.WriteLine($"成功率: {(totalUsers > 0 ? (successCount * 100.0 / totalUsers) : 0):F1}%");
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
        private async Task<List<AccountConfig>> GetActiveUserConfigsAsync()
        {
            try
            {
                return await repositoryAccountConfig.QueryListAsync(x => x.IsActive == true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"获取用户配置时发生错误: {ex.Message}");
                return new List<AccountConfig>();
            }
        }
         
        /// <summary>
        /// 更新TiktokUsers表（插入或更新）- 在外层事务中执行
        /// </summary>
        private async Task UpdateTiktokUsersAsync(TikTokApiResponse apiResponse)
        {
            try
            {
                var user = apiResponse.Data.UserInfo.User;
                var stats = apiResponse.Data.UserInfo.Stats;
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
                    UpdatedAt = DateTime.Now
                };
                var existingUser = await _repositoryTiktokUsers.FirstOrDefaultAsync(x => x.UniqueId == user.UniqueId);
                #region 更新头像 - 使用智能下载
                try
                {
                    if (!string.IsNullOrEmpty(user.AvatarMedium))
                    {
                        // 使用智能下载，自动检查本地是否已存在
                        var localAvatarPath = await _httpclient.SmartDownloadAvatarAsync(
                            user.AvatarMedium,
                            user.UniqueId,
                            "uploads/avatars");

                        tiktokUser.AvatarLarge = AppSettingConifgHelper.ReadAppSettings("HOST_IP") + localAvatarPath;
                        tiktokUser.AvatarMedium = AppSettingConifgHelper.ReadAppSettings("HOST_IP") + localAvatarPath;
                        tiktokUser.AvatarThumb = AppSettingConifgHelper.ReadAppSettings("HOST_IP") + localAvatarPath;

                        Console.WriteLine($"用户 {user.UniqueId} 头像路径: {localAvatarPath}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"下载用户 {user.UniqueId} 头像失败: {ex.Message}");
                }
                #endregion

                // 在当前事务中执行数据库操作
                if (existingUser != null)
                {
                    tiktokUser.Id = existingUser.Id;
                    tiktokUser.CreatedAt = existingUser.CreatedAt;
                    await _repositoryTiktokUsers.UpdateAsync(tiktokUser);
                    Console.WriteLine($"更新用户 {user.UniqueId} 的TiktokUsers记录");
                }
                else
                {
                    await _repositoryTiktokUsers.InsertAsync(tiktokUser);
                    Console.WriteLine($"插入用户 {user.UniqueId} 的TiktokUsers记录");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"更新TiktokUsers失败: {ex.Message}");
                throw; // 重新抛出异常，让外层事务处理
            }
        }

        /// <summary>
        /// 更新TiktokUsersDaily表（插入或更新当天记录）- 在外层事务中执行
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

                // 在当前事务中执行数据库操作
                if (existingDaily != null)
                {
                    tiktokUserDaily.CreatedAt = existingDaily.CreatedAt; // 保持原创建时间
                    await _repositoryTiktokUsersDaily.UpdateAsync(tiktokUserDaily);
                    Console.WriteLine($"更新用户 {user.UniqueId} 今日的TiktokUsersDaily记录");
                }
                else
                {
                    tiktokUserDaily.CreatedAt = DateTime.Now;
                    await _repositoryTiktokUsersDaily.InsertAsync(tiktokUserDaily);
                    Console.WriteLine($"插入用户 {user.UniqueId} 今日的TiktokUsersDaily记录");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"更新TiktokUsersDaily表时发生错误: {ex.Message}");
                throw; // 重新抛出异常，让外层事务处理
            }
        }

        /// <summary>
        /// 并发处理一批用户数据
        /// </summary>
        private async Task<(int SuccessCount, int FailedCount, int ApiCalls)> ProcessBatchAsync(
            List<AccountConfig> batch, int startIndex)
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
        /// 处理单个用户数据 - 使用事务确保数据一致性
        /// </summary>
        private async Task<bool> ProcessSingleUserAsync(AccountConfig config)
        {
            // 使用信号量确保同时只有一个用户在处理
            await _semaphore.WaitAsync();
            try
            {
                var (apiResponse, errorMessage) = await _tKAPIService.FetchUserProfileWithRetryAsync(config.UniqueId, config.SecUid);

                if (apiResponse != null)
                {
                    // 使用事务处理用户数据更新，确保数据一致性
                    var success = await repositoryAccountConfig.ExecuteTransactionAsync(async () =>
                    {
                        try
                        {
                            // 在同一个事务中更新用户信息和统计信息
                            await UpdateTiktokUsersAsync(apiResponse);
                            await UpdateTiktokUsersDailyAsync(apiResponse);

                            _logger.TaskInfo("UserDataSync", $"用户 {config.UniqueId} 事务提交成功", null, null);
                            return true;
                        }
                        catch (Exception ex)
                        {
                            _logger.TaskError($"UserDataSync-{config.UniqueId}", ex, null, null);
                            Console.WriteLine($"用户 {config.UniqueId} 事务内操作失败: {ex.Message}");
                            throw; // 重新抛出异常，触发事务回滚
                        }
                    });

                    if (success)
                    {
                        // 记录成功的API调用 - 仅文件
                        _logger.TaskApiCall("UserDataSync", config.UniqueId, true, null, null);
                        return true;
                    }
                }

                // 记录API调用失败 - 仅文件
                _logger.TaskApiCall("UserDataSync", config.UniqueId, false, $"API_FAIL:{errorMessage}", null);

                // 记录失败用户的里程碑日志
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
            finally
            {
                _semaphore.Release();
            }
        }
    }
}
