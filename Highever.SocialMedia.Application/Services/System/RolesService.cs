using Highever.SocialMedia.Application.Contracts;
using Highever.SocialMedia.Common;
using Highever.SocialMedia.Domain.Entity;
using Highever.SocialMedia.Domain.Repository;
using System.Linq.Expressions;

namespace Highever.SocialMedia.Application.Services.System
{
    public class RolesService : IRolesService
    {
        private readonly IRepository<Roles> _repository;

        public RolesService(IRepository<Roles> repository)
        {
            _repository = repository;
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

        public async Task<List<Roles>> GetQueryListAsync(Expression<Func<Roles, bool>> predicate = null)
        {
            return await _repository.GetListAsync(predicate);
        }

        public async Task<PagedResult<Roles>> GetPagedListAsync(
            Expression<Func<Roles, bool>> predicate = null,
            int pageIndex = 1,
            int pageSize = 20,
            Expression<Func<Roles, object>> orderBy = null,
            bool ascending = true)
        {
            return await _repository.GetPagedListAsync(predicate, pageIndex, pageSize, orderBy, ascending);
        }

        public async Task<int> CountAsync(Expression<Func<Roles, bool>> predicate = null)
        {
            return await _repository.CountAsync(predicate);
        }

        public async Task<bool> ExistsAsync(Expression<Func<Roles, bool>> predicate)
        {
            return await _repository.ExistsAsync(predicate);
        }
    }
}

