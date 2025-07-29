using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SqlSugar;
using Highever.SocialMedia.Common;
using Highever.SocialMedia.Domain.Repository;

namespace Highever.SocialMedia.SqlSugar
{
    public static class DependencyInjectionExtensions
    {
        /// <summary>
        /// 注册 SqlSugar 服务
        /// </summary>
        public static IServiceCollection AddSqlSugar(this IServiceCollection services)
        {
            // 注册 ISqlSugarDBContext 接口对应的上下文实现
            services.AddScoped<ISqlSugarDBContext>(provider =>
            {
                var configuration = provider.GetRequiredService<IConfiguration>();
                var logger = provider.GetRequiredService<INLogger>();
                
                // 主库连接字符串
                string masterConnectionString = configuration.GetConnectionString("Mysql") 
                    ?? throw new InvalidOperationException("未找到主库连接字符串 'Mysql'");
                
                // 从库连接配置
                List<SlaveConnectionConfig> slaveConnections = new List<SlaveConnectionConfig>();
                
                // 从库1配置
                var slave1Connection = configuration.GetConnectionString("Mysql1");
                if (!string.IsNullOrEmpty(slave1Connection))
                {
                    slaveConnections.Add(new SlaveConnectionConfig
                    {
                        HitRate = 10, // 命中率10%
                        ConnectionString = slave1Connection
                    });
                }
                
                // 从库2配置
                var slave2Connection = configuration.GetConnectionString("Mysql2");
                if (!string.IsNullOrEmpty(slave2Connection))
                {
                    slaveConnections.Add(new SlaveConnectionConfig
                    {
                        HitRate = 90, // 命中率90%
                        ConnectionString = slave2Connection
                    });
                }
                
                return new SqlSugarDBContext(masterConnectionString, slaveConnections, logger);
            });
             
            // 注册 ISqlSugarClient
            services.AddScoped(provider =>
            {
                var context = provider.GetRequiredService<ISqlSugarDBContext>();
                return context.Db;
            });

            services.AddScoped(typeof(IRepository<>), typeof(SqlSugarRepository<>));

            return services;
        }
    }
}
