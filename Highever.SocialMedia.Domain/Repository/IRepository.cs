using System.Linq.Expressions;

namespace Highever.SocialMedia.Domain.Repository
{
    /// <summary>
    /// 通用仓储接口 - 不依赖任何 ORM
    /// </summary>
    public interface IRepository<T> where T : class
    {
        // 基础查询
        Task<T?> GetByIdAsync(object id);
        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
        Task<List<T>> GetListAsync(Expression<Func<T, bool>>? predicate = null);

        // 分页查询
        Task<PagedResult<T>> GetPagedListAsync(
            Expression<Func<T, bool>>? predicate = null,
            int pageIndex = 1,
            int pageSize = 20,
            Expression<Func<T, object>>? orderBy = null,
            bool ascending = true);

        // 增删改
        Task<int> InsertAsync(T entity);
        long InsertBySnowflakeId(T entity);
        int InsertByIdentity(T entity);
        void Insert(T entity); 
        Task<int> InsertRangeAsync(IEnumerable<T> entities);
        Task<int> UpdateAsync(T entity);
        int Update(T entity);
        Task<int> UpdateRangeAsync(IEnumerable<T> entities);
        Task<int> DeleteAsync(T entity);
        Task<int> DeleteRangeAsync(IEnumerable<T> entities);
        Task<int> DeleteAsync(Expression<Func<T, bool>> predicate); 
        // 统计
        Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);
        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);

        // 批量操作
        Task<int> BulkInsertAsync(IEnumerable<T> entities);
        Task<int> BulkUpdateAsync(IEnumerable<T> entities);
        Task<int> BulkDeleteAsync(Expression<Func<T, bool>> predicate);
        Task<List<T>> QueryListAsync(Expression<Func<T, bool>>? predicate = null);
        Task<int> InsertByIdentityAsync(T entity);
    }
}
