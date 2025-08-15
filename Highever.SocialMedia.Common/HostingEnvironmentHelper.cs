using Microsoft.AspNetCore.Hosting;

namespace Highever.SocialMedia.Common
{
    /// <summary>
    /// 主机环境帮助类
    /// </summary>
    public static class HostingEnvironmentHelper
    {
        /// <summary>
        /// 主机环境实例
        /// </summary>
        private static IHostingEnvironment _hostEnviroment;

        /// <summary>
        /// 获取Web根路径
        /// </summary>
        public static string WebPath
        {
            get
            {
                if (_hostEnviroment == null)
                    throw new InvalidOperationException("HostingEnvironmentHelper未初始化，请确保在Startup中调用了UseStaticHostEnviroment()");
                return _hostEnviroment.WebRootPath;
            }
        }
        public static string ContentRootPath
        {
            get
            {
                if (_hostEnviroment == null)
                    throw new InvalidOperationException("HostingEnvironmentHelper未初始化，请确保在Startup中调用了UseStaticHostEnviroment()");
                return _hostEnviroment.ContentRootPath;
            }
        }

        /// <summary>
        /// 映射路径
        /// </summary>
        /// <param name="path">相对路径</param>
        /// <returns>绝对路径</returns>
        public static string MapPath(string path)
        {
            if (_hostEnviroment == null)
                throw new InvalidOperationException("HostingEnvironmentHelper未初始化，请确保在Startup中调用了UseStaticHostEnviroment()");

            if (string.IsNullOrEmpty(path))
                return _hostEnviroment.WebRootPath;

            return Path.Combine(_hostEnviroment.WebRootPath, path);
        }

        /// <summary>
        /// 配置主机环境
        /// </summary>
        /// <param name="hostEnviroment">主机环境实例</param>
        public static void Configure(IHostingEnvironment hostEnviroment)
        {
            _hostEnviroment = hostEnviroment ?? throw new ArgumentNullException(nameof(hostEnviroment));
        }

        /// <summary>
        /// 检查是否已初始化
        /// </summary>
        public static bool IsConfigured => _hostEnviroment != null;
    }
}
