using Microsoft.Extensions.Configuration;

namespace Highever.SocialMedia.Common
{
    /// <summary>
    /// 
    /// </summary>
    public class AppSettingConifgHelper
    {
        /// <summary>
        /// 
        /// </summary>
        private static IConfiguration _config;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="configuration"></param>
        public AppSettingConifgHelper(IConfiguration configuration)
        {
            _config = configuration;
        }
        /// <summary>
        /// 读取指定节点的字符串
        /// </summary>
        /// <param name="sessions"></param>
        /// <returns></returns>
        public static string ReadAppSettings(string sessions)
        {
            try
            {
                if (sessions.Any())
                {
                    return _config[sessions];
                }
            }
            catch
            {
                return "";
            }
            return "";
        }
        /// <summary>
        /// 读取指定节点的字符串
        /// </summary>
        /// <param name="sessions"></param>
        /// <returns></returns>
        public static string ReadAppSettings(params string[] sessions)
        {
            try
            {
                if (sessions.Any())
                {
                    return _config[string.Join(":", sessions)];
                }
            }
            catch
            {
                return "";
            }
            return "";
        }
        /// <summary>
        /// 读取实体信息
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="session"></param>
        /// <returns></returns>
        public static List<T> ReadAppSettings<T>(params string[] session)
        {
            List<T> list = new List<T>();
            _config.Bind(string.Join(":", session), list);
            return list;
        }
    }
}
