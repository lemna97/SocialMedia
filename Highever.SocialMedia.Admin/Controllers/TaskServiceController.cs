using Hangfire;
using Highever.SocialMedia.Admin.TaskService;
using Highever.SocialMedia.Common;
using Highever.SocialMedia.Domain.Entity;
using Highever.SocialMedia.Domain.Repository;
using Microsoft.AspNetCore.Mvc;

namespace Highever.SocialMedia.Admin.Controllers
{
    /// <summary>
    /// 任务管理
    /// </summary>
    [ApiController]
    [ApiGroup(SwaggerApiGroup.System)]
    [Route("api/task")]
    public class TaskServiceController : ControllerBase
    {
        private readonly IRecurringJobManager _recurringJobManager;
        private readonly IRepository<TaskEntity> _taskrepository;
        private readonly IRepository<TaskRunEntity> _task_run_repository;
        private readonly ITaskExecutorFactory _taskExecutorFactory;

        /// <summary>
        /// 任务管理
        /// </summary>
        /// <param name="recurringJobManager"></param>
        /// <param name="taskrepository"></param>
        /// <param name="task_run_repository"></param>
        /// <param name="taskExecutorFactory"></param>
        public TaskServiceController(IRecurringJobManager recurringJobManager
            , IRepository<TaskEntity> taskrepository
            , IRepository<TaskRunEntity> task_run_repository
            , ITaskExecutorFactory taskExecutorFactory)
        {
            _recurringJobManager = recurringJobManager;
            _taskrepository = taskrepository;
            _task_run_repository = task_run_repository;
            _taskExecutorFactory = taskExecutorFactory;
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
                () => newTask.CronExpression,  // Cron 表达式的获取（Func<string>）
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
        public async Task<IActionResult> UpdateTask(int taskId,string cronExpression)
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
        [ApiExplorerSettings(IgnoreApi = true)] 
        [Queue("tk_queue")]
        public void ExecuteTask(string taskName)
        {
            // 记录任务开始时间
            var taskRun = new TaskRunEntity
            {
                TaskId = _taskrepository.FirstOrDefaultAsync(t => t.TaskName == taskName).Id,
                Status = 0,  // 默认任务状态为失败
                StartTime = DateTime.Now,
                ErrorMessage = ""
            };

            _task_run_repository.InsertByIdentity(taskRun);

            try
            {
                // 通过反射获取任务执行器并执行任务
                var executor = _taskExecutorFactory.GetExecutor(taskName);
                executor.Execute(taskName);

                taskRun.Status = 1; // 成功
                taskRun.EndTime = DateTime.Now;
            }
            catch (Exception ex)
            {
                taskRun.Status = 0; // 失败
                taskRun.ErrorMessage = ex.Message;
                taskRun.EndTime = DateTime.Now;
                Console.WriteLine($"Error executing task {taskName}: {ex.Message}");
            }
            finally
            {
                _task_run_repository.Update(taskRun);
            }

            Console.WriteLine($"Task {taskName} executed at: {DateTime.Now}");
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
