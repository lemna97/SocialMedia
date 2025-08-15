using Hangfire;
using Highever.SocialMedia.Admin.TaskService;
using Highever.SocialMedia.Common;
using Highever.SocialMedia.Domain.Entity;
using Highever.SocialMedia.Domain.Repository;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Highever.SocialMedia.Admin
{
    /// <summary>
    /// 任务管理
    /// </summary>
    [ApiController]
    [ApiGroup(SwaggerApiGroup.Media)]
    [Route("api/task")]
    public class TaskServiceController : Controller
    {
        private readonly IRecurringJobManager _recurringJobManager;
        private readonly IRepository<TaskEntity> _taskrepository;
        private readonly INLogger _logger; // 添加日志记录器
        private readonly ITaskExecutionService _taskExecutionService;
        private readonly ITaskExecutionService _ITaskExecutor;

        /// <summary>
        /// 任务管理
        /// </summary>
        /// <param name="recurringJobManager"></param>
        /// <param name="taskrepository"></param> 
        /// <param name="logger"></param>
        /// <param name="taskExecutionService"></param>
        public TaskServiceController(
            IRecurringJobManager recurringJobManager,
            IRepository<TaskEntity> taskrepository,
            INLogger logger,
            ITaskExecutionService taskExecutionService,
            ITaskExecutionService iTaskExecutor)
        {
            _recurringJobManager = recurringJobManager;
            _taskrepository = taskrepository;
            _logger = logger;
            _taskExecutionService = taskExecutionService;
            _ITaskExecutor = iTaskExecutor;
        }

        /// <summary>
        /// 增加任务
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        [HttpPost("addTask")]
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
                () => _taskExecutionService.ExecuteTask(newTask.TaskName),  // 执行任务的方法
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
        [HttpPost("updateTask")]
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
                () => _taskExecutionService.ExecuteTask(task.TaskName),  // 执行任务的方法
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
        [HttpPost("deleteTask")]
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
        /// 获取任务列表
        /// </summary>
        /// <returns></returns>
        [HttpGet("getTasks")]
        public async Task<IActionResult> GetTasks()
        {
            var tasks = await _taskrepository.QueryListAsync();
            return this.Success(tasks);
        }

        /// <summary>
        /// 手动同步用户
        /// </summary>
        /// <returns></returns>
        [HttpGet("runUserJob")]
        public async Task<IActionResult> RunUserJob()
        {
            await _ITaskExecutor.ExecuteTask("UserJob");
            return this.Success();
        }


        /// <summary>
        /// 手动同步所有用户视频
        /// </summary>
        /// <returns></returns>
        [HttpGet("runVideoJob")]
        public async Task<IActionResult> RunVideoJob()
        {
            await _ITaskExecutor.ExecuteTask("VideoJob");
            return this.Success();
        }


    }
}
