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
    public class JobSeekerService : IJobSeekerService
    {
        public readonly IMongoRepository<JobSeeker> _repository;

        public JobSeekerService(IMongoRepository<JobSeeker> repository)
        {
            _repository = repository;
        }
        public async Task<int> CreateAsync(JobSeeker input)
        {
            await _repository.InsertAsync(input);
            return 1;
        }
        public async Task<int> CreateAsync(List<JobSeeker> input)
        {
            await _repository.InsertManyAsync(input);
            return 1;
        }
        public async Task<int> DeleteManyAsync(Expression<Func<JobSeeker, bool>> predicate)
        {
            await _repository.DeleteManyAsync(predicate);
            return 1;
        }
        public async Task<List<JobSeeker>> GetQueryListAsync(Expression<Func<JobSeeker, bool>> predicate)
        {
            var list = await _repository.FindAsync(predicate);
            return list.ToList();
        }  
        public async Task<JobSeeker> FindOneAsync(Expression<Func<JobSeeker, bool>> predicate)
        { 
            return await _repository.FindOneAsync(predicate);
        }
    }
}
