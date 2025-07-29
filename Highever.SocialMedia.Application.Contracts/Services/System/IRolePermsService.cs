using Highever.SocialMedia.Common;
using Highever.SocialMedia.Domain.Entity;
using System.Linq.Expressions;

namespace Highever.SocialMedia.Application.Contracts
{
    /// <summary>
    /// 角色权限关联服务接口
    /// </summary>
    public interface IRolePermsService : ITransientDependency
    {
        /// <summary>
        /// 创建角色权限关联
        /// </summary>
        /// <param name="input">角色权限关联信息</param>
        /// <returns>影响行数</returns>
        Task<int> CreateAsync(RolePerms input);

        /// <summary>
        /// 批量创建角色权限关联
        /// </summary>
        /// <param name="input">角色权限关联列表</param>
        /// <returns>影响行数</returns>
        Task<int> CreateAsync(List<RolePerms> input);

        /// <summary>
        /// 删除角色权限关联
        /// </summary>
        /// <param name="predicate">删除条件</param>
        /// <returns>影响行数</returns>
        Task<int> DeleteAsync(Expression<Func<RolePerms, bool>> predicate);

        /// <summary>
        /// 根据条件查询单个角色权限关联
        /// </summary>
        /// <param name="predicate">查询条件</param>
        /// <returns>角色权限关联信息</returns>
        Task<RolePerms?> FirstOrDefaultAsync(Expression<Func<RolePerms, bool>> predicate);

        /// <summary>
        /// 查询角色权限关联列表
        /// </summary>
        /// <param name="predicate">查询条件</param>
        /// <returns>角色权限关联列表</returns>
        Task<List<RolePerms>> GetQueryListAsync(Expression<Func<RolePerms, bool>> predicate);

        /// <summary>
        /// 根据角色ID获取权限列表
        /// </summary>
        /// <param name="roleId">角色ID</param>
        /// <returns>权限列表</returns>
        Task<List<Permissions>> GetPermissionsByRoleIdAsync(int roleId);

        /// <summary>
        /// 根据权限ID获取角色列表
        /// </summary>
        /// <param name="permId">权限ID</param>
        /// <returns>角色列表</returns>
        Task<List<Roles>> GetRolesByPermIdAsync(int permId);

        /// <summary>
        /// 检查角色是否拥有指定权限
        /// </summary>
        /// <param name="roleId">角色ID</param>
        /// <param name="permId">权限ID</param>
        /// <returns>是否拥有权限</returns>
        Task<bool> HasPermissionAsync(int roleId, int permId);
    }
}
