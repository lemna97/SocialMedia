using Hangfire;
using Hangfire.Storage;
using Microsoft.AspNetCore.Mvc;
using Highever.SocialMedia.Common;

namespace Highever.SocialMedia.Admin.Controllers
{
    /// <summary>
    /// Hangfire 管理
    /// </summary>
    [ApiController]
    [ApiGroup(SwaggerApiGroup.Media)]
    [Route("api/hangfire")]
    public class HangfireManagementController : Controller
    {
        private readonly IRecurringJobManager _recurringJobManager;
        private readonly INLogger _logger;

        public HangfireManagementController(
            IRecurringJobManager recurringJobManager,
            INLogger logger)
        {
            _recurringJobManager = recurringJobManager;
            _logger = logger;
        }

        /// <summary>
        /// 清理所有队列任务
        /// </summary>
        [HttpPost("clearAllJobs")]
        public IActionResult ClearAllJobs()
        {
            try
            {
                using var connection = JobStorage.Current.GetConnection();
                var monitoring = JobStorage.Current.GetMonitoringApi();

                // 清理所有队列中的任务
                var queues = monitoring.Queues();
                foreach (var queue in queues)
                {
                    var enqueuedJobs = monitoring.EnqueuedJobs(queue.Name, 0, int.MaxValue);
                    foreach (var job in enqueuedJobs)
                    {
                        BackgroundJob.Delete(job.Key);
                    }
                }

                // 清理处理中的任务
                var processingJobs = monitoring.ProcessingJobs(0, int.MaxValue);
                foreach (var job in processingJobs)
                {
                    BackgroundJob.Delete(job.Key);
                }

                // 清理计划任务
                var scheduledJobs = monitoring.ScheduledJobs(0, int.MaxValue);
                foreach (var job in scheduledJobs)
                {
                    BackgroundJob.Delete(job.Key);
                }

                // 清理失败任务
                var failedJobs = monitoring.FailedJobs(0, int.MaxValue);
                foreach (var job in failedJobs)
                {
                    BackgroundJob.Delete(job.Key);
                }

                _logger.Info("已清理所有 Hangfire 队列任务");
                return this.Success("所有队列任务已清理完成");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "清理 Hangfire 队列任务失败");
                return this.JsonError("清理失败: " + ex.Message);
            }
        }

        /// <summary>
        /// 清理所有循环任务
        /// </summary>
        [HttpPost("clearRecurringJobs")]
        public IActionResult ClearRecurringJobs()
        {
            try
            {
                using var connection = JobStorage.Current.GetConnection();
                var recurringJobs = connection.GetRecurringJobs();

                foreach (var job in recurringJobs)
                {
                    _recurringJobManager.RemoveIfExists(job.Id);
                }

                _logger.Info($"已清理 {recurringJobs.Count} 个循环任务");
                return this.Success($"已清理 {recurringJobs.Count} 个循环任务");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "清理循环任务失败");
                return this.JsonError("清理失败: " + ex.Message);
            }
        }

        /// <summary>
        /// 完全重置 Hangfire（清理所有数据）
        /// </summary>
        [HttpPost("resetAll")]
        public IActionResult ResetAll()
        {
            try
            {
                // 1. 清理所有队列任务
                ClearAllJobs();

                // 2. 清理所有循环任务
                ClearRecurringJobs();

                // 3. 清理服务器信息
                using var connection = JobStorage.Current.GetConnection();
                var monitoring = JobStorage.Current.GetMonitoringApi();
                
                // 清理已完成的任务
                var succeededJobs = monitoring.SucceededJobs(0, int.MaxValue);
                foreach (var job in succeededJobs)
                {
                    BackgroundJob.Delete(job.Key);
                }

                // 清理已删除的任务
                var deletedJobs = monitoring.DeletedJobs(0, int.MaxValue);
                foreach (var job in deletedJobs)
                {
                    BackgroundJob.Delete(job.Key);
                }

                _logger.Info("Hangfire 完全重置完成");
                return this.Success("Hangfire 已完全重置");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "重置 Hangfire 失败");
                return this.JsonError("重置失败: " + ex.Message);
            }
        }

        /// <summary>
        /// 获取 Hangfire 统计信息
        /// </summary>
        [HttpGet("stats")]
        public IActionResult GetStats()
        {
            try
            {
                var monitoring = JobStorage.Current.GetMonitoringApi();
                using var connection = JobStorage.Current.GetConnection();

                var stats = new
                {
                    Enqueued = monitoring.EnqueuedCount("default"),
                    Processing = monitoring.ProcessingCount(),
                    Scheduled = monitoring.ScheduledCount(),
                    Failed = monitoring.FailedCount(),
                    Succeeded = monitoring.SucceededListCount(),
                    Deleted = monitoring.DeletedListCount(),
                    Recurring = connection.GetRecurringJobs().Count,
                    Servers = monitoring.Servers().Count
                };

                return this.Success(stats);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "获取 Hangfire 统计信息失败");
                return this.JsonError("获取统计信息失败: " + ex.Message);
            }
        }
    }
}