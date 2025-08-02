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
        /// 授权认证
        /// </summary>
        [GroupInfo(Title = "系统功能", Description = "菜单权限相关接口", Version = "v1", Url = "https://auth.example.com/v1", UrlDescription = "认证服务")]
        Login,
    }
    /// <summary>
    /// 分组接口特性
    /// </summary>
    public class ApiGroupAttribute : Attribute, IApiDescriptionGroupNameProvider
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
