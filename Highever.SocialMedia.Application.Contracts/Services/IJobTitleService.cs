using Highever.SocialMedia.Common;
using Highever.SocialMedia.Domain;
using Highever.SocialMedia.Domain.Entity;
using NPOI.SS.Formula.Functions;
using System.Linq.Expressions;

namespace Highever.SocialMedia.Application.Contracts
{
    /// <summary>
    /// IJobTitleService 的应用服务接口
    /// </summary>
    public interface IJobTitleService : ITransientDependency
    {
        Task<int> CreateAsync(JobTitle input);
        Task<int> CreateAsync(List<JobTitle> input);
        Task<int> DeleteManyAsync(Expression<Func<JobTitle, bool>> predicate);
        Task<JobTitle> FindOneAsync(Expression<Func<JobTitle, bool>> predicate);
        Task<List<JobTitle>> GetQueryListAsync(Expression<Func<JobTitle, bool>> predicate);
        Task<int> UpdateAsync(Expression<Func<JobTitle, bool>> filter, JobTitle entity);
    }

}
