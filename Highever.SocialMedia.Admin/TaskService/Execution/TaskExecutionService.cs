using Highever.SocialMedia.Common;
using Highever.SocialMedia.Domain.Entity;
using Highever.SocialMedia.Domain.Repository;
using Microsoft.AspNetCore.Mvc;


namespace Highever.SocialMedia.Admin.TaskService
{
    public class TaskExecutionService: ITaskExecutionService
    {
        private readonly IRepository<TaskEntity> _taskrepository;
        private readonly IRepository<TaskRunEntity> _task_run_repository;
        private readonly ITaskExecutorFactory _taskExecutorFactory;
        private readonly INLogger _logger;

        public TaskExecutionService(
            IRepository<TaskEntity> taskrepository,
            IRepository<TaskRunEntity> task_run_repository,
            ITaskExecutorFactory taskExecutorFactory,
            INLogger logger)
        {
            _taskrepository = taskrepository;
            _task_run_repository = task_run_repository;
            _taskExecutorFactory = taskExecutorFactory;
            _logger = logger;
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

    }
}
