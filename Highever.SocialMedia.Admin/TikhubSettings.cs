using Highever.SocialMedia.Common;


/// <summary>
/// TikTok API配置设置
/// </summary>
public class TikhubSettings : ISingletonDependency
{ 

    /// <summary>
    /// API访问令牌
    /// </summary>
    public string ApiToken { get; set; } = string.Empty;

    /// <summary>
    /// 最大重试次数
    /// </summary>
    public int MaxRetryCount { get; set; } = 3;

    /// <summary>
    /// 最大延迟秒数
    /// </summary>
    public int MaxDelaySeconds { get; set; } = 10;

    /// <summary>
    /// 请求超时分钟数
    /// </summary>
    public int TimeoutMinutes { get; set; } = 1;

    /// <summary>
    /// 基础请求间隔毫秒数（避免频繁请求）
    /// </summary>
    public int RequestIntervalMs { get; set; } = 1000;

    /// <summary>
    /// 最大并发数（建议1-3，避免触发限制）
    /// </summary>
    public int MaxConcurrency { get; set; } = 2;

    /// <summary>
    /// 批次大小（每批处理的用户数）
    /// </summary>
    public int BatchSize { get; set; } = 10;

    /// <summary>
    /// 批次间延迟毫秒数（批次完成后的等待时间）
    /// </summary>
    public int BatchDelayMs { get; set; } = 5000;

    /// <summary>
    /// 是否启用自适应限流（根据响应调整请求频率）
    /// </summary>
    public bool EnableAdaptiveThrottling { get; set; } = true;

    /// <summary>
    /// 每分钟最大请求数（0表示不限制）
    /// </summary>
    public int MaxRequestsPerMinute { get; set; } = 60;

    /// <summary>
    /// 遇到限流时的退避时间（秒）
    /// </summary>
    public int ThrottleBackoffSeconds { get; set; } = 30;
}