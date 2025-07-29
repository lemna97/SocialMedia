using Highever.SocialMedia.Application.Contracts;
using Highever.SocialMedia.Domain.Entity;
using Highever.SocialMedia.Domain.Repository;
using System.Linq.Expressions;

namespace Highever.SocialMedia.Application.Services.System
{
    public class UserRolesService : IUserRolesService
    {
        private readonly IRepository<UserRoles> _repository;
        private readonly IRepository<Roles> _rolesRepository;
        private readonly IRepository<Users> _usersRepository;

        public UserRolesService(
            IRepository<UserRoles> repository,
            IRepository<Roles> rolesRepository,
            IRepository<Users> usersRepository)
        {
            _repository = repository;
            _rolesRepository = rolesRepository;
            _usersRepository = usersRepository;
        }

        public async Task<int> CreateAsync(UserRoles input)
        {
            return await _repository.InsertAsync(input);
        }

        public async Task<int> CreateAsync(List<UserRoles> input)
        {
            return await _repository.InsertRangeAsync(input);
        }

        public async Task<int> DeleteAsync(Expression<Func<UserRoles, bool>> predicate)
        {
            return await _repository.DeleteAsync(predicate);
        }

        public async Task<UserRoles?> FirstOrDefaultAsync(Expression<Func<UserRoles, bool>> predicate)
        {
            return await _repository.FirstOrDefaultAsync(predicate);
        }

        public async Task<List<UserRoles>> GetQueryListAsync(Expression<Func<UserRoles, bool>> predicate = null)
        {
            return await _repository.GetListAsync(predicate);
        }

        public async Task<List<Roles>> GetRolesByUserIdAsync(int userId)
        {
            var userRoles = await _repository.GetListAsync(ur => ur.UserId == userId);
            var roleIds = userRoles.Select(ur => ur.RoleId).ToList();
            return await _rolesRepository.GetListAsync(r => roleIds.Contains(r.Id));
        }

        public async Task<List<Users>> GetUsersByRoleIdAsync(int roleId)
        {
            var userRoles = await _repository.GetListAsync(ur => ur.RoleId == roleId);
            var userIds = userRoles.Select(ur => ur.UserId).ToList();
            return await _usersRepository.GetListAsync(u => userIds.Contains(u.Id));
        }

        public async Task<bool> HasRoleAsync(int userId, int roleId)
        {
            var userRole = await _repository.FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId);
            return userRole != null;
        }
    }
}

