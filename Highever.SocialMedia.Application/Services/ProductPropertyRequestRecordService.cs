using Highever.SocialMedia.Application.Contracts;
using Highever.SocialMedia.Common;
using Highever.SocialMedia.Domain;
using Highever.SocialMedia.Domain.Entity;
using Highever.SocialMedia.Domain.Repository;
using NPOI.SS.Formula.Functions;
using SQLBuilder.Core.Extensions;
using System.Linq.Expressions;

namespace Highever.SocialMedia.Application.Services
{
    public class ProductPropertyRequestRecordService : IProductPropertyRequestRecordService
    {
        private readonly IRepository<ProductPropertyRequestRecord> _repository;

        public ProductPropertyRequestRecordService(IRepository<ProductPropertyRequestRecord> repository)
        {
            _repository = repository;
        }
        public async Task<int> CreateAsync(ProductPropertyRequestRecord input)
        {
            return await _repository.InsertAsync(input);
        }
        public async Task<int> CreateAsync(List<ProductPropertyRequestRecord> input)
        {
            return await _repository.InsertRangeAsync(input);
        }  
        
        public async Task<int> DeleteAsync(List<ProductPropertyRequestRecord> input)
        {
            return await _repository.DeleteRangeAsync(input);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public async Task<int> DeleteAsync(Expression<Func<ProductPropertyRequestRecord, bool>> predicate)
        {
            return await _repository.BulkDeleteAsync(predicate);
        }
        public async Task<int> UpdateAsync(List<ProductPropertyRequestRecord> input)
        {
            return await _repository.BulkUpdateAsync(input);
        }

        public async Task<List<ProductPropertyRequestRecord>> GetQueryListAsync(Expression<Func<ProductPropertyRequestRecord, bool>> predicate)
        {
            return await _repository.QueryListAsync(predicate: predicate);
        }
    }
}
