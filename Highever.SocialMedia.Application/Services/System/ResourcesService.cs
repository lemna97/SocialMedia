using Highever.SocialMedia.Application.Contracts;
using Highever.SocialMedia.Common;
using Highever.SocialMedia.Domain.Entity;
using Highever.SocialMedia.Domain.Repository;
using System.Linq.Expressions;

namespace Highever.SocialMedia.Application.Services.System
{
    public class ResourcesService : IResourcesService
    {
        private readonly IRepository<Resources> _repository;

        public ResourcesService(IRepository<Resources> repository)
        {
            _repository = repository;
        }

        public async Task<int> CreateAsync(Resources input)
        {
            return await _repository.InsertAsync(input);
        }

        public async Task<int> CreateAsync(List<Resources> input)
        {
            return await _repository.InsertRangeAsync(input);
        }

        public async Task<int> UpdateAsync(Resources input)
        {
            return await _repository.UpdateAsync(input);
        }

        public async Task<int> UpdateAsync(List<Resources> input)
        {
            return await _repository.UpdateRangeAsync(input);
        }

        public async Task<int> DeleteAsync(Expression<Func<Resources, bool>> predicate)
        {
            return await _repository.DeleteAsync(predicate);
        }

        public async Task<Resources?> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public async Task<Resources?> FirstOrDefaultAsync(Expression<Func<Resources, bool>> predicate)
        {
            return await _repository.FirstOrDefaultAsync(predicate);
        }

        public async Task<List<Resources>> GetQueryListAsync(Expression<Func<Resources, bool>> predicate)
        {
            return await _repository.GetListAsync(predicate);
        }

        public async Task<PagedResult<Resources>> GetPagedListAsync(
            Expression<Func<Resources, bool>> predicate = null,
            int pageIndex = 1,
            int pageSize = 20,
            Expression<Func<Resources, object>> orderBy = null,
            bool ascending = true)
        {
            return await _repository.GetPagedListAsync(predicate, pageIndex, pageSize, orderBy, ascending);
        }

        public async Task<int> CountAsync(Expression<Func<Resources, bool>> predicate = null)
        {
            return await _repository.CountAsync(predicate);
        }

        public async Task<bool> ExistsAsync(Expression<Func<Resources, bool>> predicate)
        {
            return await _repository.ExistsAsync(predicate);
        }
    }
}
