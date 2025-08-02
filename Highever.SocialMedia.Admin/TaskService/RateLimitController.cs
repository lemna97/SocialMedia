using System.Collections.Concurrent;

namespace Highever.SocialMedia.Admin.TaskService
{
    /// <summary>
    /// API请求限流控制器
    /// </summary>
    public class RateLimitController
    {
        private readonly TikhubSettings _settings;
        private readonly ConcurrentQueue<DateTime> _requestTimes = new();
        private readonly SemaphoreSlim _semaphore;
        private DateTime _lastThrottleTime = DateTime.MinValue;
        private int _consecutiveErrors = 0;

        public RateLimitController(TikhubSettings settings)
        {
            _settings = settings;
            _semaphore = new SemaphoreSlim(_settings.MaxConcurrency);
        }

        /// <summary>
        /// 等待获取请求许可
        /// </summary>
        public async Task WaitForPermissionAsync()
        {
            // 1. 等待并发槽位
            await _semaphore.WaitAsync();

            try
            {
                // 2. 检查是否需要限流退避
                if (DateTime.Now - _lastThrottleTime < TimeSpan.FromSeconds(_settings.ThrottleBackoffSeconds))
                {
                    var waitTime = TimeSpan.FromSeconds(_settings.ThrottleBackoffSeconds) - (DateTime.Now - _lastThrottleTime);
                    Console.WriteLine($"限流退避中，等待 {waitTime.TotalSeconds:F1} 秒...");
                    await Task.Delay(waitTime);
                }

                // 3. 检查每分钟请求限制
                if (_settings.MaxRequestsPerMinute > 0)
                {
                    await EnforceRateLimitAsync();
                }

                // 4. 基础请求间隔
                var baseDelay = _settings.RequestIntervalMs;
                
                // 5. 根据连续错误数动态调整延迟
                if (_consecutiveErrors > 0)
                {
                    baseDelay *= (int)Math.Pow(2, Math.Min(_consecutiveErrors, 3)); // 最多8倍延迟
                    Console.WriteLine($"检测到连续错误，延长请求间隔至 {baseDelay}ms");
                }

                await Task.Delay(baseDelay);

                // 6. 记录请求时间
                _requestTimes.Enqueue(DateTime.Now);
            }
            finally
            {
                // 注意：这里不释放信号量，在请求完成后释放
            }
        }

        /// <summary>
        /// 释放请求许可
        /// </summary>
        public void ReleasePermission()
        {
            _semaphore.Release();
        }

        /// <summary>
        /// 报告请求成功
        /// </summary>
        public void ReportSuccess()
        {
            _consecutiveErrors = 0;
        }

        /// <summary>
        /// 报告请求失败
        /// </summary>
        public void ReportError(bool isRateLimited = false)
        {
            _consecutiveErrors++;
            
            if (isRateLimited)
            {
                _lastThrottleTime = DateTime.Now;
                Console.WriteLine($"检测到API限流，启动退避策略");
            }
        }

        /// <summary>
        /// 强制执行每分钟请求限制
        /// </summary>
        private async Task EnforceRateLimitAsync()
        {
            var now = DateTime.Now;
            var oneMinuteAgo = now.AddMinutes(-1);

            // 清理过期的请求记录
            while (_requestTimes.TryPeek(out var oldTime) && oldTime < oneMinuteAgo)
            {
                _requestTimes.TryDequeue(out _);
            }

            // 检查是否超过限制
            if (_requestTimes.Count >= _settings.MaxRequestsPerMinute)
            {
                var oldestRequest = _requestTimes.TryPeek(out var oldest) ? oldest : now;
                var waitTime = oldestRequest.AddMinutes(1) - now;
                
                if (waitTime > TimeSpan.Zero)
                {
                    Console.WriteLine($"达到每分钟请求限制，等待 {waitTime.TotalSeconds:F1} 秒...");
                    await Task.Delay(waitTime);
                }
            }
        }
    }
}