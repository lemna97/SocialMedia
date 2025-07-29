using Highever.SocialMedia.Application.Contracts;
using Highever.SocialMedia.Common;
using Highever.SocialMedia.Domain.Entity;
using Highever.SocialMedia.Domain.Repository;
using System.Linq.Expressions;

namespace Highever.SocialMedia.Application.Services.System
{
    public class PermissionsService : IPermissionsService
    {
        private readonly IRepository<Permissions> _repository;

        public PermissionsService(IRepository<Permissions> repository)
        {
            _repository = repository;
        }

        public async Task<int> CreateAsync(Permissions input)
        {
            return await _repository.InsertAsync(input);
        }

        public async Task<int> CreateAsync(List<Permissions> input)
        {
            return await _repository.InsertRangeAsync(input);
        }

        public async Task<int> UpdateAsync(Permissions input)
        {
            return await _repository.UpdateAsync(input);
        }

        public async Task<int> UpdateAsync(List<Permissions> input)
        {
            return await _repository.UpdateRangeAsync(input);
        }

        public async Task<int> DeleteAsync(Expression<Func<Permissions, bool>> predicate)
        {
            return await _repository.DeleteAsync(predicate);
        }

        public async Task<Permissions> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<Permissions?> FirstOrDefaultAsync(Expression<Func<Permissions, bool>> predicate)
        {
            return await _repository.FirstOrDefaultAsync(predicate);
        }

        public async Task<List<Permissions>> GetQueryListAsync(Expression<Func<Permissions, bool>> predicate = null)
        {
            return await _repository.GetListAsync(predicate);
        }

        public async Task<PagedResult<Permissions>> GetPagedListAsync(
            Expression<Func<Permissions, bool>> predicate = null,
            int pageIndex = 1,
            int pageSize = 20,
            Expression<Func<Permissions, object>> orderBy = null,
            bool ascending = true)
        {
            return await _repository.GetPagedListAsync(predicate, pageIndex, pageSize, orderBy, ascending);
        }

        public async Task<int> CountAsync(Expression<Func<Permissions, bool>> predicate = null)
        {
            return await _repository.CountAsync(predicate);
        }

        public async Task<bool> ExistsAsync(Expression<Func<Permissions, bool>> predicate)
        {
            return await _repository.ExistsAsync(predicate);
        }
    }
}
