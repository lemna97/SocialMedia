using Highever.SocialMedia.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Highever.SocialMedia.SqlSugar
{
    /// <summary>
    /// SqlSugar 模块化注入
    /// </summary>
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddSqlSugar(this IServiceCollection services)
        {
            #region 检查 INLogger 是否已注册 
            if (!services.Any(service => service.ServiceType == typeof(INLogger)))
            {
                //日志注入
                services.AddScoped<INLogger, NLogAdapter>();
            }
            #endregion 
            // 注入 ISqlSugarDBContext 接口对应的上下文实现
            services.AddScoped<ISqlSugarDBContext>(provider =>
            {
                var configuration = provider.GetRequiredService<IConfiguration>();

                // 主库连接字符串
                string masterConnectionString = configuration.GetConnectionString("Mysql");

                // 从库配置，或者直接从数据中读取
                List<SlaveConnectionConfig> slaveConnections = new List<SlaveConnectionConfig>
                {
                    new SlaveConnectionConfig
                    {
                        HitRate = 10,
                        ConnectionString = configuration.GetConnectionString("Mysql1")
                    },
                    new SlaveConnectionConfig
                    {
                        HitRate = 90,
                        ConnectionString = configuration.GetConnectionString("Mysql2")
                    }
                }; 
                // 日志服务（提前注入）
                var logger = provider.GetRequiredService<INLogger>();

                // 初始化并返回 SqlSugar 上下文
                return new SqlSugarDBContext(masterConnectionString, slaveConnections, logger);
            }); 
            // 注入 SqlSugar 仓储接口
            services.AddScoped(typeof(ISqlSugarRepository<>), typeof(SqlSugarRepository<>)); 

            return services;
        }

    }
}


