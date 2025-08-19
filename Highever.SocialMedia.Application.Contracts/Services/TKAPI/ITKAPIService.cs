using Highever.SocialMedia.Common;

namespace Highever.SocialMedia.Application.Contracts
{
    /// <summary>
    /// 
    /// </summary>
    public interface ITKAPIService : ITransientDependency
    {
        /// <summary>
        /// 带重试机制的用户资料API调用
        /// </summary>
        /// <param name="uniqueId">用户唯一ID</param>
        /// <param name="secUid">安全用户ID（可选）</param>
        /// <returns>API响应和错误信息</returns>
        Task<(TikTokApiResponse? Response, string? ErrorMessage)> FetchUserProfileWithRetryAsync(string uniqueId, string? secUid = null);
        /// <summary>
        /// 更新用户数据
        /// </summary>
        /// <param name="apiResponse"></param>
        /// <returns></returns>
        Task UpdateTiktokUsersAsync(TikTokApiResponse apiResponse);

        /// <summary>
        /// 带重试机制的用户视频API调用
        /// </summary>
        /// <param name="uniqueId">用户唯一ID</param>
        /// <param name="secUid">安全用户ID（可选）</param>
        /// <param name="maxCursor">分页游标</param>
        /// <param name="count">获取数量</param>
        /// <returns>视频API响应和错误信息</returns>
        Task<(TikTokVideoApiResponse? Response, string? ErrorMessage)> FetchUserVideosWithRetryAsync(string uniqueId, string? secUid = null, string maxCursor = "0", int count = 50);

        /// <summary>
        /// 带重试机制的热门标签API调用
        /// </summary>
        /// <param name="parameters">API请求参数</param>
        /// <returns>API响应结果和错误信息</returns>
        Task<(HotTagsApiResponse? Response, string? ErrorMessage)> FetchHotTagsWithRetryAsync(Dictionary<string, string> parameters);
    }
}
