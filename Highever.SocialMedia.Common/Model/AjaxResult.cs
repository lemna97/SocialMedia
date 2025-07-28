namespace Highever.SocialMedia.Common
{
    /// <summary>
    /// 统一响应模型
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class AjaxResult<T> where T : class
    {
        /// <summary>
        /// 
        /// </summary>
        public AjaxResult()
        {
            Success = true;
            Msg = "";
            HttpCode = HttpCode.失败;
        }
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 状态码 
        /// </summary>
        public HttpCode HttpCode { get; set; }

        /// <summary>
        /// 返回消息
        /// </summary>
        public string Msg { get; set; }

        /// <summary>
        /// 返回数据
        /// </summary>
        public T Data { get; set; }
    }

}
