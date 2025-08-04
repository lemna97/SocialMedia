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
        [GroupInfo(Title = "系统功能", Description = "系统功能", Version = "系统功能", Url = "https://auth.example.com/v1", UrlDescription = "系统设置")]
        系统功能,  
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
