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
    public interface IDistributionProductsAppService : ITransientDependency
    {  
        Task<int> CreateAsync(DistributionProductDto input);
        Task<int> CreateAsync(DistributionProducts input);
        Task<DistributionProductDto> GetByIdAsync(int id);
        Task<List<DistributionProducts>> GetQueryListAsync(Expression<Func<DistributionProducts, bool>>? predicate = null);
    }

}
