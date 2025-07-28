using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Driver.GeoJsonObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using static Dapper.SqlMapper;
using Highever.SocialMedia.Common;

namespace Highever.SocialMedia.MongoDB
{
    public class MongoRepository<T> : IMongoRepository<T> where T : class
    {
        private readonly IMongoDBContext _context;
        private readonly IMongoCollection<T> _collection; 

        /// <summary>
        /// 构造函数，初始化数据库上下文和集合
        /// </summary>
        /// <param name="context">MongoDB 上下文</param> 
        public MongoRepository(IMongoDBContext context)
        {
            // 1. 拿到 “表/集合” 名
            var collName = EnumException.GetTableName<T>();
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _collection = _context.GetCollection<T>(collName) ?? throw new ArgumentNullException(nameof(collName));
        }

        #region 基本 CRUD

        /// <summary>
        /// 插入单个文档
        /// </summary>
        public async Task InsertAsync(T entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            await _collection.InsertOneAsync(entity);
        }

        /// <summary>
        /// 插入多个文档
        /// </summary>
        public async Task InsertManyAsync(IEnumerable<T> entities)
        {
            if (entities == null || !entities.Any()) throw new ArgumentException("The entities collection cannot be empty.");
            await _collection.InsertManyAsync(entities);
        }

        /// <summary>
        /// 按条件查询返回多条记录
        /// </summary>
        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> filter)
        {
            return await _collection.Find(filter).ToListAsync();
        }

        /// <summary>
        /// 按条件查询返回一条记录
        /// </summary>
        public async Task<T> FindOneAsync(Expression<Func<T, bool>> filter)
        {
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }

        /// <summary>
        /// 替换更新整个文档
        /// </summary>
        public async Task UpdateAsync(Expression<Func<T, bool>> filter, T entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            await _collection.ReplaceOneAsync(filter, entity);
        }

        /// <summary>
        /// 根据条件部分字段更新
        /// </summary>
        public async Task<bool> UpdateFieldsAsync(Expression<Func<T, bool>> filter, UpdateDefinition<T> updateDefinition)
        {
            if (updateDefinition == null) throw new ArgumentNullException(nameof(updateDefinition));
            var result = await _collection.UpdateOneAsync(filter, updateDefinition);
            return result.ModifiedCount > 0;
        }

        /// <summary>
        /// 删除单个文档
        /// </summary>
        public async Task DeleteAsync(Expression<Func<T, bool>> filter)
        {
            await _collection.DeleteOneAsync(filter);
        }

        /// <summary>
        /// 删除多个文档
        /// </summary>
        public async Task DeleteManyAsync(Expression<Func<T, bool>> filter)
        {
            await _collection.DeleteManyAsync(filter);
        }

        #endregion

        #region 聚合操作

        /// <summary>
        /// 执行聚合查询
        /// </summary>
        public async Task<IEnumerable<TResult>> AggregateAsync<TResult>(PipelineDefinition<T, TResult> pipeline)
        {
            if (pipeline == null) throw new ArgumentNullException(nameof(pipeline));
            return await _collection.Aggregate(pipeline).ToListAsync();
        }

        #endregion

        #region 分页与计数

        /// <summary>
        /// 分页查询
        /// </summary>
        public async Task<(IEnumerable<T> Items, long TotalCount)> FindPagedAsync(
            Expression<Func<T, bool>> filter,
            int pageIndex,
            int pageSize,
            Expression<Func<T, object>> sortField = null,
            bool ascending = true)
        {
            if (pageIndex < 1) throw new ArgumentOutOfRangeException(nameof(pageIndex), "Page index must be greater than or equal to 1.");
            if (pageSize < 1) throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be greater than or equal to 1.");

            var query = _collection.Find(filter);

            if (sortField != null)
            {
                query = ascending ? query.SortBy(sortField) : query.SortByDescending(sortField);
            }

            var totalCount = await query.CountDocumentsAsync();
            var items = await query.Skip((pageIndex - 1) * pageSize).Limit(pageSize).ToListAsync();

            return (items, totalCount);
        }

        /// <summary>
        /// 获取符合条件的文档数量
        /// </summary>
        public async Task<long> CountAsync(Expression<Func<T, bool>> filter)
        {
            return await _collection.CountDocumentsAsync(filter);
        }

        #endregion

        #region 高级查询

        /// <summary>
        /// 文本搜索
        /// </summary>
        public async Task<IEnumerable<T>> TextSearchAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm)) throw new ArgumentException("Search term cannot be null or whitespace.");
            var filter = Builders<T>.Filter.Text(searchTerm);
            return await _collection.Find(filter).ToListAsync();
        }

        /// <summary>
        /// 返回指定字段的唯一值列表
        /// </summary>
        public async Task<IEnumerable<TField>> DistinctAsync<TField>(Expression<Func<T, TField>> field, Expression<Func<T, bool>> filter)
        {
            return await _collection.Distinct(field, filter).ToListAsync();
        }

        /// <summary>
        /// 查询并返回部分字段
        /// </summary>
        public async Task<IEnumerable<TResult>> FindWithProjectionAsync<TResult>(
            Expression<Func<T, bool>> filter,
            Expression<Func<T, TResult>> projection)
        {
            return await _collection.Find(filter).Project(projection).ToListAsync();
        }

        #endregion

        #region 地理空间查询

        /// <summary>
        /// 基于地理位置的附近查询
        /// </summary>
        public async Task<IEnumerable<T>> GeoNearAsync(double longitude, double latitude, double maxDistanceInMeters)
        {
            var filter = Builders<T>.Filter.NearSphere(
                "Location",
                new GeoJsonPoint<GeoJson2DCoordinates>(new GeoJson2DCoordinates(longitude, latitude)),
                maxDistance: maxDistanceInMeters
            );

            return await _collection.Find(filter).ToListAsync();
        }

        #endregion

        #region 索引管理

        /// <summary>
        /// 创建索引
        /// </summary>
        public async Task CreateIndexAsync(Expression<Func<T, object>> field, bool isUnique = false)
        {
            var indexKeys = Builders<T>.IndexKeys.Ascending(field);
            var options = new CreateIndexOptions { Unique = isUnique };
            await _collection.Indexes.CreateOneAsync(new CreateIndexModel<T>(indexKeys, options));
        }

        /// <summary>
        /// 列出所有索引
        /// </summary>
        public async Task<List<BsonDocument>> ListIndexesAsync()
        {
            return await _collection.Indexes.List().ToListAsync();
        }

        #endregion

        #region 事务支持

        /// <summary>
        /// 使用事务执行操作
        /// </summary>
        public async Task ExecuteTransactionAsync(Func<IClientSessionHandle, Task> operations)
        {
            using var session = await _context.Client.StartSessionAsync();
            session.StartTransaction();

            try
            {
                await operations(session);
                await session.CommitTransactionAsync();
            }
            catch
            {
                await session.AbortTransactionAsync();
                throw;
            }
        }

        #endregion

        #region 批量操作

        /// <summary>
        /// 批量插入数据
        /// </summary>
        public async Task BulkInsertAsync(IEnumerable<T> entities, bool isOrdered = true)
        {
            if (entities == null || !entities.Any()) throw new ArgumentException("The entities collection cannot be empty.");
            var options = new InsertManyOptions { IsOrdered = isOrdered };
            await _collection.InsertManyAsync(entities, options);
        }

        /// <summary>
        /// 批量更新数据
        /// </summary>
        public async Task<long> BulkUpdateAsync(Expression<Func<T, bool>> filter, UpdateDefinition<T> updateDefinition)
        {
            if (filter == null) throw new ArgumentNullException(nameof(filter));
            if (updateDefinition == null) throw new ArgumentNullException(nameof(updateDefinition));

            var result = await _collection.UpdateManyAsync(filter, updateDefinition);
            return result.ModifiedCount; // 返回修改的文档数
        }

        /// <summary>
        /// 批量删除数据
        /// </summary>
        public async Task<long> BulkDeleteAsync(Expression<Func<T, bool>> filter)
        {
            if (filter == null) throw new ArgumentNullException(nameof(filter));

            var result = await _collection.DeleteManyAsync(filter);
            return result.DeletedCount; // 返回删除的文档数
        }

        #endregion
    }
}
