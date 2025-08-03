using Hangfire;
using Highever.SocialMedia.Admin;
using Highever.SocialMedia.Admin.TaskService;
using Highever.SocialMedia.Common;
using Highever.SocialMedia.Domain.Entity;
using Highever.SocialMedia.Domain.Repository;
using Microsoft.AspNetCore.Mvc;

namespace Highever.SocialMedia.API
{
    /// <summary>
    /// 任务管理
    /// </summary>
    [ApiController]
    [ApiGroup(SwaggerApiGroup.System)]
    [Route("api/task")]
    public class TaskServiceController : Controller
    {
        private readonly IRecurringJobManager _recurringJobManager;
        private readonly IRepository<TaskEntity> _taskrepository;
        private readonly IRepository<TaskRunEntity> _task_run_repository;
        private readonly ITaskExecutorFactory _taskExecutorFactory;
        private readonly INLogger _logger; // 添加日志记录器

        /// <summary>
        /// 任务管理
        /// </summary>
        /// <param name="recurringJobManager"></param>
        /// <param name="taskrepository"></param>
        /// <param name="task_run_repository"></param>
        /// <param name="taskExecutorFactory"></param>
        /// <param name="logger"></param>
        public TaskServiceController(
            IRecurringJobManager recurringJobManager,
            IRepository<TaskEntity> taskrepository,
            IRepository<TaskRunEntity> task_run_repository,
            ITaskExecutorFactory taskExecutorFactory,
            INLogger logger) // 注入日志记录器
        {
            _recurringJobManager = recurringJobManager;
            _taskrepository = taskrepository;
            _task_run_repository = task_run_repository;
            _taskExecutorFactory = taskExecutorFactory;
            _logger = logger;
        }

        /// <summary>
        /// 增加任务
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        [HttpPost("add-task")]
        public async Task<IActionResult> AddTask([FromBody] TaskEntity task)
        {
            // 向数据库插入任务
            var newTask = new TaskEntity
            {
                TaskName = task.TaskName,
                CronExpression = task.CronExpression,
                Description = task.Description,
                CreatedIP = HttpContext.Connection.RemoteIpAddress?.ToString(),
                Status = 1  // 默认启用
            };
            var taskId = await _taskrepository.InsertByIdentityAsync(newTask);
            // 使用 Cron 表达式安排定时任务
            _recurringJobManager.AddOrUpdate(
                taskId.ToString(),  // 任务的唯一标识符 
                () => ExecuteTask(newTask.TaskName),  // 执行任务的方法
                newTask.CronExpression,  // Cron 表达式的获取
                new RecurringJobOptions
                {
                    TimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Shanghai"), // 设置时区为中国标准时间
                    MisfireHandling = MisfireHandlingMode.Strict,  // 设置错过任务后的处理方式为严格模式
                }
            );

            return Ok("Task scheduled successfully");
        }

        /// <summary>
        /// 修改任务定时时间
        /// </summary>
        /// <param name="taskId"></param>
        /// <param name="cronExpression"></param>
        /// <returns></returns>
        [HttpPost("update-task")]
        public async Task<IActionResult> UpdateTask(int taskId, string cronExpression)
        {
            var task = await _taskrepository.FirstOrDefaultAsync(t => t.Id == taskId);
            if (task == null)
            {
                return NotFound("Task not found");
            }
            task.CronExpression = cronExpression;
            task.UpdatedAt = DateTime.Now;
            await _taskrepository.UpdateAsync(task);

            // 更新任务的 Cron 表达式
            _recurringJobManager.AddOrUpdate(
                taskId.ToString(),
                () => ExecuteTask(task.TaskName),  // 执行任务的方法
                () => task.CronExpression,        // Cron 表达式的获取
                new RecurringJobOptions()         // 可选的任务选项
            );

            return Ok("Task updated successfully");
        }

        /// <summary>
        /// 删除任务
        /// </summary>
        /// <param name="taskId"></param>
        /// <returns></returns>
        [HttpPost("delete-task")]
        public async Task<IActionResult> DeleteTask(int taskId)
        {
            var task = await _taskrepository.FirstOrDefaultAsync(t => t.Id == taskId);
            if (task != null)
            {
                // 删除任务记录
                await _taskrepository.DeleteAsync(task);
            }

            // 删除定时任务
            _recurringJobManager.RemoveIfExists(taskId.ToString());

            return this.Success("Task deleted successfully");
        }


        /// <summary>
        /// 执行任务的实际方法
        /// </summary>
        /// <param name="taskName"></param>
        /// <remarks>
        ///  [Queue("tk_queue")] 可以分成多个队列来处理这些作业
        /// </remarks>
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task ExecuteTask(string taskName)
        {
            var task = await _taskrepository.FirstOrDefaultAsync(t => t.TaskName == taskName);
            if (task == null)
            {
                _logger.TaskError(taskName, new InvalidOperationException($"未找到任务: {taskName}"));
                return;
            }

            // 记录任务开始时间
            var taskRun = new TaskRunEntity
            {
                TaskId = task.Id,
                Status = 0,
                StartTime = DateTime.Now,
                ErrorMessage = ""
            };

            var taskRunId = await _task_run_repository.InsertByIdentityAsync(taskRun);
            _logger.TaskStart(taskName, task.Id, taskRunId);

            try
            {
                var executor = _taskExecutorFactory.GetExecutor(taskName);
                await executor.Execute(taskName);

                taskRun.Status = 1;
                taskRun.EndTime = DateTime.Now;
                await _task_run_repository.UpdateAsync(taskRun);
                
                var executionTime = (long)(taskRun.EndTime - taskRun.StartTime).TotalMilliseconds;
                _logger.TaskComplete(taskName, executionTime, 0, 0, 0, 0, task.Id, taskRunId);
            }
            catch (Exception ex)
            {
                taskRun.Status = 0;
                taskRun.EndTime = DateTime.Now;
                taskRun.ErrorMessage = ex.Message;
                await _task_run_repository.UpdateAsync(taskRun);
                
                _logger.TaskError(taskName, ex, task.Id, taskRunId);
                throw;
            }
        }


        /// <summary>
        /// 获取任务列表
        /// </summary>
        /// <returns></returns>
        [HttpGet("get-tasks")]
        public async Task<IActionResult> GetTasks()
        {
            var tasks = await _taskrepository.QueryListAsync();
            return this.Success(tasks);
        }
    }
}
