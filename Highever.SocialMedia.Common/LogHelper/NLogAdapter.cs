using NLog;

namespace Highever.SocialMedia.Common
{
    public class NLogAdapter : INLogger
    {
        private readonly Logger _infoLogger;
        private readonly Logger _errorLogger;
        private readonly Logger _debugLogger;
        private readonly Logger _apiLogger;
        private readonly Logger _databaseLogger;

        public NLogAdapter()
        {
            _infoLogger = LogManager.GetLogger("InfoLogger");
            _errorLogger = LogManager.GetLogger("ErrorLogger");
            _debugLogger = LogManager.GetLogger("DebugLogger");
            _apiLogger = LogManager.GetLogger("ApiLogger");
            _databaseLogger = LogManager.GetLogger("DatabaseLogger");

        }
        /// <summary>
        /// 异常记录到数据库（code）
        /// </summary>
        /// <param name="e"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public string DateBaseErrorByCode(Exception e, string message)
        {
            string errorCode = string.Empty;
            _databaseLogger.Error(PackageErrorMsg(e, ref errorCode, message));
            return string.IsNullOrEmpty(errorCode) ? errorCode : $"DATABASE错误编码：{errorCode}";
        }
        /// <summary>
        /// 异常记录到数据库
        /// </summary>
        /// <param name="message"></param>
        public void DateBaseError(string message)
        {
            _databaseLogger.Error(message);
        }
        /// <summary>
        /// 异常记录到文件
        /// </summary>
        /// <param name="message"></param>
        public void Error(string message)
        {
            _errorLogger.Error(message);
        }
        /// <summary>
        /// 异常记录到文件
        /// </summary>
        /// <param name="e"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public string Error(Exception e, string message = "")
        {
            string errorCode = string.Empty;
            _errorLogger.Error(PackageErrorMsg(e, ref errorCode, message));
            return string.IsNullOrEmpty(errorCode) ? errorCode : $"FILE错误编码：{errorCode}";
        }
        /// <summary>
        /// 组装Error格式
        /// </summary>
        /// <param name="e"></param>
        /// <param name="errorCode"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        private string PackageErrorMsg(Exception e, ref string errorCode, string message = "")
        {
            var Fac = StringExtension.Fix<Exception, string>(f => x => x.InnerException == null ? x.StackTrace : f(x.InnerException) + "\r\n" + x.StackTrace);
            var stackTrace = Fac(e);
            string s = Guid.NewGuid().ToString().Replace("-", "");
            s = s.Substring(0, 10 > s.Length ? s.Length : 10);
            errorCode = DateTime.Now.ToString("yyyyMMddHHmmssff") + "X" + s;
            string messageStr = "";
            if (!string.IsNullOrEmpty(message))
            {
                messageStr = "\r\nMessage2:" + message;
            }
            string errstr = "日志时间:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "\r\n"
                            + "错误编号:" + errorCode + "\r\n"
                            + "Message:{0}\r\n"
                            + "Type:{1}\r\n"
                            + "Source:{2}\r\n"
                            + "StackTrace:{3}\r\n"
                            + "Method:{4}\r\n"
                            + "Class:{5}\r\n";
            string error = string.Format(errstr
                                        , e.Message + messageStr, e.GetType().ToString(), e.Source, stackTrace, e.TargetSite == null ? "" : e.TargetSite.Name, e.TargetSite == null ? "" : e.TargetSite.DeclaringType == null ? "" : e.TargetSite.DeclaringType.FullName);
            return error;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public void Info(string message)
        {
            _infoLogger.Info(message);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public void ApiInfo(string message)
        {
            _apiLogger.Info(message);
        }
        public void ApiError(string message)
        {
            _apiLogger.Info(message);
        }
        public string ApiError(Exception e, string message = "")
        {
            string errorCode = string.Empty;
            _apiLogger.Error(PackageErrorMsg(e, ref errorCode, message));
            return errorCode;
        }
        public void DebugInfo(string message)
        {
            _debugLogger.Info(message);
        }
        public void DebugError(string message)
        {
            _debugLogger.Error(message);
        }
    }
}
