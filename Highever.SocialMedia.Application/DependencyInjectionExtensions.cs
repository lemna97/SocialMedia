using Highever.SocialMedia.Application.Contracts;
using Highever.SocialMedia.Application.EventBus;
using Highever.SocialMedia.Application.Services.System;
using Highever.SocialMedia.Common;
using LinqKit;
using Microsoft.Extensions.DependencyInjection;
using NPOI.SS.Formula.Functions;
using NuGet.Packaging;
using SQLBuilder.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Highever.SocialMedia.Application
{
    public static class DependencyInjectionExtensions
    {
        /// <summary>
        /// 应该写在 AddApplicationServices 中，不可以再开其他入口
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddApplicationEventBusServices(this IServiceCollection services)
        {

            // 后台服务
            services.AddHostedService<TokenCleanupService>();

            // 其他服务...

            return services;
        }
        /// <summary>
        /// Application.Contracts 接口注入
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, string nowAssemblyName)
        {
            var assemblies = new[]
            {
                Assembly.Load("Highever.SocialMedia.Application"),
                Assembly.Load("Highever.SocialMedia.Application.Contracts"),
                Assembly.Load($"{nowAssemblyName}")
            };

            services.Scan(scan => scan
                .FromAssemblies(assemblies)
                .AddClasses(classes => classes.AssignableTo<ISingletonDependency>())
                .AsImplementedInterfaces()
                .AsSelfWithInterfaces()//同时注册自身和接口
                .WithSingletonLifetime()
                .AddClasses(classes => classes.AssignableTo<IScopedDependency>())
                .AsImplementedInterfaces()
                .AsSelfWithInterfaces()//同时注册自身和接口
                .WithScopedLifetime()
                .AddClasses(classes => classes.AssignableTo<ITransientDependency>())
                .AsImplementedInterfaces()
                .AsSelfWithInterfaces()//同时注册自身和接口
                .WithTransientLifetime());

            return services;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, Assembly[] assemblies)
        {
            var now_assemblies = new[]
            {
                Assembly.Load("Highever.SocialMedia.Application"),
                Assembly.Load("Highever.SocialMedia.Application.Contracts")
            };
            now_assemblies.AddRange(assemblies);
            services.Scan(scan => scan
                .FromAssemblies(now_assemblies)
                .AddClasses(classes => classes.AssignableTo<ISingletonDependency>())
                .AsImplementedInterfaces()
                .AsSelfWithInterfaces()//同时注册自身和接口
                .WithSingletonLifetime()
                .AddClasses(classes => classes.AssignableTo<IScopedDependency>())
                .AsImplementedInterfaces()
                .AsSelfWithInterfaces()//同时注册自身和接口
                .WithScopedLifetime()
                .AddClasses(classes => classes.AssignableTo<ITransientDependency>())
                .AsImplementedInterfaces()
                .AsSelfWithInterfaces()//同时注册自身和接口
                .WithTransientLifetime());

            return services;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            var assemblies = new[]
            {
                Assembly.Load("Highever.SocialMedia.Application"),
                Assembly.Load("Highever.SocialMedia.Application.Contracts")
            };

            services.Scan(scan => scan
                .FromAssemblies(assemblies)
                .AddClasses(classes => classes.AssignableTo<ISingletonDependency>())
                .AsImplementedInterfaces()
                .AsSelfWithInterfaces()//同时注册自身和接口
                .WithSingletonLifetime()
                .AddClasses(classes => classes.AssignableTo<IScopedDependency>())
                .AsImplementedInterfaces()
                .AsSelfWithInterfaces()//同时注册自身和接口
                .WithScopedLifetime()
                .AddClasses(classes => classes.AssignableTo<ITransientDependency>())
                .AsImplementedInterfaces()
                .AsSelfWithInterfaces()//同时注册自身和接口
                .WithTransientLifetime());

            return services;
        } 
    }
}

