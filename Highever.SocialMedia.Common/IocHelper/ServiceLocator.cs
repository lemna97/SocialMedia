namespace Highever.SocialMedia.Common
{
    /// <summary>
    /// 服务定位器
    /// </summary>
    public class ServiceLocator
    {
        /// <summary>
        /// 
        /// </summary>
        private static IServiceProvider _serviceProvider { get; set; }
        public static IServiceProvider Current
        {
            get
            {
                if (!IsLocationProviderSet)
                {
                    throw new InvalidOperationException(" ServiceLocationProvider must be Set ，So you should contact the programmer ！");
                }
                return _serviceProvider;
            }
        }
        public static bool IsLocationProviderSet => _serviceProvider != null;
        public static void SetLocatorProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
    }
}
