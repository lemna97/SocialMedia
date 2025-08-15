using Highever.SocialMedia.Application.Contracts.DTOs.System;
using Highever.SocialMedia.Common;
using Highever.SocialMedia.Domain.Entity;
using System.Linq.Expressions;

namespace Highever.SocialMedia.Application.Contracts.Services
{
    /// <summary>
    /// 菜单服务接口
    /// </summary>
    public interface IMenusService : ITransientDependency
    {
        /// <summary>
        /// 添加菜单
        /// </summary>
        /// <param name="entity">菜单实体</param>
        /// <returns>添加结果</returns>
        Task<bool> AddAsync(Menus entity);

        /// <summary>
        /// 更新菜单
        /// </summary>
        /// <param name="entity">菜单实体</param>
        /// <returns>更新结果</returns>
        Task<bool> UpdateAsync(Menus entity);

        /// <summary>
        /// 删除菜单
        /// </summary>
        /// <param name="id">菜单ID</param>
        /// <returns>删除结果</returns>
        Task<bool> DeleteAsync(int id);

        /// <summary>
        /// 根据ID获取菜单
        /// </summary>
        /// <param name="id">菜单ID</param>
        /// <returns>菜单信息</returns>
        Task<Menus?> GetByIdAsync(int id);

        /// <summary>
        /// 根据条件查询单个菜单
        /// </summary>
        /// <param name="predicate">查询条件</param>
        /// <returns>菜单信息</returns>
        Task<Menus?> FirstOrDefaultAsync(Expression<Func<Menus, bool>> predicate);

        /// <summary>
        /// 获取所有菜单
        /// </summary>
        /// <returns>菜单列表</returns>
        Task<List<Menus>> GetAllAsync();

        /// <summary>
        /// 根据条件查询菜单列表
        /// </summary>
        /// <param name="predicate">查询条件</param>
        /// <returns>菜单列表</returns>
        Task<List<Menus>> GetListAsync(Expression<Func<Menus, bool>> predicate); 

        /// <summary>
        /// 根据角色ID获取菜单列表（包含分配状态）
        /// </summary>
        /// <param name="roleId">角色ID，为空则查询所有菜单</param>
        /// <returns>菜单列表</returns>
        Task<List<MenuResponse>> GetMenusWithRoleAsync(int? roleId = null);

        /// <summary>
        /// 创建菜单
        /// </summary>
        /// <param name="request">创建请求</param>
        /// <returns>创建结果</returns>
        Task<int> CreateMenuAsync(CreateMenuRequest request);

        /// <summary>
        /// 更新菜单
        /// </summary>
        /// <param name="request">更新请求</param>
        /// <returns>更新结果</returns>
        Task<bool> UpdateMenuAsync(UpdateMenuRequest request);

        /// <summary>
        /// 删除菜单
        /// </summary>
        /// <param name="request">删除请求</param>
        /// <returns>删除结果</returns>
        Task<bool> DeleteMenuAsync(DeleteMenuRequest request);

        /// <summary>
        /// 批量删除菜单
        /// </summary>
        /// <param name="request">批量删除请求</param>
        /// <returns>删除结果</returns>
        Task<bool> BatchDeleteMenuAsync(BatchDeleteMenuRequest request);

        /// <summary>
        /// 检查菜单代码是否存在
        /// </summary>
        /// <param name="code">菜单代码</param>
        /// <param name="excludeId">排除的菜单ID</param>
        /// <returns>是否存在</returns>
        Task<bool> IsMenuCodeExistAsync(string code, int? excludeId = null);

        /// <summary>
        /// 获取菜单详情
        /// </summary>
        /// <param name="id">菜单ID</param>
        /// <returns>菜单详情</returns>
        Task<MenuResponse?> GetMenuByIdAsync(int id);
    }
}



