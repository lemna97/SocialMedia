using Highever.SocialMedia.Application.Contracts;
using Highever.SocialMedia.Application.Contracts.DTOs.System;
using Highever.SocialMedia.Common;
using Highever.SocialMedia.Domain.Entity;
using Highever.SocialMedia.Domain.Repository;
using System.Linq.Expressions;

namespace Highever.SocialMedia.Application.Services.System
{
    public class RolesService : IRolesService
    {
        private readonly IRepository<Roles> _repository;
        private readonly IRepository<MenuPerms> _repositoryMenuPerms;
        private readonly IServiceProvider _serviceProvider;

        public RolesService(IRepository<Roles> repository, IServiceProvider serviceProvider, IRepository<MenuPerms> repositoryMenuPerms)
        {
            _repository = repository;
            _serviceProvider = serviceProvider;
            _repositoryMenuPerms = repositoryMenuPerms;
        }

        public async Task<int> CreateAsync(Roles input)
        {
            return await _repository.InsertAsync(input);
        }

        public async Task<int> CreateAsync(List<Roles> input)
        {
            return await _repository.InsertRangeAsync(input);
        }

        public async Task<int> UpdateAsync(Roles input)
        {
            return await _repository.UpdateAsync(input);
        }

        public async Task<int> UpdateAsync(List<Roles> input)
        {
            return await _repository.UpdateRangeAsync(input);
        }

        public async Task<int> DeleteAsync(Expression<Func<Roles, bool>> predicate)
        {
            return await _repository.DeleteAsync(predicate);
        }

        public async Task<Roles?> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<Roles?> FirstOrDefaultAsync(Expression<Func<Roles, bool>> predicate)
        {
            return await _repository.FirstOrDefaultAsync(predicate);
        }

        public async Task<List<Roles>> GetAllAsync()
        {
            return await _repository.GetListAsync();
        }

        public async Task<List<Roles>> GetListAsync(Expression<Func<Roles, bool>> predicate)
        {
            return await _repository.GetListAsync(predicate);
        }

        public async Task<List<RoleResponse>> GetRolesAsync(GetRolesRequest? request = null)
        {
            var query = await _repository.GetListAsync();

            // 应用查询条件
            if (request != null)
            {
                if (!string.IsNullOrWhiteSpace(request.Name))
                {
                    query = query.Where(x => x.Name.Contains(request.Name)).ToList();
                }
            }

            return query.Select(role => new RoleResponse
            {
                Id = role.Id,
                Name = role.Name,
                Code = role.Code,
                CreatedAt = role.CreatedAt,
                UpdatedAt = role.UpdatedAt
            }).ToList();
        }

        public async Task<int> CreateRoleAsync(CreateRoleRequest request)
        {
            // 检查角色代码是否已存在
            if (await IsRoleCodeExistAsync(request.Code))
            {
                throw new InvalidOperationException($"角色代码 '{request.Code}' 已存在");
            }

            var role = new Roles
            {
                Name = request.Name,
                Code = request.Code,
                IsSys = false,
                UpdatedAt = DateTime.Now,
                CreatedAt = DateTime.Now
            };

            var result = await _repository.InsertAsync(role);
            return result > 0 ? role.Id : 0;
        }

        public async Task<bool> UpdateRoleAsync(UpdateRoleRequest request)
        {
            // 检查角色是否存在
            var existingRole = await _repository.GetByIdAsync(request.Id);
            if (existingRole == null)
            {
                throw new InvalidOperationException("角色不存在");
            }

            // 检查角色代码是否已被其他角色使用
            if (await IsRoleCodeExistAsync(request.Code, request.Id))
            {
                throw new InvalidOperationException($"角色代码 '{request.Code}' 已被其他角色使用");
            }

            // 更新角色信息
            existingRole.Name = request.Name;
            existingRole.Code = request.Code; 
            existingRole.UpdatedAt = DateTime.Now;

            var result = await _repository.UpdateAsync(existingRole);
            return result > 0;
        }

        public async Task<bool> DeleteRoleAsync(DeleteRoleRequest request)
        {
            var role = await _repository.GetByIdAsync(request.Id);
            if (role == null)
            {
                throw new InvalidOperationException("角色不存在");
            }

            // 检查是否有菜单关联 
            var menuPerms = await _repositoryMenuPerms.GetListAsync(mp => mp.RoleId == request.Id);
            if (menuPerms.Any())
            {
                if (!request.ForceDelete)
                {
                    throw new InvalidOperationException("该角色下存在菜单关联，请先删除关联或选择强制删除");
                }
                // 强制删除时，先删除菜单关联
                await _repositoryMenuPerms.DeleteAsync(mp => mp.RoleId == request.Id);
            }

            // TODO: 检查是否有用户角色关联，如果有用户角色关联表的话
            // var userRolesRepository = _serviceProvider.GetRequiredService<IRepository<UserRoles>>();
            // var userRoles = await userRolesRepository.GetListAsync(ur => ur.RoleId == request.Id);
            // if (userRoles.Any())
            // {
            //     if (!request.ForceDelete)
            //     {
            //         throw new InvalidOperationException("该角色下存在用户关联，请先删除关联或选择强制删除");
            //     }
            //     await userRolesRepository.DeleteAsync(ur => ur.RoleId == request.Id);
            // }

            var result = await _repository.DeleteAsync(x => x.Id == request.Id);
            return result > 0;
        }

        public async Task<bool> BatchDeleteRoleAsync(BatchDeleteRoleRequest request)
        {
            foreach (var id in request.Ids)
            {
                await DeleteRoleAsync(new DeleteRoleRequest { Id = id, ForceDelete = request.ForceDelete });
            }
            return true;
        }

        public async Task<bool> IsRoleCodeExistAsync(string code, int? excludeId = null)
        {
            if (excludeId.HasValue)
            {
                return await _repository.ExistsAsync(x => x.Code == code && x.Id != excludeId.Value);
            }
            return await _repository.ExistsAsync(x => x.Code == code);
        }

        public async Task<RoleResponse?> GetRoleByIdAsync(int id)
        {
            var role = await _repository.GetByIdAsync(id);
            if (role == null) return null;

            return new RoleResponse
            {
                Id = role.Id,
                Name = role.Name,
                Code = role.Code, 
                CreatedAt = role.CreatedAt,
                UpdatedAt = role.UpdatedAt
            };
        }
    }
}


