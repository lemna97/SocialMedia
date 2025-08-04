using Hangfire;
using Highever.SocialMedia.Common;
using Highever.SocialMedia.Domain.Entity;
using Highever.SocialMedia.Domain.Repository;
using Microsoft.Extensions.Options;
using System.Text.Json;
using Highever.SocialMedia.Admin.TaskService.Models;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Highever.SocialMedia.Admin.TaskService
{
    public class VideoJob : ITaskExecutor
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly TikhubSettings _tikTokSettings;
        private readonly RateLimitController _rateLimitController;
        private readonly INLogger _logger;

        private HttpClientHelper _httpclient => _serviceProvider.GetRequiredService<HttpClientHelper>();
        private IRepository<AccountConfig> repositoryAccountConfig => _serviceProvider.GetRequiredService<IRepository<AccountConfig>>();
        private IRepository<TiktokVideos> _repositoryTiktokVideos => _serviceProvider.GetRequiredService<IRepository<TiktokVideos>>();
        private IRepository<TiktokVideosDaily> _repositoryTiktokVideosDaily => _serviceProvider.GetRequiredService<IRepository<TiktokVideosDaily>>();

        public VideoJob(IServiceProvider serviceProvider, IOptions<TikhubSettings> tikTokSettings, INLogger logger)
        {
            _serviceProvider = serviceProvider;
            _tikTokSettings = tikTokSettings.Value;
            _rateLimitController = new RateLimitController(_tikTokSettings);
            _logger = logger;
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

                // 分批处理用户
                for (int batchStart = 0; batchStart < activeConfigs.Count; batchStart += _tikTokSettings.BatchSize)
                {
                    var batch = activeConfigs.Skip(batchStart).Take(_tikTokSettings.BatchSize).ToList();
                    var batchNumber = batchStart / _tikTokSettings.BatchSize + 1;

                    var batchResults = await ProcessBatchAsync(batch, batchNumber);

                    successCount += batchResults.SuccessCount;
                    failedCount += batchResults.FailedCount;
                    apiCalls += batchResults.ApiCalls;
                    totalVideos += batchResults.VideoCount;

                    _logger.TaskBatchInfo(taskName, batchNumber, batch.Count, batchResults.SuccessCount, batchResults.FailedCount, taskRunId);

                    // 每处理50%时记录里程碑
                    var progressPercent = (batchStart + batch.Count) * 100 / totalUsers;
                    if (progressPercent >= 50 && batchStart * 100 / totalUsers < 50)
                    {
                        _logger.TaskMilestone(taskName, "处理进度50%", null, taskRunId, new Dictionary<string, object>
                        {
                            ["ProcessedCount"] = batchStart + batch.Count,
                            ["SuccessCount"] = successCount,
                            ["FailedCount"] = failedCount,
                            ["TotalVideos"] = totalVideos
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
                Console.WriteLine($"总视频数: {totalVideos}");
                Console.WriteLine($"成功率: {(totalUsers > 0 ? (successCount * 100.0 / totalUsers):0):F1}%");
                Console.WriteLine($"平均每用户视频数: {(successCount > 0 ? (totalVideos * 1.0 / successCount):0):F1}");
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
            try
            {
                var (apiResponse, errorMessage) = await FetchUserVideosWithRetryAsync(config.UniqueId, config.SecUid);

                if (apiResponse != null && apiResponse.Data?.AwemeList != null)
                {
                    try
                    {
                        var videoCount = 0;
                        foreach (var video in apiResponse.Data.AwemeList)
                        {
                            await UpdateTiktokVideosAsync(video);
                            await UpdateTiktokVideosDailyAsync(video);
                            videoCount++;
                        }

                        _logger.TaskApiCall("VideoDataSync", config.UniqueId, true, null, null);
                        return (true, videoCount);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"数据库操作失败，用户: {config.UniqueId}, 错误: {ex.Message}");
                        _logger.TaskApiCall("VideoDataSync", config.UniqueId, false, $"EXCEPTION:数据库操作失败: {ex.Message}", null);
                        _logger.TaskError($"VideoDataSync-{config.UniqueId}", ex, null, null);
                        return (false, 0);
                    }
                }

                _logger.TaskApiCall("VideoDataSync", config.UniqueId, false, $"API_FAIL:{errorMessage}", null);
                _logger.TaskMilestone("VideoDataSync", $"用户失败: {config.UniqueId}", null, null, new Dictionary<string, object>
                {
                    ["FailedUser"] = config.UniqueId,
                    ["ErrorMessage"] = errorMessage ?? "未知错误"
                });

                return (false, 0);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"处理用户 {config.UniqueId} 视频时发生错误: {ex.Message}");
                _logger.TaskError($"VideoDataSync-{config.UniqueId}", ex, null, null);
                return (false, 0);
            }
        }

        /// <summary>
        /// 带重试机制的视频API调用
        /// </summary>
        private async Task<(TikTokVideoApiResponse? Response, string? ErrorMessage)> FetchUserVideosWithRetryAsync(string uniqueId, string? secUid = null)
        {
            int retryCount = 0;
            string? lastErrorMessage = null;

            while (retryCount < _tikTokSettings.MaxRetryCount)
            {
                await _rateLimitController.WaitForPermissionAsync();

                try
                {
                    var url = "https://api.tikhub.io/api/v1/tiktok/app/v3/fetch_user_post_videos";
                    var queryParams = new Dictionary<string, string>
                    {
                        ["max_cursor"] = "0",
                        ["count"] = "50",
                        ["sort_type"] = "0"
                    };

                    // 优先使用 sec_user_id
                    if (!string.IsNullOrEmpty(secUid))
                    {
                        queryParams["sec_user_id"] = secUid;
                    }
                    else
                    {
                        queryParams["unique_id"] = uniqueId;
                    }

                    // 构建完整URL
                    var queryString = string.Join("&", queryParams.Select(kv => $"{kv.Key}={Uri.EscapeDataString(kv.Value)}"));
                    var fullUrl = $"{url}?{queryString}";

                    var headers = new Dictionary<string, string>
                    {
                        { "Authorization", _tikTokSettings.ApiToken },
                        { "User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36" }
                    };

                    var responseJson = await _httpclient.GetAsync(fullUrl, headers, TimeSpan.FromMinutes(_tikTokSettings.TimeoutMinutes));

                    if (string.IsNullOrWhiteSpace(responseJson))
                    {
                        lastErrorMessage = "API返回空响应";
                        throw new InvalidOperationException(lastErrorMessage);
                    }

                    var apiResponse = JsonSerializer.Deserialize<TikTokVideoApiResponse>(responseJson, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                    });

                    if (apiResponse?.Code == 200 && apiResponse.Data?.AwemeList != null)
                    {
                        _rateLimitController.ReportSuccess();
                        return (apiResponse, null);
                    }
                    else if (apiResponse?.Code == 429)
                    {
                        _rateLimitController.ReportError(isRateLimited: true);
                        lastErrorMessage = "API返回429限流错误";
                        Console.WriteLine($"{lastErrorMessage}，用户: {uniqueId}");
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
        /// 更新TiktokVideos表
        /// </summary>
        private async Task UpdateTiktokVideosAsync(VideoItem video)
        {
            try
            {
                var videoId = long.Parse(video.AwemeId);
                var existingVideo = await _repositoryTiktokVideos.FirstOrDefaultAsync(x => x.Id == videoId);

                var tiktokVideo = new TiktokVideos
                {
                    Id = videoId,
                    UserId = long.Parse(video.Author.Uid),
                    UniqueId = video.Author.UniqueId,
                    Nickname = video.Author.Nickname,
                    Desc = video.Desc,
                    CreateTime = video.CreateTime,
                    Duration = video.Video?.Duration ?? 0,
                    Ratio = video.Video?.Ratio ?? "",
                    CoverUrl = video.Video?.Cover?.UrlListData?.FirstOrDefault() ?? "",
                    PlayUrl = video.Video?.PlayAddr?.UrlListData?.FirstOrDefault() ?? "",
                    MusicId = string.IsNullOrEmpty(video.Music?.IdStr) ? null : long.Parse(video.Music.IdStr),
                    MusicTitle = video.Music?.Title ?? "",
                    MusicAuthor = video.Music?.Author ?? "",
                    IsOriginalSound = video.Music?.IsOriginalSound ?? false,
                    Hashtags = video.ChaList != null ? JsonSerializer.Serialize(video.ChaList.Select(x => x.ChaName)) : null,
                    PoiName = video.PoiData?.PoiName,
                    Address = video.PoiData?.AddressInfo?.Address,
                    Latitude = video.PoiData?.AddressInfo?.Lat,
                    Longitude = video.PoiData?.AddressInfo?.Lng,
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

                if (existingVideo != null)
                {
                    tiktokVideo.CreatedAt = existingVideo.CreatedAt;
                    await _repositoryTiktokVideos.UpdateAsync(tiktokVideo);
                }
                else
                {
                    tiktokVideo.CreatedAt = DateTime.Now;
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
                var videoId = long.Parse(video.AwemeId);
                var today = DateTime.Today;

                var existingDaily = await _repositoryTiktokVideosDaily.FirstOrDefaultAsync(
                    x => x.Id == videoId && x.RecordDate == today);

                var tiktokVideoDaily = new TiktokVideosDaily
                {
                    Id = videoId,
                    UserId = long.Parse(video.Author.Uid),
                    UniqueId = video.Author.UniqueId,
                    Nickname = video.Author.Nickname,
                    Desc = video.Desc,
                    CreateTime = video.CreateTime,
                    PlayCount = video.Statistics?.PlayCount ?? 0,
                    DiggCount = video.Statistics?.DiggCount ?? 0,
                    CommentCount = video.Statistics?.CommentCount ?? 0,
                    DownloadCount = video.Statistics?.DownloadCount ?? 0,
                    ShareCount = video.Statistics?.ShareCount ?? 0,
                    RecordDate = today,
                    UpdatedAt = DateTime.Now
                };

                if (existingDaily != null)
                {
                    tiktokVideoDaily.CreatedAt = existingDaily.CreatedAt;
                    await _repositoryTiktokVideosDaily.UpdateAsync(tiktokVideoDaily);
                    Console.WriteLine($"更新视频 {video.AwemeId} 今日的TiktokVideosDaily记录");
                }
                else
                {
                    tiktokVideoDaily.CreatedAt = DateTime.Now;
                    await _repositoryTiktokVideosDaily.InsertAsync(tiktokVideoDaily);
                    Console.WriteLine($"插入视频 {video.AwemeId} 今日的TiktokVideosDaily记录");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"更新TiktokVideosDaily表时发生错误: {ex.Message}");
                throw;
            }
        }
    }
}
