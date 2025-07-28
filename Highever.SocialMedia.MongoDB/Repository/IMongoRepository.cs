using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GeoJsonObjectModel;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Highever.SocialMedia.MongoDB
{
    /// <summary>
    /// 定义通用的 MongoDB 数据操作接口
    /// </summary>
    /// <typeparam name="T">实体类型</typeparam>
    public interface IMongoRepository<T> where T : class
    {
        #region 基本 CRUD

        /// <summary>
        /// 插入单个文档
        /// </summary>
        /// <param name="entity">要插入的文档</param>
        Task InsertAsync(T entity);

        /// <summary>
        /// 插入多个文档
        /// </summary>
        /// <param name="entities">要插入的文档集合</param>
        Task InsertManyAsync(IEnumerable<T> entities);

        /// <summary>
        /// 按条件查询返回多条记录
        /// </summary>
        /// <param name="filter">查询条件表达式</param>
        /// <returns>符合条件的文档集合</returns>
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> filter);

        /// <summary>
        /// 按条件查询返回一条记录
        /// </summary>
        /// <param name="filter">查询条件表达式</param>
        /// <returns>符合条件的第一条文档</returns>
        Task<T> FindOneAsync(Expression<Func<T, bool>> filter);

        /// <summary>
        /// 替换更新整个文档
        /// </summary>
        /// <param name="filter">查询条件表达式</param>
        /// <param name="entity">替换的文档对象</param>
        Task UpdateAsync(Expression<Func<T, bool>> filter, T entity);

        /// <summary>
        /// 根据条件部分字段更新
        /// </summary>
        /// <param name="filter">查询条件表达式</param>
        /// <param name="updateDefinition">更新定义</param>
        /// <returns>是否更新成功</returns>
        Task<bool> UpdateFieldsAsync(Expression<Func<T, bool>> filter, UpdateDefinition<T> updateDefinition);

        /// <summary>
        /// 删除单个文档
        /// </summary>
        /// <param name="filter">查询条件表达式</param>
        Task DeleteAsync(Expression<Func<T, bool>> filter);

        /// <summary>
        /// 删除多个文档
        /// </summary>
        /// <param name="filter">查询条件表达式</param>
        Task DeleteManyAsync(Expression<Func<T, bool>> filter);

        #endregion

        #region 聚合操作

        /// <summary>
        /// 执行聚合查询
        /// </summary>
        /// <typeparam name="TResult">结果类型</typeparam>
        /// <param name="pipeline">聚合管道</param>
        /// <returns>聚合结果集合</returns>
        Task<IEnumerable<TResult>> AggregateAsync<TResult>(PipelineDefinition<T, TResult> pipeline);

        #endregion

        #region 分页与计数

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="filter">查询条件表达式</param>
        /// <param name="pageIndex">页码，从 1 开始</param>
        /// <param name="pageSize">每页大小</param>
        /// <param name="sortField">排序字段表达式</param>
        /// <param name="ascending">是否升序，默认为 true</param>
        /// <returns>分页数据和总记录数</returns>
        Task<(IEnumerable<T> Items, long TotalCount)> FindPagedAsync(
            Expression<Func<T, bool>> filter,
            int pageIndex,
            int pageSize,
            Expression<Func<T, object>> sortField = null,
            bool ascending = true);

        /// <summary>
        /// 获取符合条件的文档数量
        /// </summary>
        /// <param name="filter">查询条件表达式</param>
        /// <returns>符合条件的文档数量</returns>
        Task<long> CountAsync(Expression<Func<T, bool>> filter);

        #endregion

        #region 高级查询

        /// <summary>
        /// 文本搜索
        /// </summary>
        /// <param name="searchTerm">搜索关键字</param>
        /// <returns>符合条件的文档集合</returns>
        Task<IEnumerable<T>> TextSearchAsync(string searchTerm);

        /// <summary>
        /// 返回指定字段的唯一值列表
        /// </summary>
        /// <typeparam name="TField">字段类型</typeparam>
        /// <param name="field">字段选择器</param>
        /// <param name="filter">查询条件表达式</param>
        /// <returns>字段唯一值列表</returns>
        Task<IEnumerable<TField>> DistinctAsync<TField>(Expression<Func<T, TField>> field, Expression<Func<T, bool>> filter);

        /// <summary>
        /// 查询并返回部分字段（投影查询）
        /// </summary>
        /// <typeparam name="TResult">投影结果类型</typeparam>
        /// <param name="filter">查询条件表达式</param>
        /// <param name="projection">投影表达式</param>
        /// <returns>投影后的结果集合</returns>
        Task<IEnumerable<TResult>> FindWithProjectionAsync<TResult>(
            Expression<Func<T, bool>> filter,
            Expression<Func<T, TResult>> projection);

        #endregion

        #region 地理空间查询

        /// <summary>
        /// 基于地理位置的附近查询
        /// </summary>
        /// <param name="longitude">经度</param>
        /// <param name="latitude">纬度</param>
        /// <param name="maxDistanceInMeters">最大距离（米）</param>
        /// <returns>符合条件的文档集合</returns>
        Task<IEnumerable<T>> GeoNearAsync(double longitude, double latitude, double maxDistanceInMeters);

        #endregion

        #region 索引管理

        /// <summary>
        /// 创建索引
        /// </summary>
        /// <param name="field">字段选择器</param>
        /// <param name="isUnique">是否唯一索引</param>
        Task CreateIndexAsync(Expression<Func<T, object>> field, bool isUnique = false);

        /// <summary>
        /// 列出所有索引
        /// </summary>
        /// <returns>索引信息列表</returns>
        Task<List<BsonDocument>> ListIndexesAsync();

        #endregion

        #region 事务支持

        /// <summary>
        /// 使用事务执行操作
        /// </summary>
        /// <param name="operations">事务内操作方法</param>
        Task ExecuteTransactionAsync(Func<IClientSessionHandle, Task> operations);

        #endregion

        #region 批量操作

        /// <summary>
        /// 批量插入数据
        /// </summary>
        /// <param name="entities">要插入的数据集合</param>
        /// <param name="isOrdered">是否按顺序插入，默认为 true。如设置为 false，则忽略中途错误继续插入。</param>
        Task BulkInsertAsync(IEnumerable<T> entities, bool isOrdered = true);

        /// <summary>
        /// 批量更新数据
        /// </summary>
        /// <param name="filter">筛选条件</param>
        /// <param name="updateDefinition">更新定义</param>
        /// <returns>被更新的文档数量</returns>
        Task<long> BulkUpdateAsync(Expression<Func<T, bool>> filter, UpdateDefinition<T> updateDefinition);

        /// <summary>
        /// 批量删除数据
        /// </summary>
        /// <param name="filter">筛选条件</param>
        /// <returns>被删除的文档数量</returns>
        Task<long> BulkDeleteAsync(Expression<Func<T, bool>> filter);

        #endregion
    }
}
