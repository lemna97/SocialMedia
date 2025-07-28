namespace Highever.SocialMedia.Common
{
    public interface INLogger
    {
        #region 文件
        /// <summary>
        /// 异常记录到文件
        /// </summary>
        /// <param name="message"></param>
        public void Error(string message);
        /// <summary>
        /// 异常记录到文件
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public string Error(Exception exception, string message = "");
        /// <summary>
        /// 跟踪日志
        /// </summary>
        /// <param name="message"></param>
        public void Info(string message);
        /// <summary>
        /// API跟踪日志
        /// </summary>
        /// <param name="message"></param>
        public void ApiInfo(string message);
        /// <summary>
        /// API异常日志
        /// </summary>
        /// <param name="message"></param>
        public void ApiError(string message);
        /// <summary>
        /// API异常日志
        /// </summary>
        /// <param name="e"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public string ApiError(Exception e, string message = "");

        /// <summary>
        /// 调试日志
        /// </summary>
        /// <param name="message"></param>
        public void DebugError(string message);
        /// <summary>
        /// 调试日志
        /// </summary>
        /// <param name="message"></param>
        public void DebugInfo(string message);
        #endregion

        #region 数据库
        /// <summary>
        /// 异常跟踪记录到数据库
        /// </summary>
        /// <param name="message"></param>
        public void DateBaseError(string message);
        /// <summary>
        /// 异常记录到数据库
        /// </summary>
        /// <param name="e"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public string DateBaseErrorByCode(Exception e, string message);
        #endregion

    }
}
