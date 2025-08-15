using Highever.SocialMedia.Domain.Repository;
using SqlSugar;
using System.Linq.Expressions;
using System.Net.Http;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace Highever.SocialMedia.SqlSugar
{
    /// <summary>
    /// SqlSugar 仓储实现 - 实现通用接口
    /// </summary>
    public class SqlSugarRepository<T> : IRepository<T> where T : class, new()
    {
        private readonly ISqlSugarDBContext _dbContext; 

        public SqlSugarRepository(ISqlSugarDBContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext)); 
        } 

        public async Task<T?> GetByIdAsync(object id)
        {
            return await _dbContext.Db.Queryable<T>().InSingleAsync(id);
        }

        public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbContext.Db.Queryable<T>().Where(predicate).FirstAsync();
        }

        public async Task<List<T>> GetListAsync(Expression<Func<T, bool>>? predicate = null)
        {
            var query = _dbContext.Db.Queryable<T>();
            if (predicate != null)
                query = query.Where(predicate);
            return await query.ToListAsync();
        }

        public async Task<List<T>> QueryListAsync(Expression<Func<T, bool>>? predicate = null)
        {
            var query = _dbContext.Db.Queryable<T>();
            if (predicate != null)
                query = query.Where(predicate);
            return await query.ToListAsync();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="orderBy"></param>
        /// <param name="ascending"></param>
        /// <returns></returns>
        public async Task<PagedResult<T>> GetPagedListAsync(
            Expression<Func<T, bool>>? predicate = null,
            int pageIndex = 1,
            int pageSize = 20,
            Expression<Func<T, object>>? orderBy = null,
            bool ascending = true)
        {
            RefAsync<int> totalCount = 0;
            var query = _dbContext.Db.Queryable<T>();

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
            return await _dbContext.Db.Insertable(entity).ExecuteCommandAsync();
        }
        public long InsertBySnowflakeId(T entity)
        {
            return _dbContext.Db.Insertable(entity).ExecuteReturnSnowflakeId();
        }
        public int InsertByIdentity(T entity)
        {
            return _dbContext.Db.Insertable(entity).ExecuteReturnIdentity();
        }
        public Task<int> InsertByIdentityAsync(T entity)
        {
            return _dbContext.Db.Insertable(entity).ExecuteReturnIdentityAsync();
        }
        public void Insert(T entity)
        {
            _dbContext.Db.Insertable(entity).ExecuteCommand();
        }
        public async Task<int> InsertAsync(IEnumerable<T> entities)
        {
            if (entities == null || !entities.Any())
                return 0;

            return await _dbContext.Db.Insertable(entities.ToList()).ExecuteCommandAsync();
        }

        public async Task<int> InsertRangeAsync(IEnumerable<T> entities)
        {
            return await _dbContext.Db.Insertable(entities.ToList()).ExecuteCommandAsync();
        }

        public async Task<int> UpdateAsync(T entity)
        {
            return await _dbContext.Db.Updateable(entity).ExecuteCommandAsync();
        }
        public int Update(T entity)
        {
            return _dbContext.Db.Updateable(entity).ExecuteCommand();
        }

        public async Task<int> UpdateAsync(IEnumerable<T> entities)
        {
            if (entities == null || !entities.Any())
                return 0;

            return await _dbContext.Db.Updateable(entities.ToList()).ExecuteCommandAsync();
        }
        public async Task<int> UpdateRangeAsync(IEnumerable<T> entities)
        {
            return await _dbContext.Db.Updateable(entities.ToList()).ExecuteCommandAsync();
        }

        public async Task<int> DeleteAsync(T entity)
        {
            return await _dbContext.Db.Deleteable(entity).ExecuteCommandAsync();
        }

        public async Task<int> DeleteRangeAsync(IEnumerable<T> entities)
        {
            return await _dbContext.Db.Deleteable(entities.ToList()).ExecuteCommandAsync();
        }

        public async Task<int> DeleteAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbContext.Db.Deleteable(predicate).ExecuteCommandAsync();
        }

        public async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
        {
            var query = _dbContext.Db.Queryable<T>();
            if (predicate != null)
                query = query.Where(predicate);
            return await query.CountAsync();
        }

        public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbContext.Db.Queryable<T>().Where(predicate).AnyAsync();
        }


        #region 事务操作 - 按照SqlSugar官方用法

        public async Task ExecuteTransactionAsync(Func<Task> operations)
        {
            await _dbContext.ExecuteTransactionAsync(operations);
        }

        public async Task<TResult> ExecuteTransactionAsync<TResult>(Func<Task<TResult>> operations)
        {
            return await _dbContext.ExecuteTransactionAsync(operations);
        }
        #endregion

        #region 大批量数据 Bulk Operations

        /// <summary>
        /// 批量插入 - 使用独立连接和事务
        /// </summary>
        public async Task<int> BulkInsertAsync<T>(IEnumerable<T> entities) where T : class, new()
        {
            if (entities == null || !entities.Any())
            {
                return 0;
            }
            await _dbContext.BulkInsertAsync(entities);
            return 1;
        }

        /// <summary>
        /// 批量更新 - 使用独立连接和事务
        /// </summary>
        public async Task<int> BulkUpdateAsync<T>(IEnumerable<T> entities) where T : class, new()
        {
            if (entities == null || !entities.Any())
            {
                return 0;
            }
            await _dbContext.BulkUpdateAsync(entities);
            return 1;
        }

        /// <summary>
        /// 批量删除 - 使用独立连接和事务
        /// </summary>
        public async Task<int> BulkDeleteAsync<T>(IEnumerable<T> entities) where T : class, new()
        {
            if (entities == null || !entities.Any())
            {
                return 0;
            }
            await _dbContext.BulkDeleteAsync(entities);
            return 1;
        }

        #endregion
    }
}
