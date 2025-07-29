using Highever.SocialMedia.Common;
using Highever.SocialMedia.Domain.Entity;
using Highever.SocialMedia.Domain.Repository;
using System.Linq.Expressions;

namespace Highever.SocialMedia.Application.Contracts
{
    /// <summary>
    /// 用户管理服务接口
    /// </summary>
    public interface IUsersService : ITransientDependency
    {
        /// <summary>
        /// 创建用户
        /// </summary>
        /// <param name="input">用户信息</param>
        /// <returns>用户ID</returns>
        Task<int> CreateAsync(Users input);

        /// <summary>
        /// 批量创建用户
        /// </summary>
        /// <param name="input">用户列表</param>
        /// <returns>影响行数</returns>
        Task<int> CreateAsync(List<Users> input);

        /// <summary>
        /// 更新用户信息
        /// </summary>
        /// <param name="input">用户信息</param>
        /// <returns>影响行数</returns>
        Task<int> UpdateAsync(Users input);

        /// <summary>
        /// 批量更新用户
        /// </summary>
        /// <param name="input">用户列表</param>
        /// <returns>影响行数</returns>
        Task<int> UpdateAsync(List<Users> input);

        /// <summary>
        /// 删除用户
        /// </summary>
        /// <param name="predicate">删除条件</param>
        /// <returns>影响行数</returns>
        Task<int> DeleteAsync(Expression<Func<Users, bool>> predicate);

        /// <summary>
        /// 根据ID获取用户
        /// </summary>
        /// <param name="id">用户ID</param>
        /// <returns>用户信息</returns>
        Task<Users?> GetByIdAsync(int id);

        /// <summary>
        /// 根据条件查询单个用户
        /// </summary>
        /// <param name="predicate">查询条件</param>
        /// <returns>用户信息</returns>
        Task<Users?> FirstOrDefaultAsync(Expression<Func<Users, bool>> predicate);

        /// <summary>
        /// 查询用户列表
        /// </summary>
        /// <param name="predicate">查询条件</param>
        /// <returns>用户列表</returns>
        Task<List<Users>> GetQueryListAsync(Expression<Func<Users, bool>> predicate);

        /// <summary>
        /// 分页查询用户
        /// </summary>
        /// <param name="predicate">查询条件</param>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">页大小</param>
        /// <param name="orderBy">排序字段</param>
        /// <param name="ascending">是否升序</param>
        /// <returns>分页结果</returns>
        Task<PagedResult<Users>> GetPagedListAsync(
            Expression<Func<Users, bool>> predicate,
            int pageIndex = 1,
            int pageSize = 20,
            Expression<Func<Users, object>> orderBy =null,
            bool ascending = true);

        /// <summary>
        /// 统计用户数量
        /// </summary>
        /// <param name="predicate">查询条件</param>
        /// <returns>用户数量</returns>
        Task<int> CountAsync(Expression<Func<Users, bool>> predicate);

        /// <summary>
        /// 检查用户是否存在
        /// </summary>
        /// <param name="predicate">查询条件</param>
        /// <returns>是否存在</returns>
        Task<bool> ExistsAsync(Expression<Func<Users, bool>> predicate);
    }
}


