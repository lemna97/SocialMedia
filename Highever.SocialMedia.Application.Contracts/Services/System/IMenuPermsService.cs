using Highever.SocialMedia.Domain.Entity;
using System.Linq.Expressions;

namespace Highever.SocialMedia.Application.Contracts.Services
{
    /// <summary>
    /// 角色菜单关联服务接口
    /// </summary>
    public interface IMenuPermsService
    {
        /// <summary>
        /// 创建角色菜单关联
        /// </summary>
        /// <param name="input">关联信息</param>
        /// <returns>创建结果</returns>
        Task<int> CreateAsync(MenuPerms input);

        /// <summary>
        /// 批量创建角色菜单关联
        /// </summary>
        /// <param name="input">关联信息列表</param>
        /// <returns>创建结果</returns>
        Task<int> CreateAsync(List<MenuPerms> input);

        /// <summary>
        /// 删除角色菜单关联
        /// </summary>
        /// <param name="predicate">删除条件</param>
        /// <returns>删除结果</returns>
        Task<int> DeleteAsync(Expression<Func<MenuPerms, bool>> predicate);

        /// <summary>
        /// 查询单个角色菜单关联
        /// </summary>
        /// <param name="predicate">查询条件</param>
        /// <returns>关联信息</returns>
        Task<MenuPerms?> FirstOrDefaultAsync(Expression<Func<MenuPerms, bool>> predicate);

        /// <summary>
        /// 查询角色菜单关联列表
        /// </summary>
        /// <param name="predicate">查询条件</param>
        /// <returns>关联列表</returns>
        Task<List<MenuPerms>> GetQueryListAsync(Expression<Func<MenuPerms, bool>> predicate = null);

        /// <summary>
        /// 根据角色ID获取菜单列表
        /// </summary>
        /// <param name="roleId">角色ID</param>
        /// <returns>菜单列表</returns>
        Task<List<Menus>> GetMenusByRoleIdAsync(long roleId);

        /// <summary>
        /// 为角色分配菜单
        /// </summary>
        /// <param name="roleId">角色ID</param>
        /// <param name="menuIds">菜单ID列表</param>
        /// <returns>分配结果</returns>
        Task<bool> AssignMenusToRoleAsync(long roleId, List<long> menuIds);
    }
}