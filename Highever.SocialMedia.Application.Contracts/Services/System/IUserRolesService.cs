using Highever.SocialMedia.Common;
using Highever.SocialMedia.Domain.Entity;
using System.Linq.Expressions;

namespace Highever.SocialMedia.Application.Contracts
{
    /// <summary>
    /// 用户角色关联服务接口
    /// </summary>
    public interface IUserRolesService : ITransientDependency
    {
        /// <summary>
        /// 创建用户角色关联
        /// </summary>
        /// <param name="input">用户角色关联信息</param>
        /// <returns>影响行数</returns>
        Task<int> CreateAsync(UserRoles input);

        /// <summary>
        /// 批量创建用户角色关联
        /// </summary>
        /// <param name="input">用户角色关联列表</param>
        /// <returns>影响行数</returns>
        Task<int> CreateAsync(List<UserRoles> input);

        /// <summary>
        /// 删除用户角色关联
        /// </summary>
        /// <param name="predicate">删除条件</param>
        /// <returns>影响行数</returns>
        Task<int> DeleteAsync(Expression<Func<UserRoles, bool>> predicate);

        /// <summary>
        /// 根据条件查询单个用户角色关联
        /// </summary>
        /// <param name="predicate">查询条件</param>
        /// <returns>用户角色关联信息</returns>
        Task<UserRoles?> FirstOrDefaultAsync(Expression<Func<UserRoles, bool>> predicate);

        /// <summary>
        /// 查询用户角色关联列表
        /// </summary>
        /// <param name="predicate">查询条件</param>
        /// <returns>用户角色关联列表</returns>
        Task<List<UserRoles>> GetQueryListAsync(Expression<Func<UserRoles, bool>> predicate);

        /// <summary>
        /// 根据用户ID获取角色列表
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>角色列表</returns>
        Task<List<Roles>> GetRolesByUserIdAsync(int userId);

        /// <summary>
        /// 根据角色ID获取用户列表
        /// </summary>
        /// <param name="roleId">角色ID</param>
        /// <returns>用户列表</returns>
        Task<List<Users>> GetUsersByRoleIdAsync(int roleId);

        /// <summary>
        /// 检查用户是否拥有指定角色
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="roleId">角色ID</param>
        /// <returns>是否拥有角色</returns>
        Task<bool> HasRoleAsync(int userId, int roleId);
    }
}
