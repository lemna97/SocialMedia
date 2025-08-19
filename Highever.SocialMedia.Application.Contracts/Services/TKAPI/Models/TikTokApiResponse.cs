namespace Highever.SocialMedia.Application.Contracts
{
    /// <summary>
    /// TikTok API响应数据模型
    /// </summary>
    public class TikTokApiResponse
    {
        public int Code { get; set; }
        public string Router { get; set; }
        public TikTokApiParams Params { get; set; }
        public TikTokApiData Data { get; set; }
    }

    public class TikTokApiParams
    {
        public string UniqueId { get; set; }
        public string SecUid { get; set; }
    }

    public class TikTokApiData
    {
        public TikTokUserInfo UserInfo { get; set; }
    }

    public class TikTokUserInfo
    {
        public TikTokUser User { get; set; }
        public TikTokStats Stats { get; set; }
    }

    public class TikTokUser
    {
        public string Id { get; set; }
        public string UniqueId { get; set; }
        public string Nickname { get; set; }
        public string AvatarLarger { get; set; }
        public string AvatarMedium { get; set; }
        public string AvatarThumb { get; set; }
        public string Signature { get; set; }
        public long CreateTime { get; set; }
        public bool Verified { get; set; }
        public string SecUid { get; set; }
        public bool PrivateAccount { get; set; }
        public string Language { get; set; }
    }

    public class TikTokStats
    {
        public int FollowerCount { get; set; }
        public int FollowingCount { get; set; }
        public int Heart { get; set; }
        public int HeartCount { get; set; }
        public int VideoCount { get; set; }
        public int DiggCount { get; set; }
        public int FriendCount { get; set; }
    }
}