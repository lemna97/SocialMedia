using Highever.SocialMedia.Domain.Entity;
using System.Linq.Expressions;

namespace Highever.SocialMedia.Domain.Repository
{
    /// <summary>
    /// 菜单仓储接口
    /// </summary>
    public interface IMenusRepository
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
        Task<bool> DeleteAsync(long id);

        /// <summary>
        /// 根据ID获取菜单
        /// </summary>
        /// <param name="id">菜单ID</param>
        /// <returns>菜单信息</returns>
        Task<Menus?> GetByIdAsync(long id);

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
    }
}