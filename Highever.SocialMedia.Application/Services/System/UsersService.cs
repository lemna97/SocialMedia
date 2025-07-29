using Highever.SocialMedia.Application.Contracts;
using Highever.SocialMedia.Common;
using Highever.SocialMedia.Domain.Entity;
using Highever.SocialMedia.Domain.Repository;
using System.Linq.Expressions;

namespace Highever.SocialMedia.Application.Services.System
{
    public class UsersService : IUsersService
    {
        private readonly IRepository<Users> _repository;

        public UsersService(IRepository<Users> repository)
        {
            _repository = repository;
        }

        public async Task<int> CreateAsync(Users input)
        {
            return await _repository.InsertAsync(input);
        }

        public async Task<int> CreateAsync(List<Users> input)
        {
            return await _repository.InsertRangeAsync(input);
        }

        public async Task<int> UpdateAsync(Users input)
        {
            return await _repository.UpdateAsync(input);
        }

        public async Task<int> UpdateAsync(List<Users> input)
        {
            return await _repository.UpdateRangeAsync(input);
        }

        public async Task<int> DeleteAsync(Expression<Func<Users, bool>> predicate)
        {
            return await _repository.DeleteAsync(predicate);
        }

        public async Task<Users?> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<Users?> FirstOrDefaultAsync(Expression<Func<Users, bool>> predicate)
        {
            return await _repository.FirstOrDefaultAsync(predicate);
        }

        public async Task<List<Users>> GetQueryListAsync(Expression<Func<Users, bool>> predicate = null)
        {
            return await _repository.GetListAsync(predicate);
        }

        public async Task<PagedResult<Users>> GetPagedListAsync(
            Expression<Func<Users, bool>> predicate = null,
            int pageIndex = 1,
            int pageSize = 20,
            Expression<Func<Users, object>> orderBy = null,
            bool ascending = true)
        {
            return await _repository.GetPagedListAsync(predicate, pageIndex, pageSize, orderBy, ascending);
        }

        public async Task<int> CountAsync(Expression<Func<Users, bool>> predicate = null)
        {
            return await _repository.CountAsync(predicate);
        }

        public async Task<bool> ExistsAsync(Expression<Func<Users, bool>> predicate)
        {
            return await _repository.ExistsAsync(predicate);
        }
    }
}

