using Highever.SocialMedia.Common;
using Highever.SocialMedia.Domain;
using Highever.SocialMedia.Domain.Entity;
using NPOI.SS.Formula.Functions;
using System.Linq.Expressions;

namespace Highever.SocialMedia.Application.Contracts
{
    /// <summary>
    /// DistributionProducts 的应用服务接口
    /// </summary>
    public interface IProductPropertyRequestRecordService : ITransientDependency
    {
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public Task<int> CreateAsync(ProductPropertyRequestRecord input);
        public Task<int> CreateAsync(List<ProductPropertyRequestRecord> inputs);  
        Task<int> DeleteAsync(Expression<Func<ProductPropertyRequestRecord, bool>> predicate);
        Task<List<ProductPropertyRequestRecord>> GetQueryListAsync(Expression<Func<ProductPropertyRequestRecord, bool>> predicate);
        Task<(IEnumerable<ProductPropertyRequestRecord> Items, long TotalCount)> GetQueryPageListAsync(Expression<Func<ProductPropertyRequestRecord, bool>> predicate, int pageIndex,
            int pageSize,
            Expression<Func<ProductPropertyRequestRecord, object>> sortField = null,
            bool ascending = true);
    }

}
