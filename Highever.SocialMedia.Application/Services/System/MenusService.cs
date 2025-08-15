using Highever.SocialMedia.Application.Contracts.Services;
using Highever.SocialMedia.Domain.Entity;
using Highever.SocialMedia.Domain.Repository;
using System.Linq.Expressions;
using Highever.SocialMedia.Application.Contracts.DTOs.System;

namespace Highever.SocialMedia.Application.Services
{
    /// <summary>
    /// 菜单服务实现
    /// </summary>
    public class MenusService : IMenusService
    {
        private readonly IRepository<Menus> _repositoryMenus;
        private readonly IServiceProvider _serviceProvider;
        private readonly IRepository<MenuPerms> _menuPermsRepository;

        public MenusService(IRepository<Menus> repositoryMenus, IRepository<MenuPerms> menuPermsRepository, IServiceProvider serviceProvider)
        {
            _repositoryMenus = repositoryMenus;
            _serviceProvider = serviceProvider;
            _menuPermsRepository = menuPermsRepository;
        }

        public async Task<bool> AddAsync(Menus entity)
        {
            var result = await _repositoryMenus.InsertAsync(entity);
            return result > 0;
        }

        public async Task<bool> UpdateAsync(Menus entity)
        {
            var result = await _repositoryMenus.UpdateAsync(entity);
            return result > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var result = await _repositoryMenus.DeleteAsync(x => x.Id == id);
            return result > 0;
        }

        public async Task<Menus?> GetByIdAsync(int id)
        {
            return await _repositoryMenus.GetByIdAsync(id);
        }

        public async Task<Menus?> FirstOrDefaultAsync(Expression<Func<Menus, bool>> predicate)
        {
            return await _repositoryMenus.FirstOrDefaultAsync(predicate);
        }

        public async Task<List<Menus>> GetAllAsync()
        {
            return await _repositoryMenus.GetListAsync();
        }

        public async Task<List<Menus>> GetListAsync(Expression<Func<Menus, bool>> predicate)
        {
            return await _repositoryMenus.GetListAsync(predicate);
        }

        public async Task<List<Menus>> GetMenuTreeAsync()
        {
            var allMenus = await _repositoryMenus.GetListAsync(x => x.IsActive);
            return allMenus.OrderBy(x => x.Sort).ToList();
        }

        /// <summary>
        /// 根据角色ID获取菜单列表（联表查询优化版）
        /// </summary>
        public async Task<List<MenuResponse>> GetMenusWithRoleAsync(int? roleId = null)
        {
            if (roleId.HasValue)
            {
                var menu_ = await _repositoryMenus.QueryListAsync();
                var menuPerm_ = await _menuPermsRepository.QueryListAsync();
                // 使用联表查询，一次性获取菜单和分配状态
                var query = from menu in menu_.AsQueryable()
                            join menuPerm in menuPerm_.AsQueryable()
                            on new { MenuId = menu.Id, RoleId = roleId.Value } equals new { menuPerm.MenuId, menuPerm.RoleId }
                            into menuPermGroup
                            from mp in menuPermGroup.DefaultIfEmpty()
                            where menu.IsActive
                            select new MenuResponse
                            {
                                Id = menu.Id,
                                ParentId = menu.ParentId,
                                Name = menu.Name,
                                Code = menu.Code,
                                Url = menu.Url,
                                Icon = menu.Icon,
                                Sort = menu.Sort,
                                IsActive = menu.IsActive,
                                RoleId = roleId.Value,
                                IsAssigned = mp != null,
                                CreatedAt = menu.CreatedAt,
                                UpdatedAt = menu.UpdatedAt
                            };

                return query.OrderBy(x => x.Sort).ToList();
            }
            else
            {
                // 查询所有菜单
                var allMenus = await _repositoryMenus.GetListAsync(x => x.IsActive);
                return allMenus.Select(menu => new MenuResponse
                {
                    Id = menu.Id,
                    ParentId = menu.ParentId ?? 0,
                    Name = menu.Name,
                    Code = menu.Code,
                    Url = menu.Url,
                    Icon = menu.Icon,
                    Sort = menu.Sort,
                    IsActive = menu.IsActive,
                    RoleId = null,
                    IsAssigned = false,
                    CreatedAt = menu.CreatedAt,
                    UpdatedAt = menu.UpdatedAt
                }).OrderBy(x => x.Sort).ToList();
            }
        }

        public async Task<int> CreateMenuAsync(CreateMenuRequest request)
        {
            // 检查菜单代码是否已存在
            if (await IsMenuCodeExistAsync(request.Code))
            {
                throw new InvalidOperationException($"菜单代码 '{request.Code}' 已存在");
            }

            // 如果指定了父级菜单，检查父级菜单是否存在
            if (request.ParentId.HasValue && request.ParentId > 0)
            {
                var parentMenu = await _repositoryMenus.GetByIdAsync(request.ParentId.Value);
                if (parentMenu == null)
                {
                    throw new InvalidOperationException($"父级菜单不存在");
                }
            }

            var menu = new Menus
            {
                ParentId = request.ParentId,
                Name = request.Name,
                Code = request.Code,
                Url = request.Url,
                Icon = request.Icon,
                Sort = request.Sort,
                IsActive = request.IsActive,
                Description = request.Description,
                CreatedAt = DateTime.Now
            };

            var result = await _repositoryMenus.InsertAsync(menu);
            return result > 0 ? menu.Id : 0;
        }

        public async Task<bool> UpdateMenuAsync(UpdateMenuRequest request)
        {
            // 检查菜单是否存在
            var existingMenu = await _repositoryMenus.GetByIdAsync(request.Id);
            if (existingMenu == null)
            {
                throw new InvalidOperationException("菜单不存在");
            }

            // 检查菜单代码是否已被其他菜单使用
            if (await IsMenuCodeExistAsync(request.Code, request.Id))
            {
                throw new InvalidOperationException($"菜单代码 '{request.Code}' 已被其他菜单使用");
            }

            // 如果指定了父级菜单，检查父级菜单是否存在且不能是自己
            if (request.ParentId.HasValue && request.ParentId > 0)
            {
                if (request.ParentId.Value == request.Id)
                {
                    throw new InvalidOperationException("不能将自己设置为父级菜单");
                }

                var parentMenu = await _repositoryMenus.GetByIdAsync(request.ParentId.Value);
                if (parentMenu == null)
                {
                    throw new InvalidOperationException("父级菜单不存在");
                }

                // 检查是否会形成循环引用
                if (await WillCreateCircularReference(request.Id, request.ParentId.Value))
                {
                    throw new InvalidOperationException("不能设置该父级菜单，会形成循环引用");
                }
            }

            // 更新菜单信息
            existingMenu.ParentId = request.ParentId;
            existingMenu.Name = request.Name;
            existingMenu.Code = request.Code;
            existingMenu.Url = request.Url;
            existingMenu.Icon = request.Icon;
            existingMenu.Sort = request.Sort;
            existingMenu.IsActive = request.IsActive;
            existingMenu.Description = request.Description;
            existingMenu.UpdatedAt = DateTime.Now;

            var result = await _repositoryMenus.UpdateAsync(existingMenu);
            return result > 0;
        }

        public async Task<bool> DeleteMenuAsync(DeleteMenuRequest request)
        {
            var menu = await _repositoryMenus.GetByIdAsync(request.Id);
            if (menu == null)
            {
                throw new InvalidOperationException("菜单不存在");
            }

            // 检查是否有子菜单
            var childMenus = await _repositoryMenus.GetListAsync(x => x.ParentId == request.Id);
            if (childMenus.Any() && !request.ForceDelete)
            {
                throw new InvalidOperationException("该菜单下存在子菜单，请先删除子菜单或选择强制删除");
            }

            // 检查是否有角色关联 
            var menuPerms = await _menuPermsRepository.GetListAsync(mp => mp.MenuId == request.Id);
            if (menuPerms.Any())
            {
                // 先删除角色菜单关联
                await _menuPermsRepository.DeleteAsync(mp => mp.MenuId == request.Id);
            }

            if (request.ForceDelete && childMenus.Any())
            {
                // 递归删除子菜单
                foreach (var childMenu in childMenus)
                {
                    await DeleteMenuAsync(new DeleteMenuRequest { Id = childMenu.Id, ForceDelete = true });
                }
            }

            var result = await _repositoryMenus.DeleteAsync(x => x.Id == request.Id);
            return result > 0;
        }

        public async Task<bool> BatchDeleteMenuAsync(BatchDeleteMenuRequest request)
        {
            foreach (var id in request.Ids)
            {
                await DeleteMenuAsync(new DeleteMenuRequest { Id = id, ForceDelete = request.ForceDelete });
            }
            return true;
        }

        public async Task<bool> IsMenuCodeExistAsync(string code, int? excludeId = null)
        {
            if (excludeId.HasValue)
            {
                return await _repositoryMenus.ExistsAsync(x => x.Code == code && x.Id != excludeId.Value);
            }
            return await _repositoryMenus.ExistsAsync(x => x.Code == code);
        }

        public async Task<MenuResponse?> GetMenuByIdAsync(int id)
        {
            var menu = await _repositoryMenus.GetByIdAsync(id);
            if (menu == null) return null;

            return new MenuResponse
            {
                Id = menu.Id,
                ParentId = menu.ParentId,
                Name = menu.Name,
                Code = menu.Code,
                Url = menu.Url,
                Icon = menu.Icon,
                Sort = menu.Sort,
                IsActive = menu.IsActive,
                RoleId = null,
                IsAssigned = false,
                CreatedAt = menu.CreatedAt,
                UpdatedAt = menu.UpdatedAt
            };
        }

        /// <summary>
        /// 检查是否会形成循环引用
        /// </summary>
        private async Task<bool> WillCreateCircularReference(int menuId, int parentId)
        {
            var visited = new HashSet<int>();
            var currentId = parentId;

            while (currentId != 0 && !visited.Contains(currentId))
            {
                if (currentId == menuId)
                {
                    return true; // 发现循环引用
                }

                visited.Add(currentId);
                var parent = await _repositoryMenus.GetByIdAsync(currentId);
                currentId = parent?.ParentId ?? 0;
            }

            return false;
        }
    }
}







