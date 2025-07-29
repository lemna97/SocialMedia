using Highever.SocialMedia.Common;
using Highever.SocialMedia.Domain.Entity;
using Highever.SocialMedia.Domain.Repository;
using System.Linq.Expressions;

namespace Highever.SocialMedia.Application.Contracts
{
    /// <summary>
    /// 资源管理服务接口
    /// </summary>
    public interface IResourcesService : ITransientDependency
    {
        /// <summary>
        /// 创建资源
        /// </summary>
        /// <param name="input">资源信息</param>
        /// <returns>资源ID</returns>
        Task<int> CreateAsync(Resources input);

        /// <summary>
        /// 批量创建资源
        /// </summary>
        /// <param name="input">资源列表</param>
        /// <returns>影响行数</returns>
        Task<int> CreateAsync(List<Resources> input);

        /// <summary>
        /// 更新资源信息
        /// </summary>
        /// <param name="input">资源信息</param>
        /// <returns>影响行数</returns>
        Task<int> UpdateAsync(Resources input);

        /// <summary>
        /// 批量更新资源
        /// </summary>
        /// <param name="input">资源列表</param>
        /// <returns>影响行数</returns>
        Task<int> UpdateAsync(List<Resources> input);

        /// <summary>
        /// 删除资源
        /// </summary>
        /// <param name="predicate">删除条件</param>
        /// <returns>影响行数</returns>
        Task<int> DeleteAsync(Expression<Func<Resources, bool>> predicate);

        /// <summary>
        /// 根据ID获取资源
        /// </summary>
        /// <param name="id">资源ID</param>
        /// <returns>资源信息</returns>
        Task<Resources?> GetByIdAsync(int id);

        /// <summary>
        /// 根据条件查询单个资源
        /// </summary>
        /// <param name="predicate">查询条件</param>
        /// <returns>资源信息</returns>
        Task<Resources?> FirstOrDefaultAsync(Expression<Func<Resources, bool>> predicate);

        /// <summary>
        /// 查询资源列表
        /// </summary>
        /// <param name="predicate">查询条件</param>
        /// <returns>资源列表</returns>
        Task<List<Resources>> GetQueryListAsync(Expression<Func<Resources, bool>> predicate);

        /// <summary>
        /// 分页查询资源
        /// </summary>
        /// <param name="predicate">查询条件</param>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">页大小</param>
        /// <param name="orderBy">排序字段</param>
        /// <param name="ascending">是否升序</param>
        /// <returns>分页结果</returns>
        Task<PagedResult<Resources>> GetPagedListAsync(
            Expression<Func<Resources, bool>> predicate,
            int pageIndex = 1,
            int pageSize = 20,
            Expression<Func<Resources, object>> orderBy = null,
            bool ascending = true);

        /// <summary>
        /// 统计资源数量
        /// </summary>
        /// <param name="predicate">查询条件</param>
        /// <returns>资源数量</returns>
        Task<int> CountAsync(Expression<Func<Resources, bool>> predicate);

        /// <summary>
        /// 检查资源是否存在
        /// </summary>
        /// <param name="predicate">查询条件</param>
        /// <returns>是否存在</returns>
        Task<bool> ExistsAsync(Expression<Func<Resources, bool>> predicate);
    }
}
