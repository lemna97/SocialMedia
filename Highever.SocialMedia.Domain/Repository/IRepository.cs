using SqlSugar;
using System.Linq.Expressions;

namespace Highever.SocialMedia.Domain.Repository
{
    /// <summary>
    /// 通用仓储接口 - 不依赖任何 ORM
    /// </summary>
    public interface IRepository<T> where T : class, new()
    {
        // 基础 CRUD 操作
        Task<T?> GetByIdAsync(object id);
        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
        Task<List<T>> GetListAsync(Expression<Func<T, bool>>? predicate = null);
        Task<PagedResult<T>> GetPagedListAsync(
            Expression<Func<T, bool>>? predicate = null,
            int pageIndex = 1,
            int pageSize = 20,
            Expression<Func<T, object>>? orderBy = null,
            bool ascending = true);

        Task<int> InsertAsync(T entity);
        long InsertBySnowflakeId(T entity);
        int InsertByIdentity(T entity);
        void Insert(T entity);
        Task<int> InsertRangeAsync(IEnumerable<T> entities);
        Task<int> UpdateAsync(T entity);
        Task<int> UpdateRangeAsync(IEnumerable<T> entities);
        int Update(T entity);
        Task<int> DeleteAsync(T entity);
        Task<int> DeleteRangeAsync(IEnumerable<T> entities);
        Task<int> DeleteAsync(Expression<Func<T, bool>> predicate);
        Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);
        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);

        // 批量操作 
        Task<List<T>> QueryListAsync(Expression<Func<T, bool>>? predicate = null);
        Task<int> InsertByIdentityAsync(T entity); 

        // 事务管理
        Task ExecuteTransactionAsync(Func<Task> operations);
        Task<TResult> ExecuteTransactionAsync<TResult>(Func<Task<TResult>> operations);
        Task<int> InsertAsync(IEnumerable<T> entities);
        Task<int> UpdateAsync(IEnumerable<T> entities);
        Task<int> BulkInsertAsync<T>(IEnumerable<T> entities) where T : class, new();
        Task<int> BulkUpdateAsync<T>(IEnumerable<T> entities) where T : class, new();
        Task<int> BulkDeleteAsync<T>(IEnumerable<T> entities) where T : class, new();
    }
}


