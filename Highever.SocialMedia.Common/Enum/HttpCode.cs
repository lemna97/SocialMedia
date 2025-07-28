using System.ComponentModel;

namespace Highever.SocialMedia.Common
{
    /// <summary>
    /// 状态码
    /// </summary>
    public enum HttpCode
    {
        /// <summary>
        /// 未登录 
        /// </summary>
        [Description("未登录")]
        未登录 = 0,
        /// <summary>
        /// 成功
        /// </summary>
        成功,
        /// <summary>
        /// 失败
        /// </summary>
        失败,
        /// <summary>
        /// 无权限
        /// </summary>
        无权限,
        /// <summary>
        /// 
        /// </summary>
        系统请求超时,
        /// <summary>
        /// 
        /// </summary>
        接口请求超时,
    }
}
