using Highever.SocialMedia.Common;
using Microsoft.AspNetCore.Mvc.ApiExplorer;

namespace Highever.SocialMedia.Admin
{
    /// <summary>
    /// 
    /// </summary>
    public enum SwaggerApiGroup
    {
        /// <summary>
        /// 授权认证
        /// </summary>
        [GroupInfo(Title = "媒体项目", Description = "媒体项目", Version = "v1", Url = "https://auth.example.com/v1", UrlDescription = "媒体项目")]
        Media,
        /// <summary>
        /// 测试接口
        /// </summary>
        [GroupInfo(Title = "测试接口", Description = "测试接口接口", Version = "v1", Url = "https://auth.example.com/v1", UrlDescription = "测试接口")]
        Test, 
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
    /// <summary>
    /// 分组接口特性
    /// </summary>
    public class ApiGroupAttribute : System.Attribute, IApiDescriptionGroupNameProvider
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        public ApiGroupAttribute(SwaggerApiGroup name)
        {
            GroupName = name.ToString();
        }
        /// <summary>
        /// 分组名称
        /// </summary>
        public string GroupName { get; set; }
    }
}
