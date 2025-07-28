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
    public class CSVTestService : ICSVTestService
    {
        public readonly IMongoRepository<CSVTest> _repository;

        public CSVTestService(IMongoRepository<CSVTest> repository)
        {
            _repository = repository;
        }
        public async Task<int> CreateAsync(CSVTest input)
        {
            await _repository.InsertAsync(input);
            return 1;
        }
        public async Task<int> CreateAsync(List<CSVTest> input)
        {
            await _repository.InsertManyAsync(input);
            return 1;
        }   
        public async Task<int> UpdateAsync(Expression<Func<CSVTest, bool>> filter, CSVTest entity)
        {
            await _repository.UpdateAsync(filter,entity);
            return 1;
        }
        public async Task<int> DeleteManyAsync(Expression<Func<CSVTest, bool>> predicate)
        {
            await _repository.DeleteManyAsync(predicate);
            return 1;
        }
        public async Task<List<CSVTest>> GetQueryListAsync(Expression<Func<CSVTest, bool>> predicate = null)
        {
            var list = await _repository.FindAsync(predicate);
            return list.ToList();
        }
        public async Task<CSVTest> FindOneAsync(Expression<Func<CSVTest, bool>> predicate)
        {
            return await _repository.FindOneAsync(predicate);
        }
    }
}
