using Highever.SocialMedia.Common;
using SqlSugar;

namespace Highever.SocialMedia.SqlSugar
{
    public class SqlSugarDBContext : ISqlSugarDBContext
    {
        private readonly SqlSugarScope _db;

        public SqlSugarDBContext(string masterConnectionString, List<SlaveConnectionConfig> slaveConnections, INLogger _nLogger)
        {
            if (string.IsNullOrWhiteSpace(masterConnectionString))
                throw new ArgumentException("主库连接字符串不能为空！");

            _db = new SqlSugarScope(new ConnectionConfig
            {
                ConnectionString = masterConnectionString,
                DbType = DbType.MySql,
                IsAutoCloseConnection = true,
                ConfigureExternalServices = new ConfigureExternalServices()
                {
                    // 添加内存缓存配置
                },
                MoreSettings = new ConnMoreSettings
                {
                    IsWithNoLockQuery = true, 
                    IsAutoRemoveDataCache = true
                },
                SlaveConnectionConfigs = slaveConnections
            },
            db =>
            {
                db.Ado.IsDisableMasterSlaveSeparation = false;
                
                // 优化日志记录
                db.Aop.OnLogExecuting = (sql, pars) =>
                {
                    _nLogger.Info($"SQL: {sql}, Parameters: {string.Join(",", pars?.Select(p => $"{p.ParameterName}={p.Value}") ?? new string[0])}");
                };
                
                // 添加错误处理
                db.Aop.OnError = (exp) =>
                {
                    _nLogger.Error($"SQL Error: {exp.Message}");
                };
            });
        }
        public void Dispose()
        {
            _db?.Dispose();
        }
        public ISqlSugarClient Db => _db;

        #region Transaction 
        public void BeginTran() => _db.Ado.BeginTran();
        public void CommitTran() => _db.Ado.CommitTran();
        public void RollbackTran() => _db.Ado.RollbackTran();
        #endregion

        #region Bulk Operations
        public async Task<int> BulkInsertAsync<T>(IEnumerable<T> entities) where T : class, new()
        {
            if (entities == null || !entities.Any())
            { 
                return 0;
            }

            return await _db.Insertable(entities.ToList()).ExecuteCommandAsync();
        }

        public async Task<int> BulkUpdateAsync<T>(IEnumerable<T> entities) where T : class, new()
        {
            if (entities == null || !entities.Any())
            { 
                return 0;
            }

            return await _db.Updateable(entities.ToList()).ExecuteCommandAsync();
        }

        public async Task<int> BulkDeleteAsync<T>(IEnumerable<T> entities) where T : class, new()
        {
            if (entities == null || !entities.Any())
            {
                return 0;
            }

            return await _db.Deleteable(entities.ToList()).ExecuteCommandAsync();
        }

        #endregion
    }
}
