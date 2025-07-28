using Highever.SocialMedia.Application.Contracts;
using Highever.SocialMedia.Common;
using Highever.SocialMedia.Domain;
using Highever.SocialMedia.Domain.Entity;
using Highever.SocialMedia.SqlSugar;
using NPOI.SS.Formula.Functions;
using SQLBuilder.Core.Extensions;
using System.Linq.Expressions;

namespace Highever.SocialMedia.Application
{
    public class ProductPropertyRequestRecordService : IProductPropertyRequestRecordService
    {
        public readonly ISqlSugarRepository<ProductPropertyRequestRecord> _repository;

        public ProductPropertyRequestRecordService(ISqlSugarRepository<ProductPropertyRequestRecord> repository)
        {
            _repository = repository;
        }
        public async Task<int> CreateAsync(ProductPropertyRequestRecord input)
        {
            return await _repository.InsertEntityAsync(input);
        }
        public async Task<int> CreateAsync(List<ProductPropertyRequestRecord> input)
        {
            return await _repository.BulkInsertAsync(input);
        }  
        
        public async Task<int> DeleteAsync(List<ProductPropertyRequestRecord> input)
        {
            return await _repository.BulkDeleteAsync(input);
        }
        public async Task<int> DeleteAsync(Expression<Func<ProductPropertyRequestRecord, bool>>? predicate = null)
        {
            return await _repository.BulkDeleteAsync(predicate);
        }
        public async Task<int> UpdateAsync(List<ProductPropertyRequestRecord> input)
        {
            return await _repository.BulkUpdateAsync(input);
        }

        public async Task<List<ProductPropertyRequestRecord>> GetQueryListAsync(Expression<Func<ProductPropertyRequestRecord, bool>>? predicate = null)
        {
            return await _repository.QueryListAsync(predicate: predicate);
        }
    }
}
