using MongoDB.Driver;

namespace Highever.SocialMedia.MongoDB
{
    public interface IMongoDBContext
    {
        /// <summary>
        /// 获取 MongoDB 客户端
        /// </summary>
        IMongoClient Client { get; }

        /// <summary>
        /// 获取数据库实例
        /// </summary>
        IMongoDatabase Database { get; }
        /// <summary>
        /// 获取 MongoDB 集合
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="collectionName">集合名称</param>
        /// <returns>MongoDB 集合</returns>
        IMongoCollection<T> GetCollection<T>(string collectionName) where T : class;
    }
}
