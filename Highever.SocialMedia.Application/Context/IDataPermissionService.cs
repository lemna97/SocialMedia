using Highever.SocialMedia.Common;

namespace Highever.SocialMedia.Application.Context
{
    /// <summary>
    /// 数据权限服务接口
    /// </summary>
    public interface IDataPermissionService: IScopedDependency
    {
        /// <summary>
        /// 根据用户ID加载数据权限上下文
        /// </summary>
        Task<DataPermissionContext> LoadDataPermissionAsync(long userId, List<string> roles);

        /// <summary>
        /// 获取用户的配置ID列表
        /// </summary>
        Task<List<string>> GetUserAccountConfigIdsAsync(long userId);
    }
}