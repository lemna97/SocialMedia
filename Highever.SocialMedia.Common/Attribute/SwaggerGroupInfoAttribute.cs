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
}
