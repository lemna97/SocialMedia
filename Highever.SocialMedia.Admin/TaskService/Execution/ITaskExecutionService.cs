using Highever.SocialMedia.Common;


namespace Highever.SocialMedia.Admin.TaskService
{
    public interface ITaskExecutionService : IScopedDependency
    {
        Task ExecuteTask(string taskName);
    }
}
