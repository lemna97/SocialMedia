namespace Highever.SocialMedia.Common
{
    /// <summary>
    /// 
    /// </summary>
    public enum SwaggerApiGroup
    {
        /// <summary>
        /// 授权认证
        /// </summary>
        [GroupInfo(Title = "认证授权", Description = "授权相关接口", Version = "v1", Url = "https://auth.example.com/v1", UrlDescription = "认证服务")]
        Login,

        /// <summary>
        /// 全托管的接口
        /// </summary>
        [GroupInfo(Title = "全托管", Description = "自动化AI", Version = "v1", Url = "https://SocialMedia.example.com/v1", UrlDescription = "全托管服务")]
        FullSocialMedia,
        /// <summary>
        /// 全托管的接口
        /// </summary>
        [GroupInfo(Title = "AI招聘", Description = "AI招聘", Version = "v1", Url = "https://SocialMedia.example.com/v1", UrlDescription = "AI招聘")]
        AIRecruit
    }

}
