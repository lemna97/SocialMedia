﻿using Highever.SocialMedia.Application.Contracts;
using Highever.SocialMedia.Common;
using Microsoft.Extensions.DependencyInjection;
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
        /// Application.Contracts 接口注入
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            var assemblies = new[]
            {
                Assembly.Load("Highever.SocialMedia.Application"),
                Assembly.Load("Highever.SocialMedia.Application.Contracts"),
                Assembly.Load("Highever.SocialMedia.Admin")
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

