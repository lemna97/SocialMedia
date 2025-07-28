using Highever.SocialMedia.Common;
using Highever.SocialMedia.Domain;
using Highever.SocialMedia.Domain.Entity;
using NPOI.SS.Formula.Functions;
using System.Linq.Expressions;

namespace Highever.SocialMedia.Application.Contracts
{
    /// <summary>
    /// ICSVTestService 的应用服务接口
    /// </summary>
    public interface ICSVTestService : ITransientDependency
    {
        Task<int> CreateAsync(CSVTest input);
        Task<int> CreateAsync(List<CSVTest> input);
        Task<int> DeleteManyAsync(Expression<Func<CSVTest, bool>> predicate);
        Task<CSVTest> FindOneAsync(Expression<Func<CSVTest, bool>> predicate);
        Task<List<CSVTest>> GetQueryListAsync(Expression<Func<CSVTest, bool>> predicate);
        Task<int> UpdateAsync(Expression<Func<CSVTest, bool>> filter, CSVTest entity);
    }

}
