using Highever.SocialMedia.Application.Contracts;
using Highever.SocialMedia.Common;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Highever.SocialMedia.Application
{
    public static class DependencyInjectionExtensions
    {
        /// <summary>
        /// 注册应用服务 - 自动检测调用程序集
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // 获取调用程序集
            var callingAssembly = Assembly.GetCallingAssembly();
            return AddApplicationServices(services, callingAssembly);
        }

        /// <summary>
        /// 注册应用服务 - 指定程序集名称
        /// </summary>
        /// <param name="services"></param>
        /// <param name="assemblyName">程序集名称</param>
        /// <returns></returns>
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, string assemblyName)
        {
            try
            {
                var assembly = Assembly.Load(assemblyName);
                return AddApplicationServices(services, assembly);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"无法加载程序集 '{assemblyName}': {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 注册应用服务 - 指定程序集实例
        /// </summary>
        /// <param name="services"></param>
        /// <param name="additionalAssembly">额外的程序集</param>
        /// <returns></returns>
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, Assembly additionalAssembly)
        {
            var assemblies = GetRequiredAssemblies(additionalAssembly);
            return RegisterServices(services, assemblies);
        }

        /// <summary>
        /// 注册应用服务 - 指定多个程序集
        /// </summary>
        /// <param name="services"></param>
        /// <param name="additionalAssemblies">额外的程序集数组</param>
        /// <returns></returns>
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, Assembly[] additionalAssemblies)
        {
            var assemblies = GetRequiredAssemblies(additionalAssemblies);
            return RegisterServices(services, assemblies);
        }

        /// <summary>
        /// 获取必需的程序集
        /// </summary>
        /// <param name="additionalAssemblies">额外程序集</param>
        /// <returns></returns>
        private static Assembly[] GetRequiredAssemblies(params Assembly[] additionalAssemblies)
        {
            var coreAssemblies = new List<Assembly>();

            // 核心程序集
            var coreAssemblyNames = new[]
            {
                "Highever.SocialMedia.Application",
                "Highever.SocialMedia.Application.Contracts"
            };

            foreach (var assemblyName in coreAssemblyNames)
            {
                try
                {
                    var assembly = Assembly.Load(assemblyName);
                    coreAssemblies.Add(assembly);
                }
                catch (Exception ex)
                {
                    // 记录警告但不中断程序
                    Console.WriteLine($"警告: 无法加载核心程序集 '{assemblyName}': {ex.Message}");
                }
            }

            // 添加额外程序集
            if (additionalAssemblies?.Length > 0)
            {
                coreAssemblies.AddRange(additionalAssemblies.Where(a => a != null));
            }

            return coreAssemblies.ToArray();
        }

        /// <summary>
        /// 注册服务到容器
        /// </summary>
        /// <param name="services"></param>
        /// <param name="assemblies"></param>
        /// <returns></returns>
        private static IServiceCollection RegisterServices(IServiceCollection services, Assembly[] assemblies)
        {
            if (assemblies?.Length == 0)
            {
                throw new InvalidOperationException("没有找到可用的程序集进行服务注册");
            }

            services.Scan(scan => scan
                .FromAssemblies(assemblies)
                // 注册单例服务
                .AddClasses(classes => classes.AssignableTo<ISingletonDependency>())
                .AsImplementedInterfaces()
                .AsSelfWithInterfaces()
                .WithSingletonLifetime()
                // 注册作用域服务
                .AddClasses(classes => classes.AssignableTo<IScopedDependency>())
                .AsImplementedInterfaces()
                .AsSelfWithInterfaces()
                .WithScopedLifetime()
                // 注册瞬态服务
                .AddClasses(classes => classes.AssignableTo<ITransientDependency>())
                .AsImplementedInterfaces()
                .AsSelfWithInterfaces()
                .WithTransientLifetime());

            return services;
        }
    }
}

