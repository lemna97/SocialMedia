using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System.Threading;
using System.Threading.Tasks;


namespace Highever.SocialMedia.Common
{
    /// <summary>
    /// 
    /// </summary>
    public static class AmazonProductHelper
    {
        /// <summary>
        /// 抓取亚马逊商品页面主要信息
        /// <remark>
        /// 
        /// </remark>
        /// </summary>
        public static AmazonProductInfo ParseAmazonProduct(string url, Dictionary<string, string> cookies = null)
        {
            // 其他的比如referer、cookie、sec-ch-ua...都不用加，Selenium浏览器会自动生成
            var options = new ChromeOptions();
            options.AddArgument("--headless");
            options.AddArgument("--disable-gpu");
            options.AddArgument("--window-size=1400,1000");
            options.AddArgument("--lang=en-US");
            options.AddArgument("--user-agent=Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/139.0.0.0 Safari/537.36");
            // 反爬虫伪装参数↓↓↓
            options.AddArgument("--disable-blink-features=AutomationControlled");
            options.AddExcludedArgument("enable-automation");
            options.AddAdditionalOption("useAutomationExtension", false);
            // 如遇特殊环境
            //options.AddArgument("--no-sandbox");
            //options.AddArgument("--disable-dev-shm-usage");
            // 【1】显式指定 chromedriver 存放路径
            string chromeDriverDirectory = AppDomain.CurrentDomain.BaseDirectory;

            // 【2】创建 ChromeDriverService
            var service = ChromeDriverService.CreateDefaultService(chromeDriverDirectory);

            // 【3】实例化时带上 service 和 options
            using (var driver = new ChromeDriver(service, options))
            {
                try
                {
                    // 注入JS覆盖 webdriver 字段
                    driver.ExecuteScript("Object.defineProperty(navigator, 'webdriver', {get: () => undefined})");
                    // 先进入 amazon.com 域名（很关键！！！）
                    driver.Navigate().GoToUrl("https://www.amazon.com");

                    // 加cookies
                    if (cookies != null)
                    {
                        foreach (var kv in cookies)
                        {
                            driver.Manage().Cookies.AddCookie(new Cookie(kv.Key, kv.Value));
                        }
                    }

                    driver.Navigate().GoToUrl(url);

                    // ---------- 新增判404逻辑 ----------
                    string pageSource = driver.PageSource;

                    // 可以根据图片src、报错文字或title判断，只需命中其一即可
                    if (pageSource.Contains("images-na.ssl-images-amazon.com/images/G/01/error/en_US/title._TTD_.png") ||
                        pageSource.Contains("Sorry! We couldn't find that page.") ||
                        pageSource.Contains("<title>Amazon.com: 404</title>"))
                    {
                        // 判定为404或商品已下架，直接返回空对象
                        return new AmazonProductInfo();
                    }

                    // 1. 标题 
                    string title = "";
                    try
                    {
                        // 等待页面加载(如需更健壮可使用WebDriverWait)
                        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                        title = wait.Until(drv => drv.FindElement(By.Id("productTitle"))).Text.Trim();
                    }
                    catch
                    {
                        File.WriteAllText(@$"C:\temp\{DateTime.Now.ToString("yyyy_mm_dd")}\amazon_{url.Split("/").Last().Replace("?th=1", "")}_page.html", driver.PageSource);
                    }

                    // 2.获取当前颜色
                    string color = "";
                    try
                    {
                        var colorEl = driver.FindElement(By.CssSelector("#inline-twister-expanded-dimension-text-color_name"));
                        if (colorEl != null)
                            color = colorEl.Text.Trim();
                    }
                    catch
                    {
                        File.WriteAllText(@$"C:\temp\{DateTime.Now.ToString("yyyy_mm_dd")}\amazon_{url.Split("/").Last().Replace("?th=1", "")}_page.html", driver.PageSource);
                    }
                    // 3. 获取图片地址们
                    var images = GetImages_Json(driver, color);

                    // 3. 五点描述，key/value字典
                    var fivePoints = GetFivePoints_ById(driver);

                    // 4. 属性表
                    var attributes = GetAttributes(driver);

                    // 5. 提取品类信息
                    var category = GetCategoryFromBreadcrumb(driver);

                    return new AmazonProductInfo
                    {
                        Title = title,
                        Images = images,
                        FivePoints = fivePoints,
                        Attributes = attributes,
                        Category = category
                    };
                }
                finally
                {
                    driver.Quit();
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        private static List<string> GetImages_Json(IWebDriver driver, string color = null)
        {
            var imgs = new List<string>();

            try
            {
                // 1. 优先用 <script> 中的 colorImages JSON
                var scripts = driver.FindElements(By.CssSelector("#imageBlockVariations_feature_div script"));
                foreach (var script in scripts)
                {
                    var js = script.GetAttribute("innerText");
                    if (!string.IsNullOrEmpty(js) && js.Contains("colorImages"))
                    {
                        int idx1 = js.IndexOf("jQuery.parseJSON('");
                        int idx2 = js.LastIndexOf("');");
                        if (idx1 >= 0 && idx2 > idx1)
                        {
                            var jsonstr = js.Substring(idx1 + "jQuery.parseJSON('".Length, idx2 - idx1 - "jQuery.parseJSON('".Length);
                            // 反转义
                            jsonstr = System.Text.RegularExpressions.Regex.Unescape(jsonstr);

                            // 用Newtonsoft.Json解析
                            var jobject = Newtonsoft.Json.Linq.JObject.Parse(jsonstr);

                            var colorobj = jobject["colorImages"] as Newtonsoft.Json.Linq.JObject;
                            if (colorobj != null)
                            {
                                // 色名兼容处理
                                IEnumerable<string> colorKeys = colorobj.Properties().Select(p => p.Name);
                                List<string> keyToUse = null;
                                if (!string.IsNullOrWhiteSpace(color))
                                {
                                    // 寻找完全匹配色名
                                    keyToUse = colorKeys.Where(k => k.Trim().ToLower() == color.Trim().ToLower()).ToList();
                                }
                                // 没找到就返回个空
                                if (keyToUse == null && colorKeys.Any())
                                {
                                    //默认取第一个
                                    //keyToUse = colorKeys.First();
                                    //如果没找到对应颜色的图片，直接返回空，后面排查原因
                                    continue;
                                }

                                if (keyToUse != null)
                                {
                                    foreach (var item in keyToUse)
                                    {
                                        var arr = colorobj[item] as Newtonsoft.Json.Linq.JArray;
                                        if (arr != null)
                                        {
                                            foreach (var j in arr)
                                            {
                                                string imgurl = j["large"]?.ToString();
                                                if (!string.IsNullOrWhiteSpace(imgurl) && !imgs.Contains(imgurl))
                                                    imgs.Add(imgurl);
                                            }
                                        }
                                    }
                                }
                                break; // 成功解析后结束循环
                            }
                        }
                    }
                }
            }
            catch { /* 忽略异常，走降级方案 */ }

            // 2. 如果未提取到，则用原 img 标签法兜底
            if (imgs.Count == 0)
            {
                var imgList = driver.FindElements(By.CssSelector("ul.a-unordered-list.a-nostyle.a-horizontal.list.maintain-height li"));
                foreach (var li in imgList)
                {
                    var imgEl = li.FindElements(By.CssSelector("img")).FirstOrDefault();
                    if (imgEl != null)
                    {
                        string imgUrl = imgEl.GetAttribute("data-old-hires");
                        if (string.IsNullOrEmpty(imgUrl))
                            imgUrl = imgEl.GetAttribute("src");
                        if (!string.IsNullOrWhiteSpace(imgUrl) && !imgs.Contains(imgUrl))
                            imgs.Add(imgUrl);
                    }
                }
            }

            return imgs;
        }
        /// <summary>
        /// 从html元素中获取
        /// </summary>
        /// <param name="driver"></param>
        /// <returns></returns>
        private static List<string> GetImages_Html(IWebDriver driver)
        {
            // 针对你提供的 HTML 结构精确提取
            var imgs = new List<string>();
            // ul 带有class
            var imgList = driver.FindElements(By.CssSelector("ul.a-unordered-list.a-nostyle.a-horizontal.list.maintain-height li"));
            foreach (var li in imgList)
            {
                // 每个li下都可能有img
                var imgEl = li.FindElements(By.CssSelector("img")).FirstOrDefault();
                if (imgEl != null)
                {
                    // 优先拿data-old-hires（高清），否则拿src
                    string imgUrl = imgEl.GetAttribute("data-old-hires");
                    if (string.IsNullOrEmpty(imgUrl))
                        imgUrl = imgEl.GetAttribute("src");
                    if (!string.IsNullOrWhiteSpace(imgUrl) && !imgs.Contains(imgUrl))
                        imgs.Add(imgUrl);
                }
            }
            return imgs;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="driver"></param>
        /// <returns></returns>
        private static Dictionary<string, string> GetFivePoints_Class(IWebDriver driver)
        {
            var dict = new Dictionary<string, string>();
            var ul = driver.FindElements(By.CssSelector("ul.a-unordered-list.a-vertical.a-spacing-mini")).FirstOrDefault();
            if (ul != null)
            {
                var lis = ul.FindElements(By.CssSelector("li"));
                int idx = 1;
                foreach (var li in lis)
                {
                    var span = li.FindElements(By.CssSelector(".a-list-item")).FirstOrDefault();
                    string text = span != null ? span.Text.Trim() : li.Text.Trim();
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        dict[$"Feature{idx}"] = text;
                        idx++;
                    }
                }
            }
            return dict;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="driver"></param>
        /// <returns></returns>
        private static Dictionary<string, string> GetFivePoints_Id(IWebDriver driver)
        {
            var dict = new Dictionary<string, string>();

            try
            {
                // 首先尝试点击展开按钮
                try
                {
                    var expandButton = driver.FindElement(By.CssSelector("#productFactsToggleButton a.a-declarative"));

                    if (expandButton.Displayed && expandButton.Enabled)
                    {
                        Console.WriteLine("找到展开按钮，正在点击...");
                        ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView(true);", expandButton);
                        Thread.Sleep(500);
                        expandButton.Click();
                        Thread.Sleep(2000);
                        Console.WriteLine("已点击展开按钮，等待内容加载...");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"点击展开按钮失败: {ex.Message}");
                }

                // 获取展开后的内容
                var expanderContent = driver.FindElement(By.CssSelector("div.a-expander-content.a-expander-partial-collapse-content"));
                var ul = expanderContent.FindElement(By.CssSelector("ul.a-unordered-list.a-vertical.a-spacing-small"));

                if (ul != null)
                {
                    var lis = ul.FindElements(By.TagName("li"));
                    Console.WriteLine($"展开后找到 {lis.Count} 个li元素");

                    int idx = 1;
                    for (int i = 0; i <= lis.Count; i++)
                    {
                        var li = lis[i];
                        Console.WriteLine($"正在处理第 {i + 1} 个li元素...");

                        try
                        {
                            var span = li.FindElement(By.CssSelector("span.a-list-item.a-size-base.a-color-base"));
                            if (span != null)
                            {
                                string text = span.Text?.Trim();
                                Console.WriteLine($"第 {i + 1} 个li的文本长度: {text?.Length ?? 0}");

                                if (!string.IsNullOrWhiteSpace(text))
                                {
                                    dict[$"Feature{idx}"] = text;
                                    Console.WriteLine($"Feature{idx}: {text.Substring(0, Math.Min(50, text.Length))}...");
                                    idx++;
                                }
                                else
                                {
                                    Console.WriteLine($"第 {i + 1} 个li的文本为空或只包含空白字符");
                                }
                            }
                            else
                            {
                                Console.WriteLine($"第 {i + 1} 个li没有找到span元素");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"处理第 {i + 1} 个li元素失败: {ex.Message}");

                            // 尝试直接获取li的文本作为备用方案
                            try
                            {
                                string liText = li.Text?.Trim();
                                if (!string.IsNullOrWhiteSpace(liText))
                                {
                                    dict[$"Feature{idx}"] = liText;
                                    Console.WriteLine($"Feature{idx} (备用方案): {liText.Substring(0, Math.Min(50, liText.Length))}...");
                                    idx++;
                                }
                            }
                            catch (Exception ex2)
                            {
                                Console.WriteLine($"备用方案也失败: {ex2.Message}");
                            }
                        }
                    }

                    Console.WriteLine($"最终获取到 {dict.Count} 个描述");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"主要方法失败: {ex.Message}");

                // 降级方案
                var lis = driver.FindElements(By.CssSelector("#feature-bullets li"));
                int idx = 1;
                foreach (var li in lis)
                {
                    var span = li.FindElements(By.CssSelector(".a-list-item")).FirstOrDefault();
                    string text = span != null ? span.Text.Trim() : li.Text.Trim();
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        dict[$"Feature{idx}"] = text;
                        idx++;
                    }
                }
            }

            return dict;
        }
        /// <summary>
        /// 通过ID定位父容器获取五点描述
        /// </summary>
        /// <param name="driver"></param>
        /// <returns></returns>
        private static Dictionary<string, string> GetFivePoints_ById(IWebDriver driver)
        {
            var dict = new Dictionary<string, string>();

            try
            {
                Console.WriteLine("开始通过ID获取五点描述...");

                // 先定位到父容器
                var parentDiv = driver.FindElement(By.Id("productFactsDesktop_feature_div"));
                Console.WriteLine("成功找到父容器: productFactsDesktop_feature_div");

                // 首先尝试点击展开按钮（如果存在）
                try
                {
                    var expandButton = parentDiv.FindElement(By.CssSelector("#productFactsToggleButton a.a-declarative"));
                    if (expandButton.Displayed && expandButton.Enabled)
                    {
                        Console.WriteLine("找到展开按钮，正在点击...");
                        expandButton.Click();
                        Thread.Sleep(2000);
                        Console.WriteLine("已点击展开按钮");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"展开按钮处理: {ex.Message}");
                }

                // 在父容器内查找ul元素
                var ul = parentDiv.FindElement(By.CssSelector("ul.a-unordered-list.a-vertical.a-spacing-small"));
                Console.WriteLine("成功找到ul元素");

                // 获取所有li元素
                var lis = ul.FindElements(By.TagName("li"));
                Console.WriteLine($"找到 {lis.Count} 个li元素");

                // 逐个处理li元素
                for (int i = 0; i < lis.Count; i++)
                {
                    var li = lis[i];
                    Console.WriteLine($"\n=== 处理第 {i + 1} 个li元素 ===");

                    try
                    {
                        // 直接获取li的完整文本内容
                        string liText = li.Text?.Trim();
                        Console.WriteLine($"li完整文本内容: {liText}");
                        Console.WriteLine($"文本长度: {liText?.Length ?? 0}");

                        if (!string.IsNullOrWhiteSpace(liText))
                        {
                            dict[$"Feature{i + 1}"] = liText;
                            Console.WriteLine($"✓ 成功添加 Feature{i + 1}");
                        }
                        else
                        {
                            Console.WriteLine($"✗ 第 {i + 1} 个li文本为空");
                        }

                        // 同时也尝试获取span内容作为对比
                        try
                        {
                            var span = li.FindElement(By.CssSelector("span.a-list-item.a-size-base.a-color-base"));
                            string spanText = span?.Text?.Trim();
                            Console.WriteLine($"span文本内容: {spanText}");
                            Console.WriteLine($"li文本和span文本是否一致: {liText == spanText}");
                        }
                        catch (Exception spanEx)
                        {
                            Console.WriteLine($"获取span失败: {spanEx.Message}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"✗ 处理第 {i + 1} 个li失败: {ex.Message}");
                    }
                }

                Console.WriteLine($"\n=== 最终结果 ===");
                Console.WriteLine($"总共获取到 {dict.Count} 个描述");

                // 输出所有获取到的描述
                foreach (var kvp in dict)
                {
                    Console.WriteLine($"{kvp.Key}: {kvp.Value.Substring(0, Math.Min(100, kvp.Value.Length))}...");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"获取五点描述失败: {ex.Message}");
                Console.WriteLine($"异常详情: {ex.StackTrace}");
            }

            return dict;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="driver"></param>
        /// <returns></returns>
        private static Dictionary<string, string> GetAttributes(IWebDriver driver)
        {
            var dict = new Dictionary<string, string>();

            try
            {
                // 找到产品详情区域
                var productFactsSection = driver.FindElement(By.CssSelector("div.a-section[role='list']"));
                var productFactsDetails = productFactsSection.FindElements(By.CssSelector("div.product-facts-detail"));

                foreach (var detail in productFactsDetails)
                {
                    try
                    {
                        // 获取左侧的属性名（key）
                        var keyElement = detail.FindElement(By.CssSelector("div.a-col-left span.a-color-base"));
                        // 获取右侧的属性值（value）
                        var valueElement = detail.FindElement(By.CssSelector("div.a-col-right span.a-color-base"));

                        if (keyElement != null && valueElement != null)
                        {
                            string key = keyElement.Text.Trim();
                            string value = valueElement.Text.Trim();

                            if (!string.IsNullOrWhiteSpace(key) && !string.IsNullOrWhiteSpace(value))
                            {
                                if (!dict.ContainsKey(key))
                                    dict[key] = value;
                            }
                        }
                    }
                    catch
                    {
                        // 忽略单个属性解析失败
                    }
                }
            }
            catch
            {
                // 如果找不到产品详情区域，尝试原来的方法作为降级方案
                var rows = driver.FindElements(By.CssSelector("table.a-normal tr"));
                foreach (var row in rows)
                {
                    try
                    {
                        var keyCell = row.FindElements(By.CssSelector("td.a-span3 span")).FirstOrDefault();
                        var valueCell = row.FindElements(By.CssSelector("td.a-span9 span")).FirstOrDefault();
                        if (keyCell != null && valueCell != null)
                        {
                            string key = keyCell.Text.Trim();
                            string value = valueCell.Text.Trim();
                            if (!dict.ContainsKey(key))
                                dict[key] = value;
                        }
                    }
                    catch { }
                }
            }

            return dict;
        }

        /// <summary>
        /// 从面包屑导航中提取品类信息
        /// </summary>
        /// <param name="driver"></param>
        /// <returns></returns>
        private static string GetCategoryFromBreadcrumb(IWebDriver driver)
        {
            try
            {
                // 查找面包屑导航
                var breadcrumbDiv = driver.FindElement(By.Id("wayfinding-breadcrumbs_feature_div"));
                var breadcrumbLinks = breadcrumbDiv.FindElements(By.CssSelector("a.a-link-normal"));

                if (breadcrumbLinks.Any())
                {
                    // 获取最后一个链接的文本作为具体品类
                    var lastCategory = breadcrumbLinks.Last().Text.Trim();
                    return lastCategory;
                }
            }
            catch (Exception)
            {
                // 如果找不到面包屑导航，返回空字符串
            }

            return string.Empty;
        }

    }

    public class AmazonProductInfo
    {
        public string Title { get; set; }
        public List<string> Images { get; set; }
        public Dictionary<string, string> FivePoints { get; set; }
        public Dictionary<string, string> Attributes { get; set; }
        public string Category { get; set; } // 新增品类字段
    }
}
