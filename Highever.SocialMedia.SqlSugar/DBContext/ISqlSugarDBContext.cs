using SqlSugar;

namespace Highever.SocialMedia.SqlSugar
{
    public interface ISqlSugarDBContext: IDisposable
    {
        public ISqlSugarClient Db { get; }
        public void BeginTran();
        public void CommitTran();
        public void RollbackTran();

        // 批量操作
        public Task<int> BulkInsertAsync<T>(IEnumerable<T> entities) where T : class, new();
        public Task<int> BulkUpdateAsync<T>(IEnumerable<T> entities) where T : class, new();
        public Task<int> BulkDeleteAsync<T>(IEnumerable<T> entities) where T : class, new();
    }
}
