using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.DevTools;
using OpenQA.Selenium.DevTools.V136;
// 放到最上面的 using 区域
using NetCookie = System.Net.Cookie;
using SeleniumCookie = OpenQA.Selenium.Cookie;

namespace Highever.SocialMedia.Common;
/// <summary>
/// 
/// </summary>
public static class CookieDumper
{
    const string _ChromeDriverDir = @"D:\项目文件\HigheverSocialMedia\Highever.SocialMedia\Highever.SocialMedia.API\bin\Debug\net6.0\138chromedriver-win64"; // chromedriver.exe 所在目录
    const string _LoginUrl = "https://www.topview.ai";
    const string _Username = "highever.dt@gmail.com";
    const string _Password = "abc123...";
    const string _ProtectedApi = "https://example.com/protected-data";
    public static async Task<string> GetCookie()
    {
        string rawCookieHeader = string.Empty;
        // ① ChromeOptions
        var options = new ChromeOptions();
        options.AddArgument("--headless=new");
        options.AddArgument("--disable-gpu");
        options.AddArgument("--window-size=1400,1000");
        options.AddArgument("--lang=en-US");
        options.AddArgument("--user-agent=Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/136.0.0.0 Safari/537.36");
        options.AddArgument("--disable-blink-features=AutomationControlled");
        options.AddExcludedArgument("enable-automation");
        options.AddAdditionalOption("useAutomationExtension", false);
        // 如果你想在无 UI 服务器上跑：
        // options.AddArgument("--no-sandbox");
        // options.AddArgument("--disable-dev-shm-usage"); 

        //启动 Driver
        var service = ChromeDriverService.CreateDefaultService(_ChromeDriverDir);
        service.SuppressInitialDiagnosticInformation = true;   // 控制台更干净
        using (var driver = new ChromeDriver(service, options))
        {
            try
            {
                driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);

                driver.Navigate().GoToUrl(_LoginUrl);
                driver.FindElement(By.CssSelector("button[type=submit]")).Click();
                driver.FindElement(By.Name("username")).SendKeys(_Username);
                driver.FindElement(By.Name("password")).SendKeys(_Password);
                driver.FindElement(By.CssSelector("button[type=submit]")).Click();

                // 等待跳转或出现某元素（按需改）
                new OpenQA.Selenium.Support.UI.WebDriverWait(driver, TimeSpan.FromSeconds(10))
                    .Until(d => d.Url.Contains("/dashboard"));
                Console.WriteLine("✅ 已成功登录");

                // ④ DevTools：建立 Session，拿到 Domains 容器
                IDevTools dev = (IDevTools)driver;
                var cdpSession = dev.GetDevToolsSession();
                var domains = cdpSession.GetVersionSpecificDomains<OpenQA.Selenium.DevTools.V136.DevToolsSessionDomains>();

                var storResp = await domains.Storage.GetCookies(new OpenQA.Selenium.DevTools.V136.Storage.GetCookiesCommandSettings { });
                var cookies = storResp.Cookies; // List<Storage.Item>
                                                // ⑤ 输出到控制台并序列化到文件
                Console.WriteLine($"🍪 共获取 {cookies.Count()} 条 Cookie：");
                foreach (var ck in cookies)
                    Console.WriteLine($"{ck.Name}={ck.Value}; domain={ck.Domain}; HttpOnly={ck.HttpOnly}");

                rawCookieHeader = string.Join("; ",
            cookies.Select(ck => $"{ck.Name}={ck.Value}"));

                await File.WriteAllTextAsync(
            _ChromeDriverDir,
            JsonSerializer.Serialize(cookies, new JsonSerializerOptions { WriteIndented = true }));

                Console.WriteLine($"📄 完整 Cookie 已保存到 {_ChromeDriverDir}");

                // ⑥ Cookie 转 CookieContainer → HttpClient 继续访问
                var container = new CookieContainer();
                foreach (var ck in cookies)
                {
                    Console.WriteLine($"{ck.Name}={ck.Value}; domain={ck.Domain}; HttpOnly={ck.HttpOnly}");
                    container.Add(new NetCookie(
                        ck.Name,
                        ck.Value,
                        ck.Path,
                        ck.Domain.TrimStart('.'))
                    {
                        HttpOnly = ck.HttpOnly,
                        Secure = ck.Secure,
                        Expires = DateTimeOffset.FromUnixTimeMilliseconds((long)(ck.Expires * 1000)).UtcDateTime
                    });
                }
                using var handler = new HttpClientHandler { CookieContainer = container };
                using var http = new HttpClient(handler);

                //var apiHtml = await http.GetStringAsync(_ProtectedApi);
                //await File.WriteAllTextAsync("protected.html", apiHtml);
                //Console.WriteLine("🎉 已使用同一会话请求受保护接口，结果保存为 protected.html");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"🎉 异常：{ex.Message}，{ex.StackTrace}");
            }
            finally
            {
                driver.Quit();
                driver.Dispose();
                service.Dispose();
            }
        }

        return rawCookieHeader;
    }


    static readonly char[] InvalidNameChars = { ' ', '\t', '\r', '\n', ';', ',' };
    static readonly char[] InvalidValueChars = { '\r', '\n', ';', ',' };

    private static NetCookie ToNetCookie(dynamic ck)  // ck 是 Storage.Cookie 或 Network.Cookie
    {
        // 1️⃣ 过滤无效字符（也可选择 Uri.EscapeDataString 做编码）
        string name = ck.Name.Trim();
        string value = ck.Value.Replace("\r", "%0D")
                               .Replace("\n", "%0A")
                               .Replace(";", "%3B")
                               .Replace(",", "%2C");

        // Name 不能含下列字符
        if (name.IndexOfAny(InvalidNameChars) >= 0)
            throw new Exception($"Cookie 名字含非法字符: {name}");

        // 2️⃣ Path / Domain 兜底
        string path = string.IsNullOrEmpty(ck.Path) ? "/" : ck.Path;
        string domain = (ck.Domain ?? "").TrimStart('.');
        if (string.IsNullOrWhiteSpace(domain))
            throw new Exception("Cookie.Domain 不能为空");

        // 3️⃣ 构造
        var c = new NetCookie(name, value, path, domain)
        {
            HttpOnly = ck.HttpOnly,
            Secure = ck.Secure
        };

        // 4️⃣ 处理过期时间（DevTools 里是 UnixTime 秒或毫秒，按实际字段改）
        if (ck.Expires is double exp && exp > 0)
            c.Expires = DateTimeOffset.FromUnixTimeSeconds((long)exp).UtcDateTime;

        return c;
    }

}
