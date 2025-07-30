using Highever.SocialMedia.Application.Contracts;
using Highever.SocialMedia.Common;
using Highever.SocialMedia.Domain;
using Highever.SocialMedia.Domain.Entity;
using Highever.SocialMedia.Domain.Repository;
using Highever.SocialMedia.MongoDB;
using NPOI.POIFS.Crypt.Dsig;
using NPOI.SS.Formula.Functions;
using SQLBuilder.Core.Extensions;
using System.Linq.Expressions;

namespace Highever.SocialMedia.Application.Services
{
    public class ProductPropertyRequestRecordService : IProductPropertyRequestRecordService
    {
        private readonly IMongoRepository<ProductPropertyRequestRecord> _repository;

        public ProductPropertyRequestRecordService(IMongoRepository<ProductPropertyRequestRecord> repository)
        {
            _repository = repository;
        }
        public async Task<int> CreateAsync(ProductPropertyRequestRecord input)
        {
            await _repository.InsertAsync(input);
            return 1;
        }
        public async Task<int> CreateAsync(List<ProductPropertyRequestRecord> input)
        {
            await _repository.InsertManyAsync(input);
            return 1;
        }

        public async Task<int> DeleteAsync(Expression<Func<ProductPropertyRequestRecord, bool>> predicate)
        {
            await _repository.DeleteManyAsync(predicate);
            return 1;
        }
        
        public async Task<List<ProductPropertyRequestRecord>> GetQueryListAsync(Expression<Func<ProductPropertyRequestRecord, bool>> predicate)
        {
            var data = await _repository.FindAsync(predicate);
            return data.ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="sortField"></param>
        /// <param name="ascending"></param>
        /// <returns></returns>
        public async Task<(IEnumerable<ProductPropertyRequestRecord> Items, long TotalCount)> GetQueryPageListAsync(Expression<Func<ProductPropertyRequestRecord, bool>> predicate, int pageIndex,
            int pageSize,
            Expression<Func<ProductPropertyRequestRecord, object>> sortField = null,
            bool ascending = true)
        {
            IEquatable<ProductPropertyRequestRecord> data; 
            return await _repository.FindPagedAsync(predicate, pageIndex, pageSize, sortField, ascending);
        }
    }
}
