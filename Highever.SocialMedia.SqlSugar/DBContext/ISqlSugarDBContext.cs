using SqlSugar;

namespace Highever.SocialMedia.SqlSugar
{
    public interface ISqlSugarDBContext: IDisposable
    {
        public ISqlSugarClient Db { get; } 
        
        // 添加异步事务方法
        public Task BeginTranAsync();
        public Task CommitTranAsync();
        public Task RollbackTranAsync();
        public Task<T> ExecuteTransactionAsync<T>(Func<Task<T>> operation);
        public Task ExecuteTransactionAsync(Func<Task> operation);

        // 批量操作
        public Task<int> BulkInsertAsync<T>(IEnumerable<T> entities) where T : class, new();
        public Task<int> BulkUpdateAsync<T>(IEnumerable<T> entities) where T : class, new();
        public Task<int> BulkDeleteAsync<T>(IEnumerable<T> entities) where T : class, new();
        
    }
}
