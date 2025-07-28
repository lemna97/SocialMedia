using Microsoft.AspNetCore.Hosting;

namespace Highever.SocialMedia.Common
{
    /// <summary>
    /// 
    /// </summary>
    public static class HostingEnvironmentHelper
    {
        /// <summary>
        /// 
        /// </summary>
        private static IHostingEnvironment _hostEnviroment;
        public static string WebPath => _hostEnviroment.WebRootPath;

        public static string MapPath(string path)
        {
            return Path.Combine(_hostEnviroment.WebRootPath, path);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="hostEnviroment"></param>
        public static void Configure(IHostingEnvironment hostEnviroment)
        {
            _hostEnviroment = hostEnviroment;
        }
    }
}