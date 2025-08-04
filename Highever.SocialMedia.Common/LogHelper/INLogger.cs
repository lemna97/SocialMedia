namespace Highever.SocialMedia.Common
{
    public interface INLogger: IScopedDependency
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

        #region Task服务专用日志
        /// <summary>
        /// 记录任务开始 - 关键日志，入库
        /// </summary>
        void TaskStart(string taskName, long? taskId = null, long? taskRunId = null);

        /// <summary>
        /// 记录任务完成 - 关键日志，入库
        /// </summary>
        void TaskComplete(string taskName, long executionTime, int userCount, int successCount, int failedCount, int apiCalls, long? taskId = null, long? taskRunId = null);

        /// <summary>
        /// 记录任务错误 - 关键日志，入库
        /// </summary>
        void TaskError(string taskName, Exception exception, long? taskId = null, long? taskRunId = null);

        /// <summary>
        /// 记录批次处理信息 - 详细日志，仅文件
        /// </summary>
        void TaskBatchInfo(string taskName, int batchNumber, int batchSize, int successCount, int failedCount, long? taskRunId = null);

        /// <summary>
        /// 记录API调用信息 - 详细日志，仅文件
        /// </summary>
        void TaskApiCall(string taskName, string uniqueId, bool success, string? errorMessage = null, long? taskRunId = null);

        /// <summary>
        /// 记录任务一般信息 - 详细日志，仅文件
        /// </summary>
        void TaskInfo(string taskName, string message, long? taskId = null, long? taskRunId = null);

        /// <summary>
        /// 记录任务里程碑信息 - 关键日志，入库
        /// </summary>
        void TaskMilestone(string taskName, string milestone, long? taskId = null, long? taskRunId = null, Dictionary<string, object>? properties = null);
        #endregion
    }
}
