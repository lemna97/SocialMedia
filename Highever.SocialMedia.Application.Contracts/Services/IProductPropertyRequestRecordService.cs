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
        Task<int> DeleteAsync(List<ProductPropertyRequestRecord> input);
        Task<int> DeleteAsync(Expression<Func<ProductPropertyRequestRecord, bool>>? predicate = null);
        Task<List<ProductPropertyRequestRecord>> GetQueryListAsync(Expression<Func<ProductPropertyRequestRecord, bool>>? predicate = null);
        Task<int> UpdateAsync(List<ProductPropertyRequestRecord> input);
    }

}
