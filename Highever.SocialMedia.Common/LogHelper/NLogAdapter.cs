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
        private readonly Logger _taskCriticalLogger; // 关键任务日志 - 入库
        private readonly Logger _taskDetailLogger;   // 详细任务日志 - 仅文件

        public NLogAdapter()
        {
            _infoLogger = LogManager.GetLogger("InfoLogger");
            _errorLogger = LogManager.GetLogger("ErrorLogger");
            _debugLogger = LogManager.GetLogger("DebugLogger");
            _apiLogger = LogManager.GetLogger("ApiLogger");
            _databaseLogger = LogManager.GetLogger("DatabaseLogger");
            _taskCriticalLogger = LogManager.GetLogger("TaskService.Critical"); // 关键日志入库
            _taskDetailLogger = LogManager.GetLogger("TaskService.Detail");     // 详细日志仅文件
        }

        #region Task服务专用日志方法
        /// <summary>
        /// 记录任务开始 - 关键日志，入库
        /// </summary>
        public void TaskStart(string taskName, long? taskId = null, long? taskRunId = null)
        {
            var logEvent = new LogEventInfo(LogLevel.Info, _taskCriticalLogger.Name, "任务开始执行");
            logEvent.Properties["TaskName"] = taskName;
            logEvent.Properties["TaskId"] = taskId;
            logEvent.Properties["TaskRunId"] = taskRunId;
            _taskCriticalLogger.Log(logEvent);
        }

        /// <summary>
        /// 记录任务完成 - 关键日志，入库
        /// </summary>
        public void TaskComplete(string taskName, long executionTime, int userCount, int successCount, int failedCount, int apiCalls, long? taskId = null, long? taskRunId = null)
        {
            var logEvent = new LogEventInfo(LogLevel.Info, _taskCriticalLogger.Name, 
                $"任务执行完成 - 耗时:{executionTime}ms, 用户:{userCount}, 成功:{successCount}, 失败:{failedCount}, API调用:{apiCalls}");
            logEvent.Properties["TaskName"] = taskName;
            logEvent.Properties["TaskId"] = taskId;
            logEvent.Properties["TaskRunId"] = taskRunId;
            logEvent.Properties["ExecutionTime"] = executionTime;
            logEvent.Properties["UserCount"] = userCount;
            logEvent.Properties["SuccessCount"] = successCount;
            logEvent.Properties["FailedCount"] = failedCount;
            logEvent.Properties["ApiCalls"] = apiCalls;
            _taskCriticalLogger.Log(logEvent);
        }

        /// <summary>
        /// 记录任务错误 - 关键日志，入库
        /// </summary>
        public void TaskError(string taskName, Exception exception, long? taskId = null, long? taskRunId = null)
        {
            var logEvent = new LogEventInfo(LogLevel.Error, _taskCriticalLogger.Name, "任务执行发生错误");
            logEvent.Exception = exception;
            logEvent.Properties["TaskName"] = taskName;
            logEvent.Properties["TaskId"] = taskId;
            logEvent.Properties["TaskRunId"] = taskRunId;
            _taskCriticalLogger.Log(logEvent);
        }

        /// <summary>
        /// 记录批次处理信息 - 详细日志，仅文件
        /// </summary>
        public void TaskBatchInfo(string taskName, int batchNumber, int batchSize, int successCount, int failedCount, long? taskRunId = null)
        {
            var logEvent = new LogEventInfo(LogLevel.Info, _taskDetailLogger.Name, 
                $"批次 {batchNumber} 处理完成，大小: {batchSize}, 成功: {successCount}, 失败: {failedCount}");
            logEvent.Properties["TaskName"] = taskName;
            logEvent.Properties["TaskRunId"] = taskRunId;
            logEvent.Properties["UserCount"] = batchSize;
            logEvent.Properties["SuccessCount"] = successCount;
            logEvent.Properties["FailedCount"] = failedCount;
            _taskDetailLogger.Log(logEvent);
        }

        /// <summary>
        /// 记录API调用信息 - 详细日志，仅文件
        /// </summary>
        public void TaskApiCall(string taskName, string uniqueId, bool success, string? errorMessage = null, long? taskRunId = null)
        {
            var level = success ? LogLevel.Debug : LogLevel.Warn;
            var message = success ? $"API调用成功: {uniqueId}" : $"API调用失败: {uniqueId}, 错误: {errorMessage}";
            
            var logEvent = new LogEventInfo(level, _taskDetailLogger.Name, message);
            logEvent.Properties["TaskName"] = taskName;
            logEvent.Properties["TaskRunId"] = taskRunId;
            logEvent.Properties["ApiCalls"] = 1;
            _taskDetailLogger.Log(logEvent);
        }

        /// <summary>
        /// 记录任务一般信息 - 详细日志，仅文件
        /// </summary>
        public void TaskInfo(string taskName, string message, long? taskId = null, long? taskRunId = null)
        {
            var logEvent = new LogEventInfo(LogLevel.Info, _taskDetailLogger.Name, message);
            logEvent.Properties["TaskName"] = taskName;
            logEvent.Properties["TaskId"] = taskId;
            logEvent.Properties["TaskRunId"] = taskRunId;
            _taskDetailLogger.Log(logEvent);
        }

        /// <summary>
        /// 记录任务里程碑信息 - 关键日志，入库
        /// </summary>
        public void TaskMilestone(string taskName, string milestone, long? taskId = null, long? taskRunId = null, Dictionary<string, object>? properties = null)
        {
            var logEvent = new LogEventInfo(LogLevel.Info, _taskCriticalLogger.Name, $"任务里程碑: {milestone}");
            logEvent.Properties["TaskName"] = taskName;
            logEvent.Properties["TaskId"] = taskId;
            logEvent.Properties["TaskRunId"] = taskRunId;
            
            if (properties != null)
            {
                foreach (var prop in properties)
                {
                    logEvent.Properties[prop.Key] = prop.Value;
                }
            }
            
            _taskCriticalLogger.Log(logEvent);
        }
        #endregion

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
