using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Volvo.Release
{
    /// <summary>
    /// 
    /// </summary>
    public class HttpWebHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <param name="method"></param>
        /// <param name="requestmodel"></param>
        /// <returns></returns>
        public static HttpWebRequest Getrequest(string url, string method, RequestModel requestmodel)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            WebProxy webProxy = new WebProxy();


            if (requestmodel.ProxyIp != null)
            {
                string ip = requestmodel.ProxyIp;
                if (!ip.StartsWith("http"))
                {
                    ip = "http://" + ip;
                }
                ip = ip.Replace("https", "http");
                webProxy.Address = new Uri(ip);
                webProxy.Credentials = new NetworkCredential(requestmodel.ProxyUser, requestmodel.ProxyPassword);
                request.Proxy = webProxy;
            }
            if (requestmodel.container != null)
            {
                request.CookieContainer = requestmodel.container;
            }

            request.UseDefaultCredentials = true;

            request.UserAgent = requestmodel.UserAgent;
            request.ContentType = "application/json";
            request.Headers.Add("sec-fetch-mode", "navigate");
            request.Headers.Add("sec-ch-ua-mobile", "?0");
            request.Headers.Add("sec-fetch-site", "same-origin");
            request.Headers.Add("sec-fetch-dest", "document");
            request.Headers.Add("sec-ch-ua-platform", "\"Windows\"");
            request.Headers.Add("upgrade-insecure-requests", "1");
            request.Headers.Add("sec-fetch-user", "?1");


            request.Accept = "*/*";
            request.Method = method;
            return request;
        }
          
    }
}
