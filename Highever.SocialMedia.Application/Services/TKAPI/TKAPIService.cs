using Highever.SocialMedia.Application.Contracts;
using Highever.SocialMedia.Common;
using Highever.SocialMedia.Domain.Entity;
using Highever.SocialMedia.Domain.Repository;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Highever.SocialMedia.Application.Services.TK
{
    public class TKAPIService : ITKAPIService
    {
        private readonly HttpClientHelper _httpClient;
        private readonly TikhubSettings _tikTokSettings;
        private readonly RateLimitController _rateLimitController;

        private readonly IServiceProvider _serviceProvider;
        private IRepository<AccountConfig> repositoryAccountConfig => _serviceProvider.GetRequiredService<IRepository<AccountConfig>>();
        private IRepository<TiktokUsers> _repositoryTiktokUsers => _serviceProvider.GetRequiredService<IRepository<TiktokUsers>>();
        private HttpClientHelper _httpclient => _serviceProvider.GetRequiredService<HttpClientHelper>();
        public TKAPIService(HttpClientHelper httpClient, IOptions<TikhubSettings> tikTokSettings, IServiceProvider serviceProvider)
        {
            _httpClient = httpClient;
            _tikTokSettings = tikTokSettings.Value;
            _rateLimitController = new RateLimitController(_tikTokSettings);
            _serviceProvider = serviceProvider;
        }

        #region 用户
        /// <summary>
        /// 带重试机制的用户资料API调用
        /// </summary>
        public async Task<(TikTokApiResponse? Response, string? ErrorMessage)> FetchUserProfileWithRetryAsync(string uniqueId, string? secUid = null)
        {
            int retryCount = 0;
            string? lastErrorMessage = null;

            while (retryCount < _tikTokSettings.MaxRetryCount)
            {
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

                    var responseJson = await _httpClient.GetAsync(url, headers, TimeSpan.FromMinutes(_tikTokSettings.TimeoutMinutes));

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
                    else if (apiResponse?.Code == 429)
                    {
                        _rateLimitController.ReportError(isRateLimited: true);
                        lastErrorMessage = "API返回429限流错误";
                        Console.WriteLine($"{lastErrorMessage}，用户: {uniqueId}");
                    }
                    else
                    {
                        lastErrorMessage = $"API返回错误码: {apiResponse?.Code}";
                        Console.WriteLine($"{lastErrorMessage}，用户: {uniqueId}");
                    }
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
        /// 更新用户的数据
        /// </summary>
        /// <param name="apiResponse"></param>
        /// <returns></returns>
        public async Task UpdateTiktokUsersAsync(TikTokApiResponse apiResponse)
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

        #endregion

        #region 视频
        /// <summary>
        /// 带重试机制的用户视频API调用
        /// </summary>
        public async Task<(TikTokVideoApiResponse? Response, string? ErrorMessage)> FetchUserVideosWithRetryAsync(string uniqueId, string? secUid = null, string maxCursor = "0", int count = 50)
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
                        ["max_cursor"] = maxCursor,
                        ["count"] = count.ToString(),
                        ["sort_type"] = "0"
                    };

                    if (!string.IsNullOrEmpty(secUid))
                    {
                        queryParams["sec_user_id"] = secUid;
                    }
                    else
                    {
                        queryParams["unique_id"] = uniqueId;
                    }

                    var queryString = string.Join("&", queryParams.Select(kv => $"{kv.Key}={Uri.EscapeDataString(kv.Value)}"));
                    var fullUrl = $"{url}?{queryString}";

                    var headers = new Dictionary<string, string>
                    {
                        { "Authorization", _tikTokSettings.ApiToken },
                        { "User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36" }
                    };

                    var responseJson = await _httpClient.GetAsync(fullUrl, headers, TimeSpan.FromMinutes(_tikTokSettings.TimeoutMinutes));

                    if (string.IsNullOrWhiteSpace(responseJson))
                    {
                        lastErrorMessage = "API返回空响应";
                        throw new InvalidOperationException(lastErrorMessage);
                    }

                    var apiResponse = JsonSerializer.Deserialize<TikTokVideoApiResponse>(responseJson, JsonHelper.DefaultOptions);

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
                        lastErrorMessage = $"API返回错误码: {apiResponse?.Code}";
                    }
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
        #endregion


        #region 热门标签
        /// <summary>
        /// 带重试机制的热门标签API调用
        /// </summary>
        public async Task<(HotTagsApiResponse? Response, string? ErrorMessage)> FetchHotTagsWithRetryAsync(Dictionary<string, string> parameters)
        {
            int retryCount = 0;
            string? lastErrorMessage = null;
            var sortType = parameters.GetValueOrDefault("sort_by", "unknown");
            var page = parameters.GetValueOrDefault("page", "1");

            while (retryCount < _tikTokSettings.MaxRetryCount)
            {
                await _rateLimitController.WaitForPermissionAsync();

                try
                {
                    var url = "https://api.tikhub.io/api/v1/tiktok/ads/get_hashtag_list";
                    var queryParams = new Dictionary<string, string>(parameters);

                    var queryString = string.Join("&", queryParams.Select(kv => $"{kv.Key}={Uri.EscapeDataString(kv.Value)}"));
                    var fullUrl = $"{url}?{queryString}";

                    var headers = new Dictionary<string, string>
            {
                { "Authorization", _tikTokSettings.ApiToken },
                { "User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36" }
            };

                    Console.WriteLine($"调用热门标签API - 排序: {sortType}, 页码: {page}, URL: {fullUrl}");

                    var responseJson = await _httpClient.GetAsync(fullUrl, headers, TimeSpan.FromMinutes(_tikTokSettings.TimeoutMinutes));

                    if (string.IsNullOrWhiteSpace(responseJson))
                    {
                        lastErrorMessage = "API返回空响应";
                        throw new InvalidOperationException(lastErrorMessage);
                    }

                    var apiResponse = JsonSerializer.Deserialize<HotTagsApiResponse>(responseJson, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                    });

                    // 完整的响应状态检查
                    if (apiResponse?.Code == 200)
                    {
                        // 检查内部数据状态
                        if (apiResponse.Data?.Code == 0)
                        {
                            // 检查数据是否存在
                            if (apiResponse.Data?.Data?.List != null && apiResponse.Data.Data.List.Any())
                            {
                                _rateLimitController.ReportSuccess();
                                Console.WriteLine($"热门标签API调用成功 - 排序: {sortType}, 页码: {page}, 返回条数: {apiResponse.Data.Data.List.Count}");
                                return (apiResponse, null);
                            }
                            else
                            {
                                lastErrorMessage = "API返回数据为空";
                                Console.WriteLine($"{lastErrorMessage}，排序类型: {sortType}, 页码: {page}");
                            }
                        }
                        else if (apiResponse.Data?.Code == 40000)
                        {
                            // 参数验证错误，不需要重试
                            lastErrorMessage = $"API参数验证失败: {apiResponse.Data.Msg}";
                            Console.WriteLine($"{lastErrorMessage}，排序类型: {sortType}, 页码: {page}");
                            return (null, lastErrorMessage);
                        }
                        else
                        {
                            lastErrorMessage = $"API内部错误码: {apiResponse.Data?.Code}, 消息: {apiResponse.Data?.Msg}";
                            Console.WriteLine($"{lastErrorMessage}，排序类型: {sortType}, 页码: {page}");
                        }
                    }
                    else if (apiResponse?.Code == 429)
                    {
                        _rateLimitController.ReportError(isRateLimited: true);
                        lastErrorMessage = "API返回429限流错误";
                        Console.WriteLine($"{lastErrorMessage}，排序类型: {sortType}, 页码: {page}");
                    }
                    else
                    {
                        lastErrorMessage = $"API返回错误码: {apiResponse?.Code}";
                        Console.WriteLine($"{lastErrorMessage}，排序类型: {sortType}, 页码: {page}");
                    }
                }
                catch (HttpRequestException ex) when (ex.Message.Contains("429"))
                {
                    _rateLimitController.ReportError(isRateLimited: true);
                    lastErrorMessage = $"遇到限流: {ex.Message}";
                    Console.WriteLine($"热门标签 {sortType} 页码 {page} {lastErrorMessage}");
                }
                catch (Exception ex)
                {
                    _rateLimitController.ReportError();
                    lastErrorMessage = $"请求异常: {ex.Message}";
                    Console.WriteLine($"热门标签 {sortType} 页码 {page} {lastErrorMessage} (第 {retryCount + 1} 次)");
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
        #endregion
    }
}
