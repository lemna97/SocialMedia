using Highever.SocialMedia.Common;
using SqlSugar;
using System.Linq.Expressions;

namespace Highever.SocialMedia.SqlSugar
{
    /// <summary>
    /// 扩展泛型方法
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SqlSugarRepository<T> : SimpleClient<T>, ISqlSugarRepository<T> where T : class ,new()
    {
        private readonly ISqlSugarClient _db;

        public SqlSugarRepository(ISqlSugarDBContext dbContext)
        {
            if (dbContext == null)
                throw new ArgumentNullException(nameof(dbContext));

            _db = dbContext.Db;
        }

        #region 扩展方法，批量方法，后续通用扩展方法

        /// <summary>
        /// 条件查询单个实体
        /// </summary>
        public async Task<T> QuerySingleAsync(Expression<Func<T, bool>> predicate)
        {
            return await _db.Queryable<T>().Where(predicate).SingleAsync();
        }

        /// <summary>
        /// 条件查询列表
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="orderBy"></param>
        /// <param name="isAsc"></param>
        /// <returns></returns>
        public async Task<List<T>> QueryListAsync(
            Expression<Func<T, bool>>? predicate = null,
            Expression<Func<T, object>>? orderBy = null,
            bool isAsc = true)
        {
            var query = _db.Queryable<T>();

            if (predicate != null)
                query = query.Where(predicate);

            if (orderBy != null)
                query = isAsc ? query.OrderBy(orderBy) : query.OrderBy(orderBy, OrderByType.Desc);

            return await query.ToListAsync();
        }

        /// <summary>
        /// 分页查询
        /// </summary>
        public async Task<PageResult<T>> QueryPageAsync(
            Expression<Func<T, bool>> predicate,
            int pageIndex,
            int pageSize,
            Expression<Func<T, object>> orderBy,
            bool isAsc = true)
        {
            RefAsync<int> totalCount = 0;

            var query = _db.Queryable<T>().Where(predicate);

            query = isAsc ? query.OrderBy(orderBy) : query.OrderBy(orderBy, OrderByType.Desc);

            var items = await query.ToPageListAsync(pageIndex, pageSize, totalCount);

            return new PageResult<T>
            {
                Total = totalCount.Value,
                Items = items
            };
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        public async Task<int> BulkInsertAsync(List<T> entities)
        {
            return await _db.Insertable(entities).ExecuteCommandAsync();
        }
         /// <summary>
        /// 
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        public async Task<int> BulkUpdateAsync(List<T> entities)
        {
            return await _db.Updateable(entities).ExecuteCommandAsync();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<int> InsertEntityAsync(T entity)
        {
            return await base.InsertAsync(entity) ? 1 : 0;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="updateColumns"></param>
        /// <returns></returns>
        public async Task<bool> UpdatePartialAsync(T entity, params string[] updateColumns)
        {
            var result = await _db.Updateable(entity)
                .UpdateColumns(updateColumns)
                .ExecuteCommandAsync();
            return result > 0;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        public async Task<int> BulkDeleteAsync(List<T> entities)
        {
            return await _db.Deleteable(entities).ExecuteCommandAsync();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        public async Task<int> BulkDeleteAsync(Expression<Func<T, bool>>?  entities)
        {
            return await _db.Deleteable(entities).ExecuteCommandAsync();
        }


        #endregion

    }

}
