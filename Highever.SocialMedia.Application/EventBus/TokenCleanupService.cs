using Highever.SocialMedia.Application.Contracts;
using Highever.SocialMedia.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Highever.SocialMedia.Application.EventBus
{
    /// <summary>
    /// Token清理后台服务
    /// </summary>
    public class TokenCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly TimeSpan _cleanupInterval = TimeSpan.FromHours(6); // 每6小时清理一次

        public TokenCleanupService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // 在作用域内获取日志服务
            using var scope = _serviceProvider.CreateScope();
            var logger = scope.ServiceProvider.GetRequiredService<INLogger>();
            
            logger.Info("Token清理服务已启动");
            Console.WriteLine("Token清理服务已启动");
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var innerScope = _serviceProvider.CreateScope();
                    var tokenService = innerScope.ServiceProvider.GetRequiredService<ITokenService>();
                    var scopedLogger = innerScope.ServiceProvider.GetRequiredService<INLogger>();

                    var cleanedCount = await tokenService.CleanupExpiredTokensAsync();

                    if (cleanedCount > 0)
                    {
                        scopedLogger.Info($"定时清理完成，共清理 {cleanedCount} 个过期令牌");
                    }
                }
                catch (Exception ex)
                {
                    using var errorScope = _serviceProvider.CreateScope();
                    var errorLogger = errorScope.ServiceProvider.GetRequiredService<INLogger>();
                    errorLogger.Error(ex, "定时清理过期令牌时发生错误");
                }

                await Task.Delay(_cleanupInterval, stoppingToken);
            }
            
            using var finalScope = _serviceProvider.CreateScope();
            var finalLogger = finalScope.ServiceProvider.GetRequiredService<INLogger>();
            finalLogger.Info("Token清理服务已停止");
            Console.WriteLine("Token清理服务已停止");
        }
    }
}
