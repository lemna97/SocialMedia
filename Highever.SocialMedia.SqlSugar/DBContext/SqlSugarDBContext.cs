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
                //主库
                ConnectionString = masterConnectionString,
                DbType = DbType.MySql,
                //是否自动关闭链接
                IsAutoCloseConnection = true,
                ConfigureExternalServices = new ConfigureExternalServices()
                {
                      // 内存缓存
                },
                MoreSettings = new ConnMoreSettings
                {
                    IsWithNoLockQuery = true // 默认查询加 NOLOCK
                },
                SlaveConnectionConfigs = slaveConnections
            },
             db =>
             {
                 // 所有读写操作都走主库true
                 db.Ado.IsDisableMasterSlaveSeparation = false; 

                 // SQL 日志记录
                 db.Aop.OnLogExecuting = (sql, pars) =>
                 {
                     Console.WriteLine($"SQL: {sql}");
                 };
                 // 替换日志记录方式
                 db.Aop.OnLogExecuting = (sql, pars) =>
                 {
                     _nLogger.Info($"SQL: {sql}");
                 };
                 // 实体映射：固定表明配置
                 db.MappingTables.Add("YourEntity", $"YourEntity_2023");
             });
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
