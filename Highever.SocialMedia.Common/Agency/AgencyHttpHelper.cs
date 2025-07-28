using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Highever.SocialMedia.Common;

/// <summary>
/// 通用 HTTP 帮助类：自动携带登录 Cookie，获取指定 URL 的响应文本。
/// </summary>
public static class AgencyHttpHelper
{
    /// <summary>
    /// 发送 GET 请求并返回响应内容（UTF-8 编码）。<br/>
    /// 依赖 <see cref="CookieDumper.GetCookie"/> 获取登录会话 Cookie。
    /// </summary>
    /// <param name="url">完整绝对 URL，例如 https://example.com/api/data</param>
    /// <returns>服务器返回的响应字符串</returns>
    /// <exception cref="HttpRequestException">HTTP 非 2xx 时抛出</exception>
    public static async Task<string> FetchAsync(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("URL 不能为空", nameof(url));

        //先拿到登录 Cookie（按需可缓存到内存/Redis，避免每次都重新登陆）
        //string rawCookieHeader = await CookieDumper.GetCookie();

        // ② 准备 HttpClient，并把 Cookie 写进请求头
        using var handler = new HttpClientHandler();
        using var client = new HttpClient(handler, disposeHandler: true);

        // AddWithoutValidation 防止某些特殊字符被 HttpClient 拒绝
        //client.DefaultRequestHeaders.TryAddWithoutValidation("Cookie", rawCookieHeader);

        // ③ 发送 GET
        using var resp = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
        resp.EnsureSuccessStatusCode();
         
        return await resp.Content.ReadAsStringAsync();
    }
}
