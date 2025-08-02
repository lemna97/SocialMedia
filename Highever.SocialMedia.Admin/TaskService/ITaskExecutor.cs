using Highever.SocialMedia.Common;

namespace Highever.SocialMedia.Admin.TaskService
{
    /// <summary>
    /// 任务执行接口
    /// </summary>
    public interface ITaskExecutor : ITransientDependency
    {
        Task Execute(string taskName); // 改为返回Task
    }

}
