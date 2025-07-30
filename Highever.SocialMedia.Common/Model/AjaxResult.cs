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
            success = true;
            msg = "";
            httpCode = HttpCode.成功;
        }  

        /// <summary>
        /// 是否成功
        /// </summary>
        public bool success { get; set; }

        /// <summary>
        /// 状态码 
        /// </summary>
        public HttpCode httpCode { get; set; }

        /// <summary>
        /// 返回消息
        /// </summary>
        public string msg { get; set; }

        /// <summary>
        /// 返回数据
        /// </summary>
        public T data { get; set; }

        /// <summary>
        /// 成功响应
        /// </summary>
        public static AjaxResult<T> Success(T data = null, string message = "操作成功")
        {
            return new AjaxResult<T>
            {
                success = true,
                httpCode = HttpCode.成功,
                msg = message,
                data = data
            };
        }

        /// <summary>
        /// 失败响应
        /// </summary>
        public static AjaxResult<T> Fail(string message = "操作失败", HttpCode httpCode = HttpCode.失败)
        {
            return new AjaxResult<T>
            {
                success = false,
                httpCode = httpCode,
                msg = message,
                data = null
            };
        }

        /// <summary>
        /// 分页响应
        /// </summary>
        public static AjaxResult<PageResult<TItem>> Page<TItem>(List<TItem> items, int total, string message = "查询成功")
        {
            return new AjaxResult<PageResult<TItem>>
            {
                success = true,
                httpCode = HttpCode.成功,
                msg = message,
                data = new PageResult<TItem> { Items = items, Total = total }
            };
        }
    }
}
