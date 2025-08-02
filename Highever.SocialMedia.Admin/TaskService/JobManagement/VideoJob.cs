using Hangfire;
using Highever.SocialMedia.Common;
using Highever.SocialMedia.Domain.Entity;
using Highever.SocialMedia.Domain.Repository;

namespace Highever.SocialMedia.Admin.TaskService
{
    public class VideoJob : ITaskExecutor
    {
        private readonly IServiceProvider _serviceProvider;
        private HttpClientHelper _jobSeekerService => _serviceProvider.GetRequiredService<HttpClientHelper>();

        private IRepository<TaskEntity> _repository => _serviceProvider.GetRequiredService<IRepository<TaskEntity>>();

        public VideoJob(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        /// <summary>
        /// 这里需要传入令牌的
        /// </summary>
        /// <param name="taskName"></param>
        /// <returns></returns>
        public async Task Execute(string taskName)
        {
            // 执行 Task2 任务的逻辑
            Console.WriteLine($"Executing Task 2: {taskName}");
            // 模拟任务执行 
            await Task.CompletedTask;
        }
    }
}
