using Microsoft.AspNetCore.Mvc.ApiExplorer;

namespace Highever.SocialMedia.Common
{
    /// <summary>
    /// 分组信息特性
    /// </summary>
    public class GroupInfoAttribute : System.Attribute
    {
        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// 版本
        /// </summary>
        public string Version { get; set; }
        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// 地址
        /// </summary>
        public string Url { get; set; }
        /// <summary>
        /// 地址描述
        /// </summary>
        public string UrlDescription { get; set; }
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
