using Highever.SocialMedia.Common;
using Microsoft.AspNetCore.Mvc.ApiExplorer;

namespace Highever.SocialMedia.API
{
    /// <summary>
    /// 
    /// </summary>
    public enum SwaggerApiGroup
    {
        /// <summary>
        /// 系统相关
        /// </summary>
        [GroupInfo(Title = "系统设置", Description = "系统设置", Version = "v1", Url = "https://auth.example.com/v1", UrlDescription = "系统设置")]
        System,
        /// <summary>
        /// 用户相关
        /// </summary>
        [GroupInfo(Title = "用户管理", Description = "用户相关接口", Version = "v1", Url = "https://auth.example.com/v1", UrlDescription = "用户相关接口")]
        User,
        /// <summary>
        /// 视频相关
        /// </summary>
        [GroupInfo(Title = "音频管理", Description = "音频相关接口", Version = "v1", Url = "https://auth.example.com/v1", UrlDescription = "音频相关接口")]
        Video,
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
