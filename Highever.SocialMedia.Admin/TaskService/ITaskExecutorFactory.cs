using Highever.SocialMedia.Common;

namespace Highever.SocialMedia.Admin.TaskService
{
    public interface ITaskExecutorFactory: ITransientDependency
    {
        ITaskExecutor GetExecutor(string taskName);
    }
}