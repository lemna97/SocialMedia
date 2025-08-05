using Highever.SocialMedia.Application.Contracts.Services;
using Highever.SocialMedia.Domain.Entity;
using Highever.SocialMedia.Domain.Repository;
using System.Linq.Expressions;

namespace Highever.SocialMedia.Application.Services
{
    /// <summary>
    /// 角色菜单关联服务实现
    /// </summary>
    public class MenuPermsService : IMenuPermsService
    {
        private readonly IRepository<MenuPerms> _repository;
        private readonly IRepository<Menus> _menusRepository;
        private readonly IRepository<Roles> _rolesRepository;

        public MenuPermsService(
            IRepository<MenuPerms> repository,
            IRepository<Menus> menusRepository,
            IRepository<Roles> rolesRepository)
        {
            _repository = repository;
            _menusRepository = menusRepository;
            _rolesRepository = rolesRepository;
        }

        public async Task<int> CreateAsync(MenuPerms input)
        {
            return await _repository.InsertAsync(input);
        }

        public async Task<int> CreateAsync(List<MenuPerms> input)
        {
            return await _repository.InsertRangeAsync(input);
        }

        public async Task<int> DeleteAsync(Expression<Func<MenuPerms, bool>> predicate)
        {
            return await _repository.DeleteAsync(predicate);
        }

        public async Task<MenuPerms?> FirstOrDefaultAsync(Expression<Func<MenuPerms, bool>> predicate)
        {
            return await _repository.FirstOrDefaultAsync(predicate);
        }

        public async Task<List<MenuPerms>> GetQueryListAsync(Expression<Func<MenuPerms, bool>> predicate = null)
        {
            return await _repository.GetListAsync(predicate);
        }

        public async Task<List<Menus>> GetMenusByRoleIdAsync(long roleId)
        {
            var menuPerms = await _repository.GetListAsync(mp => mp.RoleId == roleId);
            var menuIds = menuPerms.Select(mp => mp.MenuId).ToList();
            return await _menusRepository.GetListAsync(m => menuIds.Contains(m.Id));
        }

        public async Task<bool> AssignMenusToRoleAsync(long roleId, List<long> menuIds)
        {
            // 先删除该角色的所有菜单关联
            await _repository.DeleteAsync(mp => mp.RoleId == roleId);

            // 如果菜单ID列表为空，直接返回成功
            if (!menuIds.Any())
            {
                return true;
            }

            // 添加新的菜单关联
            var menuPerms = menuIds.Select(menuId => new MenuPerms
            {
                RoleId = roleId,
                MenuId = menuId
            }).ToList();

            var result = await _repository.InsertRangeAsync(menuPerms);
            return result > 0;
        }
    }
}
