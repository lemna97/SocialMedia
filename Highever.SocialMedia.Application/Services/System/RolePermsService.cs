using Highever.SocialMedia.Application.Contracts;
using Highever.SocialMedia.Domain.Entity;
using Highever.SocialMedia.Domain.Repository;
using System.Linq.Expressions;

namespace Highever.SocialMedia.Application.Services.System
{
    public class RolePermsService : IRolePermsService
    {
        private readonly IRepository<RolePerms> _repository;
        private readonly IRepository<Permissions> _permissionsRepository;
        private readonly IRepository<Roles> _rolesRepository;

        public RolePermsService(
            IRepository<RolePerms> repository,
            IRepository<Permissions> permissionsRepository,
            IRepository<Roles> rolesRepository)
        {
            _repository = repository;
            _permissionsRepository = permissionsRepository;
            _rolesRepository = rolesRepository;
        }

        public async Task<int> CreateAsync(RolePerms input)
        {
            return await _repository.InsertAsync(input);
        }

        public async Task<int> CreateAsync(List<RolePerms> input)
        {
            return await _repository.InsertRangeAsync(input);
        }

        public async Task<int> DeleteAsync(Expression<Func<RolePerms, bool>> predicate)
        {
            return await _repository.DeleteAsync(predicate);
        }

        public async Task<RolePerms?> FirstOrDefaultAsync(Expression<Func<RolePerms, bool>> predicate)
        {
            return await _repository.FirstOrDefaultAsync(predicate);
        }

        public async Task<List<RolePerms>> GetQueryListAsync(Expression<Func<RolePerms, bool>> predicate = null)
        {
            return await _repository.GetListAsync(predicate);
        }

        public async Task<List<Permissions>> GetPermissionsByRoleIdAsync(int roleId)
        {
            var rolePerms = await _repository.GetListAsync(rp => rp.RoleId == roleId);
            var permIds = rolePerms.Select(rp => rp.PermId).ToList();
            return await _permissionsRepository.GetListAsync(p => permIds.Contains(p.Id));
        }

        public async Task<List<Roles>> GetRolesByPermIdAsync(int permId)
        {
            var rolePerms = await _repository.GetListAsync(rp => rp.PermId == permId);
            var roleIds = rolePerms.Select(rp => rp.RoleId).ToList();
            return await _rolesRepository.GetListAsync(r => roleIds.Contains(r.Id));
        }

        public async Task<bool> HasPermissionAsync(int roleId, int permId)
        {
            var rolePerms = await _repository.FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermId == permId);
            return rolePerms != null;
        }
    }
}

