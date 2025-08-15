using Highever.SocialMedia.Application.Contracts.DTOs.System;
using Highever.SocialMedia.Common;
using Highever.SocialMedia.Domain.Entity;
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
        /// 获取所有角色
        /// </summary>
        /// <returns>角色列表</returns>
        Task<List<Roles>> GetAllAsync();

        /// <summary>
        /// 根据条件查询角色列表
        /// </summary>
        /// <param name="predicate">查询条件</param>
        /// <returns>角色列表</returns>
        Task<List<Roles>> GetListAsync(Expression<Func<Roles, bool>> predicate);

        /// <summary>
        /// 获取角色列表（带查询条件）
        /// </summary>
        /// <param name="request">查询请求</param>
        /// <returns>角色列表</returns>
        Task<List<RoleResponse>> GetRolesAsync(GetRolesRequest? request = null);

        /// <summary>
        /// 创建角色
        /// </summary>
        /// <param name="request">创建请求</param>
        /// <returns>创建结果</returns>
        Task<int> CreateRoleAsync(CreateRoleRequest request);

        /// <summary>
        /// 更新角色
        /// </summary>
        /// <param name="request">更新请求</param>
        /// <returns>更新结果</returns>
        Task<bool> UpdateRoleAsync(UpdateRoleRequest request);

        /// <summary>
        /// 删除角色
        /// </summary>
        /// <param name="request">删除请求</param>
        /// <returns>删除结果</returns>
        Task<bool> DeleteRoleAsync(DeleteRoleRequest request);

        /// <summary>
        /// 批量删除角色
        /// </summary>
        /// <param name="request">批量删除请求</param>
        /// <returns>删除结果</returns>
        Task<bool> BatchDeleteRoleAsync(BatchDeleteRoleRequest request);

        /// <summary>
        /// 检查角色代码是否存在
        /// </summary>
        /// <param name="code">角色代码</param>
        /// <param name="excludeId">排除的角色ID</param>
        /// <returns>是否存在</returns>
        Task<bool> IsRoleCodeExistAsync(string code, int? excludeId = null);

        /// <summary>
        /// 获取角色详情
        /// </summary>
        /// <param name="id">角色ID</param>
        /// <returns>角色详情</returns>
        Task<RoleResponse?> GetRoleByIdAsync(int id);
    }
}


