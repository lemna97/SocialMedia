using Highever.SocialMedia.Common;
using Highever.SocialMedia.Domain;
using Highever.SocialMedia.Domain.Entity;
using NPOI.SS.Formula.Functions;
using System.Linq.Expressions;

namespace Highever.SocialMedia.Application.Contracts
{
    /// <summary>
    /// IJobSeekerService 的应用服务接口
    /// </summary>
    public interface IJobSeekerService : ITransientDependency
    {
        Task<int> CreateAsync(JobSeeker input);
        Task<int> CreateAsync(List<JobSeeker> input);
        Task<int> DeleteManyAsync(Expression<Func<JobSeeker, bool>> predicate);
        Task<JobSeeker> FindOneAsync(Expression<Func<JobSeeker, bool>> predicate);
        Task<List<JobSeeker>> GetQueryListAsync(Expression<Func<JobSeeker, bool>> predicate);
    }

}
