using Highever.SocialMedia.Common;
using Highever.SocialMedia.Domain.Entity;
using Highever.SocialMedia.Domain.Repository;
using System.Linq.Expressions;

namespace Highever.SocialMedia.Application.Contracts
{
    /// <summary>
    /// 权限管理服务接口
    /// </summary>
    public interface IPermissionsService : ITransientDependency
    {
        /// <summary>
        /// 创建权限
        /// </summary>
        /// <param name="input">权限信息</param>
        /// <returns>权限ID</returns>
        Task<int> CreateAsync(Permissions input);

        /// <summary>
        /// 批量创建权限
        /// </summary>
        /// <param name="input">权限列表</param>
        /// <returns>影响行数</returns>
        Task<int> CreateAsync(List<Permissions> input);

        /// <summary>
        /// 更新权限信息
        /// </summary>
        /// <param name="input">权限信息</param>
        /// <returns>影响行数</returns>
        Task<int> UpdateAsync(Permissions input);

        /// <summary>
        /// 批量更新权限
        /// </summary>
        /// <param name="input">权限列表</param>
        /// <returns>影响行数</returns>
        Task<int> UpdateAsync(List<Permissions> input);

        /// <summary>
        /// 删除权限
        /// </summary>
        /// <param name="predicate">删除条件</param>
        /// <returns>影响行数</returns>
        Task<int> DeleteAsync(Expression<Func<Permissions, bool>> predicate);

        /// <summary>
        /// 根据ID获取权限
        /// </summary>
        /// <param name="id">权限ID</param>
        /// <returns>权限信息</returns>
        Task<Permissions?> GetByIdAsync(int id);

        /// <summary>
        /// 根据条件查询单个权限
        /// </summary>
        /// <param name="predicate">查询条件</param>
        /// <returns>权限信息</returns>
        Task<Permissions?> FirstOrDefaultAsync(Expression<Func<Permissions, bool>> predicate);

        /// <summary>
        /// 查询权限列表
        /// </summary>
        /// <param name="predicate">查询条件</param>
        /// <returns>权限列表</returns>
        Task<List<Permissions>> GetQueryListAsync(Expression<Func<Permissions, bool>> predicate);

        /// <summary>
        /// 分页查询权限
        /// </summary>
        /// <param name="predicate">查询条件</param>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">页大小</param>
        /// <param name="orderBy">排序字段</param>
        /// <param name="ascending">是否升序</param>
        /// <returns>分页结果</returns>
        Task<PagedResult<Permissions>> GetPagedListAsync(
            Expression<Func<Permissions, bool>> predicate,
            int pageIndex = 1,
            int pageSize = 20,
            Expression<Func<Permissions, object>> orderBy = null,
            bool ascending = true);

        /// <summary>
        /// 统计权限数量
        /// </summary>
        /// <param name="predicate">查询条件</param>
        /// <returns>权限数量</returns>
        Task<int> CountAsync(Expression<Func<Permissions, bool>> predicate = null);

        /// <summary>
        /// 检查权限是否存在
        /// </summary>
        /// <param name="predicate">查询条件</param>
        /// <returns>是否存在</returns>
        Task<bool> ExistsAsync(Expression<Func<Permissions, bool>> predicate);
    }
}
