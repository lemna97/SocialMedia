using Highever.SocialMedia.Common;

namespace Highever.SocialMedia.Application.Context
{
    /// <summary>
    /// 数据权限上下文服务接口
    /// </summary>
    public interface IDataPermissionContextService: IScopedDependency
    {
        /// <summary>
        /// 获取当前数据权限上下文
        /// </summary>
        DataPermissionContext? GetCurrentContext();

        /// <summary>
        /// 设置当前数据权限上下文
        /// </summary>
        void SetCurrentContext(DataPermissionContext context);

        /// <summary>
        /// 清除当前上下文
        /// </summary>
        void ClearContext();
    }
}