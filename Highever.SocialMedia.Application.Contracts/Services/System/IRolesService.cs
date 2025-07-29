using Highever.SocialMedia.Common;
using Highever.SocialMedia.Domain.Entity;
using Highever.SocialMedia.Domain.Repository;
using System.Linq.Expressions;

namespace Highever.SocialMedia.Application.Contracts
{
    /// <summary>
    /// 角色管理服务接口
    /// </summary>
    public interface IRolesService : ITransientDependency
    {
        /// <summary>
        /// 创建角色
        /// </summary>
        /// <param name="input">角色信息</param>
        /// <returns>角色ID</returns>
        Task<int> CreateAsync(Roles input);

        /// <summary>
        /// 批量创建角色
        /// </summary>
        /// <param name="input">角色列表</param>
        /// <returns>影响行数</returns>
        Task<int> CreateAsync(List<Roles> input);

        /// <summary>
        /// 更新角色信息
        /// </summary>
        /// <param name="input">角色信息</param>
        /// <returns>影响行数</returns>
        Task<int> UpdateAsync(Roles input);

        /// <summary>
        /// 批量更新角色
        /// </summary>
        /// <param name="input">角色列表</param>
        /// <returns>影响行数</returns>
        Task<int> UpdateAsync(List<Roles> input);

        /// <summary>
        /// 删除角色
        /// </summary>
        /// <param name="predicate">删除条件</param>
        /// <returns>影响行数</returns>
        Task<int> DeleteAsync(Expression<Func<Roles, bool>> predicate);

        /// <summary>
        /// 根据ID获取角色
        /// </summary>
        /// <param name="id">角色ID</param>
        /// <returns>角色信息</returns>
        Task<Roles?> GetByIdAsync(int id);

        /// <summary>
        /// 根据条件查询单个角色
        /// </summary>
        /// <param name="predicate">查询条件</param>
        /// <returns>角色信息</returns>
        Task<Roles?> FirstOrDefaultAsync(Expression<Func<Roles, bool>> predicate);

        /// <summary>
        /// 查询角色列表
        /// </summary>
        /// <param name="predicate">查询条件</param>
        /// <returns>角色列表</returns>
        Task<List<Roles>> GetQueryListAsync(Expression<Func<Roles, bool>> predicate);

        /// <summary>
        /// 分页查询角色
        /// </summary>
        /// <param name="predicate">查询条件</param>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">页大小</param>
        /// <param name="orderBy">排序字段</param>
        /// <param name="ascending">是否升序</param>
        /// <returns>分页结果</returns>
        Task<PagedResult<Roles>> GetPagedListAsync(
            Expression<Func<Roles, bool>> predicate,
            int pageIndex = 1,
            int pageSize = 20,
            Expression<Func<Roles, object>> orderBy = null,
            bool ascending = true);

        /// <summary>
        /// 统计角色数量
        /// </summary>
        /// <param name="predicate">查询条件</param>
        /// <returns>角色数量</returns>
        Task<int> CountAsync(Expression<Func<Roles, bool>> predicate);

        /// <summary>
        /// 检查角色是否存在
        /// </summary>
        /// <param name="predicate">查询条件</param>
        /// <returns>是否存在</returns>
        Task<bool> ExistsAsync(Expression<Func<Roles, bool>> predicate);
    }
}

