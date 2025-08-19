using Highever.SocialMedia.Application.Contracts;
using Highever.SocialMedia.Common;
using Highever.SocialMedia.Domain.Entity;
using Highever.SocialMedia.Domain.Repository;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Text.Json;

namespace Highever.SocialMedia.Admin.TaskService
{
    public class VideoJob : ITaskExecutor
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly TikhubSettings _tikTokSettings; 
        private readonly INLogger _logger;

        private readonly ITKAPIService _tKAPIService;
        private HttpClientHelper _httpclient => _serviceProvider.GetRequiredService<HttpClientHelper>();
        private IRepository<AccountConfig> repositoryAccountConfig => _serviceProvider.GetRequiredService<IRepository<AccountConfig>>();
        private IRepository<TiktokVideos> _repositoryTiktokVideos => _serviceProvider.GetRequiredService<IRepository<TiktokVideos>>();
        private IRepository<TiktokVideosDaily> _repositoryTiktokVideosDaily => _serviceProvider.GetRequiredService<IRepository<TiktokVideosDaily>>();

        // 使用信号量控制并发数，设置为1确保串行处理
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1); // 改为最多1个并发

        public VideoJob(IServiceProvider serviceProvider, IOptions<TikhubSettings> tikTokSettings, INLogger logger, ITKAPIService tKAPIService)
        {
            _serviceProvider = serviceProvider;
            _tikTokSettings = tikTokSettings.Value;
            _logger = logger;
            _tKAPIService = tKAPIService;
        }

        /// <summary>
        /// 执行视频数据同步任务
        /// </summary>
        /// <param name="taskName"></param>
        /// <returns></returns>
        public async Task Execute(string taskName)
        {
            var stopwatch = Stopwatch.StartNew();
            int totalUsers = 0, successCount = 0, failedCount = 0, apiCalls = 0, totalVideos = 0;
            long? taskRunId = null;

            try
            {
                var activeConfigs = await repositoryAccountConfig.GetListAsync(x => x.IsActive);
                totalUsers = activeConfigs.Count;

                _logger.TaskMilestone(taskName, "数据获取完成", null, taskRunId, new Dictionary<string, object>
                {
                    ["UserCount"] = totalUsers
                });

                _logger.TaskInfo(taskName, $"开始处理 {totalUsers} 个用户的视频数据", null, taskRunId);

                // 直接循环处理每个用户，使用信号量确保一次只处理一个
                for (int i = 0; i < activeConfigs.Count; i++)
                {
                    var config = activeConfigs[i];
                    var userIndex = i + 1;
                    
                    Console.WriteLine($"准备处理第 {userIndex}/{totalUsers} 个用户的视频: {config.UniqueId}");

                    var result = await ProcessSingleUserVideosAsync(config);

                    apiCalls++;
                    totalVideos += result.VideoCount;
                    
                    if (result.Success)
                    {
                        successCount++;
                        Console.WriteLine($"✓ 用户 {config.UniqueId} 视频数据同步成功 ({userIndex}/{totalUsers})，获取 {result.VideoCount} 个视频");
                    }
                    else
                    {
                        failedCount++;
                        Console.WriteLine($"✗ 用户 {config.UniqueId} 视频数据同步失败 ({userIndex}/{totalUsers})");
                    }

                    // 每处理50%时记录里程碑
                    var progressPercent = userIndex * 100 / totalUsers;
                    if (progressPercent >= 50 && (userIndex - 1) * 100 / totalUsers < 50)
                    {
                        _logger.TaskMilestone(taskName, "处理进度50%", null, taskRunId, new Dictionary<string, object>
                        {
                            ["ProcessedCount"] = userIndex,
                            ["SuccessCount"] = successCount,
                            ["FailedCount"] = failedCount,
                            ["TotalVideos"] = totalVideos
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
                Console.WriteLine($"总视频数: {totalVideos}");
                Console.WriteLine($"成功率: {(totalUsers > 0 ? (successCount * 100.0 / totalUsers) : 0):F1}%");
                Console.WriteLine($"平均每用户视频数: {(successCount > 0 ? (totalVideos * 1.0 / successCount) : 0):F1}");
                Console.WriteLine($"=====================================\n");

                // 任务完成日志
                _logger.TaskComplete(taskName, stopwatch.ElapsedMilliseconds, totalUsers, successCount, failedCount, apiCalls, null, taskRunId);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.TaskError(taskName, ex, null, taskRunId);
                throw;
            }
        }

        /// <summary>
        /// 并发处理一批用户的视频数据
        /// </summary>
        private async Task<(int SuccessCount, int FailedCount, int ApiCalls, int VideoCount)> ProcessBatchAsync(
            List<AccountConfig> batch, int batchNumber)
        {
            var successCount = 0;
            var failureCount = 0;
            var apiCalls = 0;
            var videoCount = 0;
            var lockObject = new object();

            var tasks = batch.Select(async (config, index) =>
            {
                var userIndex = batchNumber * _tikTokSettings.BatchSize + index;
                Console.WriteLine($"准备处理第 {userIndex} 个用户的视频: {config.UniqueId}");

                var result = await ProcessSingleUserVideosAsync(config);

                lock (lockObject)
                {
                    apiCalls++;
                    videoCount += result.VideoCount;
                    if (result.Success)
                    {
                        successCount++;
                        Console.WriteLine($"✓ 用户 {config.UniqueId} 视频数据同步成功 ({userIndex})，获取 {result.VideoCount} 个视频");
                    }
                    else
                    {
                        failureCount++;
                        Console.WriteLine($"✗ 用户 {config.UniqueId} 视频数据同步失败 ({userIndex})");
                    }
                }
            });

            await Task.WhenAll(tasks);
            return (successCount, failureCount, apiCalls, videoCount);
        }

        /// <summary>
        /// 处理单个用户的视频数据
        /// </summary>
        private async Task<(bool Success, int VideoCount)> ProcessSingleUserVideosAsync(AccountConfig config)
        {
            // 使用信号量确保同时只有一个用户在处理
            await _semaphore.WaitAsync();
            try
            {
                var (apiResponse, errorMessage) = await _tKAPIService.FetchUserVideosWithRetryAsync(config.UniqueId, config.SecUid);

                if (apiResponse != null && apiResponse.Data?.AwemeList != null)
                {
                    // 使用SqlSugar官方推荐的事务处理方式
                    var result = await _repositoryTiktokVideos.ExecuteTransactionAsync(async () =>
                    {
                        var videoCount = 0;
                        foreach (var video in apiResponse.Data.AwemeList)
                        {
                            await UpdateTiktokVideosAsync(video);
                            await UpdateTiktokVideosDailyAsync(video);
                            videoCount++;
                        }
                        return videoCount;
                    });

                    _logger.TaskApiCall("VideoDataSync", config.UniqueId, true, null, null);
                    Console.WriteLine($"✓ 用户 {config.UniqueId} 事务提交成功，处理了 {result} 个视频");
                    return (true, result);
                }

                _logger.TaskApiCall("VideoDataSync", config.UniqueId, false, $"API_FAIL:{errorMessage}", null);
                return (false, 0);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"处理用户 {config.UniqueId} 视频时发生错误: {ex.Message}");
                _logger.TaskError($"VideoDataSync-{config.UniqueId}", ex, null, null);
                return (false, 0);
            }
            finally
            {
                _semaphore.Release();
            }
        } 
        /// <summary>
        /// 更新TiktokVideos表
        /// </summary>
        private async Task UpdateTiktokVideosAsync(VideoItem video)
        {
            try
            {
                var videoId = video.AwemeId; // 改为字符串
                var existingVideo = await _repositoryTiktokVideos.FirstOrDefaultAsync(x => x.Id == videoId);

                var tiktokVideo = new TiktokVideos
                {
                    Id = videoId,
                    UserId = video.Author.Uid,
                    UniqueId = video.Author.UniqueId,
                    Nickname = video.Author.Nickname,
                    Desc = video.Desc,
                    CreateTime = video.CreateTime,
                    Duration = video.Video?.Duration ?? 0,
                    Ratio = video.Video?.Ratio ?? "",
                    CoverUrl = video.Video?.Cover?.UrlListData?.FirstOrDefault() ?? "",
                    PlayUrl = video.Video?.PlayAddr?.UrlListData?.FirstOrDefault() ?? "",
                    MusicId = video.Music?.IdStr, // 直接使用字符串，不需要转换
                    MusicTitle = video.Music?.Title ?? "",
                    MusicAuthor = video.Music?.Author ?? "",
                    IsOriginalSound = video.Music?.IsOriginalSound ?? false,
                    Hashtags = video.ChaList != null ? JsonSerializer.Serialize(video.ChaList.Select(x => x.ChaName)) : null,
                    PoiName = video.PoiData?.PoiName,
                    Address = video.PoiData?.AddressInfo?.Address,
                    Latitude = video.PoiData?.AddressInfo?.Lat?.ToString(),
                    Longitude = video.PoiData?.AddressInfo?.Lng?.ToString(),
                    Region = video.Region ?? "",
                    PlayCount = video.Statistics?.PlayCount ?? 0,
                    DiggCount = video.Statistics?.DiggCount ?? 0,
                    CommentCount = video.Statistics?.CommentCount ?? 0,
                    DownloadCount = video.Statistics?.DownloadCount ?? 0,
                    ShareCount = video.Statistics?.ShareCount ?? 0,
                    AllowComment = video.Status?.AllowComment ?? true,
                    AllowShare = video.Status?.AllowShare ?? true,
                    AllowDownload = video.VideoControl?.AllowDownload ?? true,
                    AllowDuet = video.VideoControl?.AllowDuet ?? true,
                    AllowStitch = video.VideoControl?.AllowStitch ?? true,
                    CreatedByAi = video.AigcInfo?.CreatedByAi ?? false,
                    ShareUrl = video.ShareInfo?.ShareUrl ?? "",
                    // 新增字段赋值
                    Signature = video.Author?.Signature ?? "",
                    FollowerCount = video.Author?.FollowerCount ?? 0,
                    FollowingCount = video.Author?.FollowingCount ?? 0,
                    AwemeCount = video.Author?.AwemeCount ?? 0,
                    TotalFavorited = video.Author?.TotalFavorited ?? 0,
                    FavoritingCount = video.Author?.FavoritingCount ?? 0,
                    ContentDesc = video.ContentDesc ?? "",
                    IsAds = video.IsAds ?? false,
                    IsTop = video.IsTop ?? false,
                    ContentOriginalType = video.ContentOriginalType ?? 0,
                    MusicIsCommerceMusic = video.Music?.IsCommerceMusic ?? false,
                    ChaNames = video.ChaList != null ? JsonSerializer.Serialize(video.ChaList.Select(x => x.ChaName)) : null,
                    HashtagIds = video.ChaList != null ? JsonSerializer.Serialize(video.ChaList.Select(x => x.Cid)) : null,
                    CollectCount = video.Statistics?.CollectCount ?? 0,
                    ForwardCount = video.Statistics?.ForwardCount ?? 0,
                    WhatsappShareCount = video.Statistics?.WhatsappShareCount ?? 0,
                    AigcLabelType = video.AigcInfo?.AigcLabelType ?? 0,
                    MiscInfo = video.MiscInfo ?? "",
                    HasWatermark = video.Video?.HasWatermark ?? false,
                    PromoteIconText = video.PromoteIconText ?? "",
                    CoverLabels = video.CoverLabels != null ? JsonSerializer.Serialize(video.CoverLabels) : null,
                    UpdatedAt = DateTime.Now
                };
                #region 更新视频封面 CoverUrl - 使用专用智能下载方法
                try
                {
                    if (!string.IsNullOrEmpty(tiktokVideo.CoverUrl))
                    {
                        // 使用专用的视频封面智能下载方法
                        var localCoverUrlPath = await _httpclient.SmartDownloadVideoCoverAsync(
                            tiktokVideo.CoverUrl,
                            tiktokVideo.Id,
                            "uploads/video_coverurl");

                        // 更新实体中的封面路径
                        tiktokVideo.CoverUrl = AppSettingConifgHelper.ReadAppSettings("HOST_IP") + localCoverUrlPath;

                        Console.WriteLine($"用户 {tiktokVideo.UniqueId}，视频 {tiktokVideo.Id} 封面已保存: {localCoverUrlPath}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"用户 {tiktokVideo.UniqueId}，视频 {tiktokVideo.Id}，封面下载失败：{ex.Message}"); 
                    // 封面下载失败不影响主流程，使用原始URL
                    _logger.TaskError($"VideoCoverDownload-{tiktokVideo.Id}", ex, null, null);
                }
                #endregion

                // 数据库操作 - 确保在正确的事务上下文中
                if (existingVideo != null)
                { 
                    tiktokVideo.CreatedAt = existingVideo.CreatedAt;
                    // 使用当前事务上下文的仓储实例
                    await _repositoryTiktokVideos.UpdateAsync(tiktokVideo);
                }
                else
                {
                    await _repositoryTiktokVideos.InsertAsync(tiktokVideo);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"更新TiktokVideos失败: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 更新TiktokVideosDaily表
        /// </summary>
        private async Task UpdateTiktokVideosDailyAsync(VideoItem video)
        {
            try
            {
                var videoId = video.AwemeId;
                var today = DateTime.Today;

                var existingDaily = await _repositoryTiktokVideosDaily.FirstOrDefaultAsync(
                    x => x.Id == videoId && x.RecordDate == today);

                // 获取封面URL，优先使用已下载的本地路径
                var coverUrl = video.Video?.Cover?.UrlListData?.FirstOrDefault(); 
                var tiktokVideoDaily = new TiktokVideosDaily
                {
                    Id = videoId,
                    UserId = video.Author.Uid,
                    UniqueId = video.Author.UniqueId,
                    Nickname = video.Author.Nickname,
                    Desc = video.Desc,
                    CreateTime = video.CreateTime,
                    Duration = video.Video?.Duration,
                    CoverUrl = coverUrl, // 使用处理后的封面URL（可能是本地路径）
                    Hashtags = video.ChaList != null ? JsonSerializer.Serialize(video.ChaList.Select(x => x.ChaName)) : null,
                    Region = video.Region,
                    PlayCount = video.Statistics?.PlayCount ?? 0,
                    DiggCount = video.Statistics?.DiggCount ?? 0,
                    CommentCount = video.Statistics?.CommentCount ?? 0,
                    DownloadCount = video.Statistics?.DownloadCount ?? 0,
                    ShareCount = video.Statistics?.ShareCount ?? 0,
                    RecordDate = today,
                    UpdatedAt = DateTime.Now,
                    FollowerCount = video.Author?.FollowerCount ?? 0,
                    FollowingCount = video.Author?.FollowingCount ?? 0,
                    AwemeCount = video.Author?.AwemeCount ?? 0,
                    TotalFavorited = video.Author?.TotalFavorited ?? 0,
                    FavoritingCount = video.Author?.FavoritingCount ?? 0,
                    CollectCount = video.Statistics?.CollectCount ?? 0,
                    ForwardCount = video.Statistics?.ForwardCount ?? 0,
                    WhatsappShareCount = video.Statistics?.WhatsappShareCount ?? 0,
                    AllowComment = video.Status?.AllowComment ?? false,
                    AllowShare = video.Status?.AllowShare ?? false,
                    AllowDownload = video.VideoControl?.AllowDownload ?? false,
                    AllowDuet = video.VideoControl?.AllowDuet ?? false,
                    AllowStitch = video.VideoControl?.AllowStitch ?? false,
                    IsTop = video.IsTop ?? false,
                    IsAds = video.IsAds ?? false,
                    PromoteIconText = video.PromoteIconText,
                    ChaNames = video.ChaList != null ? JsonSerializer.Serialize(video.ChaList.Select(x => x.ChaName)) : null,
                    CoverLabels = video.CoverLabels != null ? JsonSerializer.Serialize(video.CoverLabels) : null,
                    AigcLabelType = video.AigcInfo?.AigcLabelType ?? 0,
                    CreatedByAi = video.AigcInfo?.CreatedByAi ?? false,
                    HasWatermark = video.Video?.HasWatermark ?? false
                };

                if (existingDaily != null)
                {
                    tiktokVideoDaily.CreatedAt = existingDaily.CreatedAt;
                    await _repositoryTiktokVideosDaily.UpdateAsync(tiktokVideoDaily);
                }
                else
                {
                    tiktokVideoDaily.CreatedAt = DateTime.Now;
                    await _repositoryTiktokVideosDaily.InsertAsync(tiktokVideoDaily);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"更新TiktokVideosDaily失败: {ex.Message}");
                throw;
            }
        }
    }
}
