using Highever.SocialMedia.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Highever.SocialMedia.MongoDB
{
    /// <summary>
    /// MongoDB 模块化注入
    /// </summary>
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddMongoDB(this IServiceCollection services)
        {
            const string _databaseName = "DatabaseName2";
            #region 检查 INLogger 是否已注册 
            if (!services.Any(service => service.ServiceType == typeof(INLogger)))
            {
                //日志注入
                services.AddScoped<INLogger, NLogAdapter>();
            }
            #endregion 
            // 注入 IMongoDBContext 接口对应的上下文实现
            services.AddScoped<IMongoDBContext>(provider =>
            {
                var configuration = provider.GetRequiredService<IConfiguration>();

                // 从配置中读取主库连接字符串和数据库名称
                string connectionString = configuration.GetConnectionString("Mongodb");
                string databaseName = configuration[$"Mongodb:{_databaseName}"];

                if (string.IsNullOrWhiteSpace(connectionString))
                    throw new ArgumentException("Mongodb 连接字符串不能为空！");
                if (string.IsNullOrWhiteSpace(databaseName))
                    throw new ArgumentException("Mongodb 数据库名称不能为空！");

                // 日志服务（提前注入）
                var logger = provider.GetRequiredService<INLogger>();

                // 初始化并返回 MongoDB 上下文
                return new MongoDBContext(connectionString, databaseName, logger);
            });
            // 注入 仓储接口
            services.AddScoped(typeof(IMongoRepository<>), typeof(MongoRepository<>));

            return services;
        }
    }
}
