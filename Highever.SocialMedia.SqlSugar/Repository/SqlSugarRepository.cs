using Highever.SocialMedia.Domain.Repository;
using SqlSugar;
using System.Linq.Expressions;

namespace Highever.SocialMedia.SqlSugar
{
    /// <summary>
    /// SqlSugar 仓储实现 - 实现通用接口
    /// </summary>
    public class SqlSugarRepository<T> : IRepository<T> where T : class, new()
    {
        private readonly ISqlSugarClient _db;

        public SqlSugarRepository(ISqlSugarDBContext dbContext)
        {
            _db = dbContext?.Db ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<T?> GetByIdAsync(object id)
        {
            return await _db.Queryable<T>().InSingleAsync(id);
        }

        public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
        {
            return await _db.Queryable<T>().Where(predicate).FirstAsync();
        }

        public async Task<List<T>> GetListAsync(Expression<Func<T, bool>>? predicate = null)
        {
            var query = _db.Queryable<T>();
            if (predicate != null)
                query = query.Where(predicate);
            return await query.ToListAsync();
        }

        public async Task<PagedResult<T>> GetPagedListAsync(
            Expression<Func<T, bool>>? predicate = null,
            int pageIndex = 1,
            int pageSize = 20,
            Expression<Func<T, object>>? orderBy = null,
            bool ascending = true)
        {
            RefAsync<int> totalCount = 0;
            var query = _db.Queryable<T>();

            if (predicate != null)
                query = query.Where(predicate);

            if (orderBy != null)
                query = ascending ? query.OrderBy(orderBy) : query.OrderBy(orderBy, OrderByType.Desc);

            var items = await query.ToPageListAsync(pageIndex, pageSize, totalCount);

            return new PagedResult<T>
            {
                Items = items,
                TotalCount = totalCount.Value,
                PageIndex = pageIndex,
                PageSize = pageSize
            };
        }

        public async Task<int> InsertAsync(T entity)
        {
            return await _db.Insertable(entity).ExecuteCommandAsync();
        }
        public long InsertBySnowflakeId(T entity)
        {
            return _db.Insertable(entity).ExecuteReturnSnowflakeId();
        }
        public int InsertByIdentity(T entity)
        {
            return _db.Insertable(entity).ExecuteReturnIdentity();
        }   
        public Task<int> InsertByIdentityAsync(T entity)
        {
            return _db.Insertable(entity).ExecuteReturnIdentityAsync();
        }
        public void Insert(T entity)
        {
            _db.Insertable(entity).ExecuteCommand();
        }

        public async Task<int> InsertRangeAsync(IEnumerable<T> entities)
        {
            return await _db.Insertable(entities.ToList()).ExecuteCommandAsync();
        }

        public async Task<int> UpdateAsync(T entity)
        {
            return await _db.Updateable(entity).ExecuteCommandAsync();
        }
        public int Update(T entity)
        {
            return _db.Updateable(entity).ExecuteCommand();
        }

        public async Task<int> UpdateRangeAsync(IEnumerable<T> entities)
        {
            return await _db.Updateable(entities.ToList()).ExecuteCommandAsync();
        }

        public async Task<int> DeleteAsync(T entity)
        {
            return await _db.Deleteable(entity).ExecuteCommandAsync();
        }

        public async Task<int> DeleteRangeAsync(IEnumerable<T> entities)
        {
            return await _db.Deleteable(entities.ToList()).ExecuteCommandAsync();
        }

        public async Task<int> DeleteAsync(Expression<Func<T, bool>> predicate)
        {
            return await _db.Deleteable(predicate).ExecuteCommandAsync();
        }

        public async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
        {
            var query = _db.Queryable<T>();
            if (predicate != null)
                query = query.Where(predicate);
            return await query.CountAsync();
        }

        public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
        {
            return await _db.Queryable<T>().Where(predicate).AnyAsync();
        }


        #region 新增的批量操作方法实现

        /// <summary>
        /// 批量插入
        /// </summary>
        public async Task<int> BulkInsertAsync(IEnumerable<T> entities)
        {
            if (entities == null || !entities.Any())
                return 0;

            return await _db.Insertable(entities.ToList()).ExecuteCommandAsync();
        }

        /// <summary>
        /// 批量更新
        /// </summary>
        public async Task<int> BulkUpdateAsync(IEnumerable<T> entities)
        {
            if (entities == null || !entities.Any())
                return 0;

            return await _db.Updateable(entities.ToList()).ExecuteCommandAsync();
        }

        /// <summary>
        /// 批量删除（根据条件）
        /// </summary>
        public async Task<int> BulkDeleteAsync(Expression<Func<T, bool>> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return await _db.Deleteable<T>().Where(predicate).ExecuteCommandAsync();
        }

        /// <summary>
        /// 查询列表（别名方法，与 GetListAsync 功能相同）
        /// </summary>
        public async Task<List<T>> QueryListAsync(Expression<Func<T, bool>>? predicate = null)
        {
            var query = _db.Queryable<T>();
            if (predicate != null)
                query = query.Where(predicate);
            return await query.ToListAsync();
        }

        #endregion
    }
}
