using Highever.SocialMedia.Common;
using System.Linq.Expressions;

namespace Highever.SocialMedia.SqlSugar
{
    /// <summary>
    /// 仓储，需要就扩展
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ISqlSugarRepository<T> where T : class, new()
    {
        Task<T> QuerySingleAsync(Expression<Func<T, bool>> predicate);
        Task<List<T>> QueryListAsync(Expression<Func<T, bool>>? predicate = null, Expression<Func<T, object>>? orderBy = null, bool isAsc = true);
        Task<PageResult<T>> QueryPageAsync(Expression<Func<T, bool>> predicate, int pageIndex, int pageSize, Expression<Func<T, object>> orderBy, bool isAsc = true);

        Task<int> BulkInsertAsync(List<T> entities);

        Task<bool> UpdatePartialAsync(T entity, params string[] updateColumns);
        Task<int> BulkDeleteAsync(List<T> entities);
        Task<int> InsertEntityAsync(T entity);
        Task<int> BulkUpdateAsync(List<T> entities);
        Task<int> BulkDeleteAsync(Expression<Func<T, bool>>? entities);
    }
}
