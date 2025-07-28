using Highever.SocialMedia.Application.Contracts;
using Highever.SocialMedia.Common;
using Highever.SocialMedia.Domain;
using Highever.SocialMedia.Domain.Entity;
using Highever.SocialMedia.MongoDB;
using Highever.SocialMedia.SqlSugar;
using NPOI.SS.Formula.Functions;
using SQLBuilder.Core.Extensions;
using System.Linq.Expressions;

namespace Highever.SocialMedia.Application
{
    public class JobTitleService : IJobTitleService
    {
        public readonly IMongoRepository<JobTitle> _repository;

        public JobTitleService(IMongoRepository<JobTitle> repository)
        {
            _repository = repository;
        }
        public async Task<int> CreateAsync(JobTitle input)
        {
            await _repository.InsertAsync(input);
            return 1;
        }
        public async Task<int> CreateAsync(List<JobTitle> input)
        {
            await _repository.InsertManyAsync(input);
            return 1;
        }   
        public async Task<int> UpdateAsync(Expression<Func<JobTitle, bool>> filter, JobTitle entity)
        {
            await _repository.UpdateAsync(filter,entity);
            return 1;
        }
        public async Task<int> DeleteManyAsync(Expression<Func<JobTitle, bool>> predicate)
        {
            await _repository.DeleteManyAsync(predicate);
            return 1;
        }
        public async Task<List<JobTitle>> GetQueryListAsync(Expression<Func<JobTitle, bool>> predicate = null)
        {
            var list = await _repository.FindAsync(predicate);
            return list.ToList();
        }
        public async Task<JobTitle> FindOneAsync(Expression<Func<JobTitle, bool>> predicate)
        {
            return await _repository.FindOneAsync(predicate);
        }
    }
}
