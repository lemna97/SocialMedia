using Highever.SocialMedia.Common;
using MongoDB.Driver;

namespace Highever.SocialMedia.MongoDB
{
    /// <summary>
    /// 可以把数据库的名字提取，支持多库
    /// </summary>
    public class MongoDBContext : IMongoDBContext
    {
        private readonly IMongoClient _client;
        private readonly IMongoDatabase _database;

        public MongoDBContext(string connectionString, string databaseName, INLogger logger)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("MongoDB 连接字符串不能为空！");
            if (string.IsNullOrWhiteSpace(databaseName))
                throw new ArgumentException("MongoDB 数据库名称不能为空！");

            _client = new MongoClient(connectionString);
            _database = _client.GetDatabase(databaseName);

            logger?.Info($"Connected to MongoDB: {databaseName}");
        }

        public IMongoClient Client => _client;

        public IMongoDatabase Database => _database;

        public IMongoCollection<T> GetCollection<T>(string collectionName) where T : class
        {
            if (string.IsNullOrWhiteSpace(collectionName))
                throw new ArgumentException("集合名称不能为空！");

            return _database.GetCollection<T>(collectionName);
        }
    }
}
