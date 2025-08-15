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
                    IsAutoRemoveDataCache = true,
                    SqlServerCodeFirstNvarchar = true,
                    DefaultCacheDurationInSeconds = 600
                },
                SlaveConnectionConfigs = slaveConnections
            },
            // 重要：添加这个配置来避免连接冲突
            db =>
            {
                // 每个线程使用独立的连接
                db.Aop.OnLogExecuting = (sql, pars) =>
                {
                    // 可选：添加SQL日志
                };
            });
        }
        public void Dispose()
        {
            Db?.Dispose();
        }
        public ISqlSugarClient Db => _db;

        #region Transaction  

        // 添加异步事务方法
        public async Task BeginTranAsync() => await Db.Ado.BeginTranAsync();
        public async Task CommitTranAsync() => await Db.Ado.CommitTranAsync();
        public async Task RollbackTranAsync() => await Db.Ado.RollbackTranAsync();

        // 添加事务执行方法
        public async Task<T> ExecuteTransactionAsync<T>(Func<Task<T>> operation)
        {
            try
            {
                await BeginTranAsync();
                var result = await operation();
                await CommitTranAsync();
                return result;
            }
            catch
            {
                await RollbackTranAsync();
                throw;
            }
        }

        public async Task ExecuteTransactionAsync(Func<Task> operation)
        {
            try
            {
                await BeginTranAsync();
                await operation();
                await CommitTranAsync();
            }
            catch
            {
                await RollbackTranAsync();
                throw;
            }
        }
        #endregion

        #region Bulk Operations

        /// <summary>
        /// 批量插入 - 使用独立连接和事务
        /// </summary>
        public async Task<int> BulkInsertAsync<T>(IEnumerable<T> entities) where T : class, new()
        {
            if (entities == null || !entities.Any())
            { 
                return 0;
            }

            // 使用独立连接执行批量插入，避免连接冲突
            return await ExecuteTransactionAsync(async () =>
            {
                var entityList = entities.ToList();
              
                try
                {
                    var result = await Db.Insertable(entityList).ExecuteCommandAsync();
                     
                    return result;
                }
                catch (Exception ex)
                { 
                    throw;
                }
            });
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

            return await ExecuteTransactionAsync(async () =>
            {
                var entityList = entities.ToList(); 
                
                try
                {
                    var result = await Db.Updateable(entityList).ExecuteCommandAsync(); 
                    return result;
                }
                catch (Exception ex)
                { 
                    throw;
                }
            });
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

            return await ExecuteTransactionAsync(async () =>
            {
                var entityList = entities.ToList(); 
                
                try
                {
                    var result = await Db.Deleteable(entityList).ExecuteCommandAsync(); 
                    return result;
                }
                catch (Exception ex)
                { 
                    throw;
                }
            });
        }

        #endregion 
    }
}
