namespace Highever.SocialMedia.Admin.TaskService
{
    /// <summary>
    /// 任务执行策略工厂
    /// </summary>
    public class TaskExecutorFactory : ITaskExecutorFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public TaskExecutorFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// 根据任务名称获取对应的执行器
        /// </summary>
        /// <param name="taskName"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public ITaskExecutor GetExecutor(string taskName)
        {
            // 构建类名，假设任务名与类名一致
            //注意命名空间 JobManagement
            var className = $"{typeof(TaskExecutorFactory).Namespace}.{taskName}";
            var executorType = Type.GetType(className);

            if (executorType == null)
            {
                throw new InvalidOperationException($"Executor not found for task: {taskName}");
            }

            // 使用 IServiceProvider 动态解析执行器实例
            var executor = (ITaskExecutor)_serviceProvider.GetRequiredService(executorType);
            return executor;
        }
    }
}
