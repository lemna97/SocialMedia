using Highever.SocialMedia.Common;
using Newtonsoft.Json.Linq;
using System.Drawing;
using System.Drawing.Imaging;

namespace Volvo.Release
{
    /// <summary>
    /// 主要的业务流程
    /// </summary>
    public class WorkService
    {
        public WorkService() { }

        // 使用信号量控制并发数，设置为1确保串行处理
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        public string _projectRoot = AppDomain.CurrentDomain.BaseDirectory;
        /// <summary>
        /// 运行主流程
        /// </summary>
        /// <returns></returns>
        public async Task Run()
        {
            string asin_excl = Path.Combine($"{_projectRoot}\\asin", "沃尔玛自动上架产品列表.xlsx");
            // 读取表格数据
            var asins_list = ExcelHelper.ReadTableAsProductInfoList(asin_excl);
            for (int i = 0; i < asins_list.Count; i++)
            {
                var ASIN = asins_list[i].ASIN;
                var UPC = asins_list[i].UPC;
                var SKU = asins_list[i].SKU;
                try
                {
                    await _semaphore.WaitAsync();
                    //公共的属性
                    CommonAttributes common = new CommonAttributes(ASIN, UPC, SKU);
                    //获取亚马逊ASIN数据
                    var (status, msg, asinInfo) = await GetAmazonAsinInfoRun(ASIN);
                    if (status != 1 || asinInfo == null)
                    {
                        continue;
                    }
                    if (asinInfo.Images == null || !asinInfo.Images.Any())
                    {
                        Console.WriteLine($"SKU:{ASIN}，抓取图片异常！");
                        continue;
                    }
                    //过滤
                    asinInfo.Title = asinInfo.Title.Replace("\\", "")
                              .Replace("\"", "")
                              .Replace("\n", "")
                              .Replace("\t", "")
                              .Replace("\"", "");
                    if (asinInfo.Title.Length > 138)
                    {
                        asinInfo.Title = asinInfo.Title.Substring(0, 137) + common.BrandName;
                    }
                    else
                    {
                        asinInfo.Title = asinInfo.Title +"..."+ common.BrandName;
                    }
                    //确保是主图
                    var imageurls = asinInfo.Images;
                    //请求GPT的图片
                    var img_url = GetBase64ByUrl(imageurls[0], ASIN);

                    #region 1、通过拼配URL匹配到，判断是项链/耳饰/手链/戒指
                    // 获取沃尔沃品类
                    string volvoCategorization = MapToVolvoCategorization(asinInfo.Category);

                    // 如果无法匹配到有效品类，跳过该产品
                    if (string.IsNullOrEmpty(volvoCategorization))
                    {
                        Console.WriteLine($"SKU:{ASIN}，无法识别产品品类：{asinInfo.Category}");
                        continue;
                    }

                    Console.WriteLine($"SKU:{ASIN}，亚马逊品类：{asinInfo.Category}，沃尔沃品类：{volvoCategorization}");

                    // 使用GPT生成产品标题、简介和五点描述
                    ChatGPTHelper chatGPTHelper = new ChatGPTHelper();
                    var gptresult = await chatGPTHelper.Chat($"{PromptConfig.promptTitleDesc},标题：{asinInfo.Title}", $"data:image/jpeg;base64,{await img_url}");
                    var jobjs = JsonHelper.GetJObjectDeserialize(gptresult);

                    // 提取GPT生成的内容 
                    string gptDescription = jobjs["description"]?.ToString() ?? "";
                    gptDescription = gptDescription.Replace("\\", "")
                                                     .Replace("\"", "")
                                                     .Replace("\n", "")
                                                     .Replace("\t", "")
                                                     .Replace("\"", "");
                    List<string> keyFeatures = new List<string>();

                    if (jobjs["keyFeatures"] is JArray features)
                    {
                        foreach (var feature in features)
                        {
                            var temp_feature = feature.ToString().Replace("\\", "")
                                                       .Replace("\"", "")
                                                       .Replace("\n", "")
                                                       .Replace("\t", "")
                                                       .Replace("\"", "");
                            keyFeatures.Add(temp_feature);
                        }
                    } 
                    Console.WriteLine($"SKU:{ASIN}，GPT生成简介：{gptDescription.Substring(0, Math.Min(100, gptDescription.Length))}...");
                    Console.WriteLine($"SKU:{ASIN}，GPT生成特性数量：{keyFeatures.Count}");

                    // 容错处理：如果亚马逊五点描述不足5点，使用GPT生成的keyFeatures补充
                    if (asinInfo.FivePoints != null && asinInfo.FivePoints.Count < 5 && keyFeatures.Count > 0)
                    {
                        int currentCount = asinInfo.FivePoints.Count;
                        int needCount = 5 - currentCount;
                        int availableFeatures = keyFeatures.Count - currentCount; // 可用的特性数量
                        int actualSupplementCount = Math.Min(needCount, availableFeatures); // 实际能补充的数量

                        Console.WriteLine($"SKU:{ASIN}，亚马逊五点描述只有{currentCount}点，需要补充{needCount}点");
                        Console.WriteLine($"SKU:{ASIN}，GPT生成{keyFeatures.Count}个特性，可补充{actualSupplementCount}点");

                        for (int j = 0; j < actualSupplementCount; j++)
                        {
                            string featureKey = $"Feature{currentCount + j + 1}";
                            string featureValue = keyFeatures[currentCount + j];

                            asinInfo.FivePoints[featureKey] = featureValue;
                            Console.WriteLine($"SKU:{ASIN}，补充第{currentCount + j + 1}点：{featureValue.Substring(0, Math.Min(50, featureValue.Length))}...");
                        }

                        // 如果补充后仍然不足5点，给出警告
                        if (asinInfo.FivePoints.Count < 5)
                        {
                            Console.WriteLine($"⚠️ SKU:{ASIN}，警告：补充后仍只有{asinInfo.FivePoints.Count}点描述，GPT生成的特性不足！");
                        }

                        Console.WriteLine($"SKU:{ASIN}，补充后五点描述总数：{asinInfo.FivePoints.Count}");
                    }
                    else if (asinInfo.FivePoints == null && keyFeatures.Count > 0)
                    {
                        // 如果亚马逊五点描述为空，直接使用GPT生成的前5个特性
                        asinInfo.FivePoints = new Dictionary<string, string>();
                        Console.WriteLine($"SKU:{ASIN}，亚马逊五点描述为空，使用GPT生成的特性");

                        int maxFeatures = Math.Min(5, keyFeatures.Count);
                        for (int j = 0; j < maxFeatures; j++)
                        {
                            string featureKey = $"Feature{j + 1}";
                            string featureValue = keyFeatures[j];

                            asinInfo.FivePoints[featureKey] = featureValue;
                            Console.WriteLine($"SKU:{ASIN}，添加第{j + 1}点：{featureValue.Substring(0, Math.Min(50, featureValue.Length))}...");
                        }

                        // 如果GPT生成的特性不足5点，给出警告
                        if (keyFeatures.Count < 5)
                        {
                            Console.WriteLine($"⚠️ SKU:{ASIN}，警告：GPT只生成了{keyFeatures.Count}个特性，无法达到5点描述！");
                        }

                        Console.WriteLine($"SKU:{ASIN}，最终五点描述总数：{asinInfo.FivePoints.Count}");
                    }
                    else if (asinInfo.FivePoints == null && keyFeatures.Count == 0)
                    {
                        // 如果亚马逊和GPT都没有五点描述
                        Console.WriteLine($"❌ SKU:{ASIN}，严重警告：亚马逊和GPT都没有提供五点描述！");
                        asinInfo.FivePoints = new Dictionary<string, string>();
                    }
                    else if (asinInfo.FivePoints != null && asinInfo.FivePoints.Count < 5 && keyFeatures.Count == 0)
                    {
                        // 如果亚马逊描述不足且GPT没有生成特性
                        Console.WriteLine($"❌ SKU:{ASIN}，严重警告：亚马逊只有{asinInfo.FivePoints.Count}点描述，且GPT未生成补充特性！");
                    }
                    #endregion

                    #region 2、针对不同的品类，拿到不同的 Prompt
                    string categoryPrompt = "";
                    switch (volvoCategorization)
                    {
                        case "Fashion / Jewelry / Necklaces":
                            // 项链品类
                            categoryPrompt = PromptConfig.promptNecklaceAttributes;
                            Console.WriteLine($"SKU:{ASIN}，使用项链品类提示词");
                            break;
                        
                        case "Fashion / Jewelry / Earrings":
                            // 耳饰品类
                            categoryPrompt = PromptConfig.promptEarringsAttributes;
                            Console.WriteLine($"SKU:{ASIN}，使用耳饰品类提示词");
                            break;
                            
                        case "Fashion / Jewelry / Rings":
                            // 戒指品类
                            categoryPrompt = PromptConfig.promptRingsAttributes;
                            Console.WriteLine($"SKU:{ASIN}，使用戒指品类提示词");
                            break;
                            
                        case "Fashion / Jewelry / Bracelets":
                            // 手链品类
                            categoryPrompt = PromptConfig.promptBraceletsAttributes;
                            Console.WriteLine($"SKU:{ASIN}，使用手链品类提示词");
                            break;
                            
                        default:
                            Console.WriteLine($"SKU:{ASIN}，暂未支持的品类：{volvoCategorization}");
                            continue; // 跳过不支持的品类
                    }
                    #endregion

                    #region 3、拿到 Prompt 后请求GPT 针对不同的品类，拿到不同的 分类属性
                    Dictionary<string, object> categoryAttributes = new Dictionary<string, object>();

                    if (!string.IsNullOrEmpty(categoryPrompt))
                    {
                        try
                        {
                            var categoryGptResult = await chatGPTHelper.Chat($"{categoryPrompt},标题：{asinInfo.Title}", $"data:image/jpeg;base64,{await img_url}");
                            
                            if (!string.IsNullOrEmpty(categoryGptResult))
                            {
                                var categoryJobjs = JsonHelper.GetJObjectDeserialize(categoryGptResult);
                                
                                switch (volvoCategorization)
                                {
                                    case "Fashion / Jewelry / Necklaces":
                                        // 解析项链属性
                                        // 扣环类型
                                        categoryAttributes["claspType"] = categoryJobjs["claspType"]?.ToString() ?? "Lobster Claw";
                                        // 项链风格
                                        categoryAttributes["necklaceStyle"] = categoryJobjs["necklaceStyle"]?.ToString() ?? "Pendant";
                                        // 链子长度
                                        categoryAttributes["chainLength"] = categoryJobjs["chainLength"]?.ToObject<decimal>() ?? 18m;
                                        // 链子图案
                                        categoryAttributes["chainPattern"] = categoryJobjs["chainPattern"]?.ToString() ?? "Cable";
                                        // 链子宽度
                                        categoryAttributes["chainWidthSize"] = categoryJobjs["chainWidthSize"]?.ToObject<decimal>() ?? 0.059m;
                                        // 吊坠长度
                                        categoryAttributes["dropLength"] = categoryJobjs["dropLength"]?.ToObject<decimal>() ?? 1m;
                                        
                                        Console.WriteLine($"SKU:{ASIN}，项链属性解析完成：扣环类型={categoryAttributes["claspType"]}，风格={categoryAttributes["necklaceStyle"]}");
                                        break;
                                        
                                    case "Fashion / Jewelry / Earrings":
                                        // 解析耳饰属性
                                        // 耳饰背扣
                                        categoryAttributes["earringBack"] = categoryJobjs["earringBack"]?.ToString() ?? "Friction";
                                        // 耳饰风格
                                        categoryAttributes["earringStyle"] = categoryJobjs["earringStyle"]?.ToString() ?? "Stud";
                                        // 耳饰特性
                                        categoryAttributes["earringFeature"] = "Sensitive Ear"; // 固定值
                                        
                                        Console.WriteLine($"SKU:{ASIN}，耳饰属性解析完成：背扣类型={categoryAttributes["earringBack"]}，风格={categoryAttributes["earringStyle"]}");
                                        break;
                                        
                                    case "Fashion / Jewelry / Rings":
                                        // 解析戒指属性
                                        // 戒指是否可调节
                                        categoryAttributes["isRingResizable"] = "No"; // 固定值
                                        // 戒指风格
                                        categoryAttributes["ringStyle"] = categoryJobjs["ringStyle"]?.ToString() ?? "Solitaire";
                                        // 戒指尺寸
                                        categoryAttributes["ringSize"] = categoryJobjs["ringSize"]?.ToString() ?? "7";
                                        
                                        Console.WriteLine($"SKU:{ASIN}，戒指属性解析完成：风格={categoryAttributes["ringStyle"]}，尺寸={categoryAttributes["ringSize"]}");
                                        break;
                                        
                                    case "Fashion / Jewelry / Bracelets":
                                        // 解析手链属性
                                        // 手链风格
                                        categoryAttributes["braceletStyle"] = categoryJobjs["braceletStyle"]?.ToString() ?? "Charm";
                                        // 扣环类型
                                        categoryAttributes["claspType"] = categoryJobjs["claspType"]?.ToString() ?? "Lobster Claw";
                                        // 链子长度
                                        categoryAttributes["chainLength"] = categoryJobjs["chainLength"]?.ToObject<decimal>() ?? 7m;
                                        // 链子图案
                                        categoryAttributes["chainPattern"] = categoryJobjs["chainPattern"]?.ToString() ?? "Cable";
                                        // 链子宽度
                                        categoryAttributes["chainWidthSize"] = categoryJobjs["chainWidthSize"]?.ToObject<decimal>() ?? 0.059m;
                                        
                                        Console.WriteLine($"SKU:{ASIN}，手链属性解析完成：风格={categoryAttributes["braceletStyle"]}，扣环类型={categoryAttributes["claspType"]}");
                                        break;
                                        
                                    default:
                                        Console.WriteLine($"SKU:{ASIN}，未知品类的属性解析：{volvoCategorization}");
                                        break;
                                }
                            }
                            else
                            {
                                Console.WriteLine($"SKU:{ASIN}，品类属性GPT返回结果为空");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"SKU:{ASIN}，品类属性GPT请求失败：{ex.Message}");
                        }
                    }
                    #endregion

                    #region 4、通过GPT获取通用属性
                    Dictionary<string, object> commonAttributes = new Dictionary<string, object>();

                    try
                    {
                        // 构建五点描述文本
                        string fivePointsText = "";
                        if (asinInfo.FivePoints != null && asinInfo.FivePoints.Count > 0)
                        {
                            fivePointsText = string.Join(" ", asinInfo.FivePoints.Values);
                        }

                        var commonGptResult = await chatGPTHelper.Chat($"{PromptConfig.promptCommonAttributes},标题：{asinInfo.Title},五点描述：{fivePointsText}", $"data:image/jpeg;base64,{await img_url}");
                        
                        if (!string.IsNullOrEmpty(commonGptResult))
                        {
                            var commonJobjs = JsonHelper.GetJObjectDeserialize(commonGptResult);
                            
                            // 解析通用属性
                            // 颜色
                            commonAttributes["color"] = commonJobjs["color"]?.ToString() ?? "Silver";
                            // 颜色类别
                            commonAttributes["colorCategory"] = commonJobjs["colorCategory"]?.ToString() ?? "Gold";
                            // 宝石类型
                            commonAttributes["gemstoneType"] = commonJobjs["gemstoneType"]?.ToString() ?? "Cubic Zirconia";
                            // 材质
                            commonAttributes["material"] = commonJobjs["material"]?.ToString() ?? "Silver";
                            // 金属类型
                            commonAttributes["metalType"] = commonJobjs["metalType"]?.ToString() ?? "Sterling Silver";
                            // 净重量数值
                            commonAttributes["netContentMeasure"] = commonJobjs["netContentMeasure"]?.ToObject<decimal>() ?? 2.8m;
                            // 净重量单位
                            commonAttributes["netContentUnit"] = commonJobjs["netContentUnit"]?.ToString() ?? "Gram";

                            // 产品尺寸相关（可能为null）
                            if (commonJobjs["assembledProductDepthMeasure"]?.ToObject<decimal?>() != null)
                            { 
                                // 产品深度数值
                                commonAttributes["assembledProductDepthMeasure"] = commonJobjs["assembledProductDepthMeasure"]?.ToObject<decimal?>();
                                // 产品深度单位
                                commonAttributes["assembledProductDepthUnit"] = commonJobjs["assembledProductDepthUnit"]?.ToString();
                            }

                            // 产品高度相关（可能为null）
                            if (commonJobjs["assembledProductHeightMeasure"]?.ToObject<decimal?>() != null)
                            {
                                // 产品高度数值
                                commonAttributes["assembledProductHeightMeasure"] = commonJobjs["assembledProductHeightMeasure"]?.ToObject<decimal?>();
                                // 产品高度单位
                                commonAttributes["assembledProductHeightUnit"] = commonJobjs["assembledProductHeightUnit"]?.ToString();
                            }

                            // 产品重量相关（可能为null）
                            if (commonJobjs["assembledProductWeightMeasure"]?.ToObject<decimal?>() != null)
                            {
                                // 产品重量数值
                                commonAttributes["assembledProductWeightMeasure"] = commonJobjs["assembledProductWeightMeasure"]?.ToObject<decimal?>();
                                // 产品重量单位
                                commonAttributes["assembledProductWeightUnit"] = commonJobjs["assembledProductWeightUnit"]?.ToString();
                            }

                            // 产品宽度相关（可能为null）
                            if (commonJobjs["assembledProductWidthMeasure"]?.ToObject<decimal?>() != null)
                            {
                                // 产品宽度数值
                                commonAttributes["assembledProductWidthMeasure"] = commonJobjs["assembledProductWidthMeasure"]?.ToObject<decimal?>();
                                // 产品宽度单位
                                commonAttributes["assembledProductWidthUnit"] = commonJobjs["assembledProductWidthUnit"]?.ToString();
                            }

                            // 生辰石月份
                            commonAttributes["birthstoneMonth"] = commonJobjs["birthstoneMonth"]?.ToString() ?? "April - Diamond";
                            // 包含物品
                            commonAttributes["itemsIncluded"] = commonJobjs["itemsIncluded"]?.ToString() ?? "";
                            // 珠宝镶嵌
                            commonAttributes["jewelrySetting"] = commonJobjs["jewelrySetting"]?.ToString() ?? "Pavé";

                            // 克拉相关（可能为null）
                            if (commonJobjs["karatsMeasure"]?.ToObject<decimal?>() != null)
                            {
                                // 克拉数值
                                commonAttributes["karatsMeasure"] = commonJobjs["karatsMeasure"]?.ToObject<decimal?>();
                                // 克拉单位
                                commonAttributes["karatsUnit"] = commonJobjs["karatsUnit"]?.ToString() ?? "kt";
                            }
                            // 金属印记
                            commonAttributes["metalStamp"] = commonJobjs["metalStamp"]?.ToString() ?? "925 Sterling";
                            // 宝石数量
                            commonAttributes["numberOfGemstones"] = commonJobjs["numberOfGemstones"]?.ToObject<int>() ?? 1;
                            // 珍珠数量
                            commonAttributes["numberOfPearls"] = commonJobjs["numberOfPearls"]?.ToObject<int>() ?? 0;
                            // 爪数量
                            commonAttributes["numberOfProgs"] = commonJobjs["numberOfProgs"]?.ToObject<int>() ?? 3;
                            // 图案
                            commonAttributes["pattern"] = commonJobjs["pattern"]?.ToString() ?? "Unique Design";
                            // 形状
                            commonAttributes["shape"] = commonJobjs["shape"]?.ToString() ?? "";
                            // 主题
                            commonAttributes["theme"] = commonJobjs["theme"]?.ToString() ?? "Unique";
                            
                            Console.WriteLine($"SKU:{ASIN}，通用属性解析完成：颜色={commonAttributes["color"]}，金属类型={commonAttributes["metalType"]}，宝石类型={commonAttributes["gemstoneType"]}");
                        }
                        else
                        {
                            Console.WriteLine($"SKU:{ASIN}，通用属性GPT返回结果为空");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"SKU:{ASIN}，通用属性GPT请求失败：{ex.Message}");
                    }
                    #endregion

                    #region 5、发包的属性拼装

                    #endregion

                    #region 6、沃尔沃上架SKU摸你发包

                    #endregion



                }
                catch (Exception)
                {

                }
                finally
                {
                    _semaphore.Release();
                }
            }
            await Task.CompletedTask;
            return;
        }
        /// <summary>
        /// 获取亚马逊产品详情页面信息
        /// </summary>
        /// <returns></returns>
        private async Task<(int, string, AmazonProductInfo?)> GetAmazonAsinInfoRun(string asin)
        {
            //读取cookies
            string amazonCookie = File.ReadAllText(@$"amazon_cookie.txt");
            var url = $"https://www.amazon.com/dp/{asin}?th=1";
            // 转为字典
            var cookieDict = amazonCookie.Split(';')
                .Select(t => t.Trim().Split('='))
                .Where(kv => kv.Length == 2)
                .ToDictionary(kv => kv[0], kv => kv[1]);
            var amazoninfo = AmazonProductHelper.ParseAmazonProduct(url, cookieDict);
            if (amazoninfo == null)
            {
                Console.WriteLine($"Asin:{asin}，亚马逊找不到该产品！");
                return (0, "亚马逊找不到该产品", null);
            }
            await Task.CompletedTask;
            return (1, string.Empty, amazoninfo);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="imageUrl"></param> 
        /// <param name="name"></param>
        /// <param name="itemNo"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private async Task<string> GetBase64ByUrl(string imageUrl, string itemNo)
        {
            // 确保img_file目录存在
            string imgDirectory = Path.Combine(_projectRoot, "img_file");
            if (!Directory.Exists(imgDirectory))
            {
                Directory.CreateDirectory(imgDirectory);
            }

            // 从URL中提取文件扩展名
            string fileExtension = GetImageExtensionFromUrl(imageUrl);

            // 构建完整的文件路径
            string localFilePath = Path.Combine(imgDirectory, $"{itemNo}{fileExtension}");

            string base64String = string.Empty;
            try
            {
                byte[] imageBytes;

                if (File.Exists(localFilePath))
                {
                    // 如果文件存在，从现有文件读取字节
                    imageBytes = await File.ReadAllBytesAsync(localFilePath);
                    // 调整图像大小到800×800
                    byte[] resizedImageBytes = ResizeImageTo800(imageBytes);

                    // 将调整后的图像保存到本地文件（可选）
                    await File.WriteAllBytesAsync(localFilePath, resizedImageBytes);

                    // 将调整后的图像数据转换为Base64字符串
                    base64String = Convert.ToBase64String(resizedImageBytes);
                }
                else if (File.Exists(localFilePath.Replace(".jpg", ".png")))
                {
                    imageBytes = await File.ReadAllBytesAsync(localFilePath.Replace(".jpg", ".png"));
                    // 调整图像大小到800×800
                    byte[] resizedImageBytes = ResizeImageTo800(imageBytes);

                    // 将调整后的图像保存到本地文件（可选）
                    await File.WriteAllBytesAsync(localFilePath, resizedImageBytes);

                    // 将调整后的图像数据转换为Base64字符串
                    base64String = Convert.ToBase64String(resizedImageBytes);
                }
                else
                {
                    using (HttpClient client = new HttpClient())
                    {
                        client.Timeout = new TimeSpan(0, 0, 30); // 设置请求超时
                        int retryCount = 2; // 重试次数
                        bool isDownloaded = false;

                        for (int attempt = 0; attempt <= retryCount; attempt++)
                        {
                            try
                            {
                                // 从URL下载图像数据
                                imageBytes = await client.GetByteArrayAsync(imageUrl);

                                // 保存原始图像数据到本地文件
                                await File.WriteAllBytesAsync(localFilePath, imageBytes);
                                isDownloaded = true; // 下载成功

                                // 调整图像大小到800×800
                                byte[] resizedImageBytes = ResizeImageTo800(imageBytes);

                                // 将调整后的图像保存到本地文件
                                await File.WriteAllBytesAsync(localFilePath, resizedImageBytes);

                                // 将调整后的图像数据转换为Base64字符串
                                base64String = Convert.ToBase64String(resizedImageBytes);

                                Console.WriteLine($"图片下载成功: {localFilePath}");
                                break; // 退出重试循环
                            }
                            catch (HttpRequestException httpEx)
                            {
                                Console.WriteLine($"下载失败 ({attempt + 1}/{retryCount + 1}): {httpEx.Message}");

                                // 如果这是最后一次尝试，则抛出异常
                                if (attempt == retryCount)
                                {
                                    throw;
                                }

                                // 等待一段时间后再重试（可选）
                                await Task.Delay(1000); // 等待1秒
                            }
                        }

                        if (!isDownloaded)
                        {
                            throw new Exception("下载图片失败");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"发生错误: {ex.Message}");
            }

            return base64String;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="imageBytes"></param>
        /// <returns></returns>
        private byte[] ResizeImageTo800(byte[] imageBytes)
        {
            using (var inputStream = new MemoryStream(imageBytes))
            {
                using (var image = Image.FromStream(inputStream))
                {
                    int width = image.Width;
                    int height = image.Height;

                    // 仅当图像尺寸大于800×800时才调整大小
                    if (width <= 800 && height <= 800)
                    {
                        // 不需要调整大小，返回原始字节数据
                        return imageBytes;
                    }

                    float scale = Math.Min(800f / width, 800f / height);
                    int newWidth = (int)(width * scale);
                    int newHeight = (int)(height * scale);

                    using (var bitmap = new Bitmap(newWidth, newHeight))
                    {
                        using (var graphics = Graphics.FromImage(bitmap))
                        {
                            graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                            graphics.DrawImage(image, 0, 0, newWidth, newHeight);
                        }
                        using (var outputStream = new MemoryStream())
                        {
                            // 保存图像，保持原始格式
                            ImageFormat format = image.RawFormat;
                            bitmap.Save(outputStream, format);
                            return outputStream.ToArray();
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 将亚马逊品类映射到沃尔沃品类
        /// </summary>
        /// <param name="amazonCategory">亚马逊品类</param>
        /// <returns>沃尔沃品类</returns>
        private string MapToVolvoCategorization(string amazonCategory)
        {
            if (string.IsNullOrEmpty(amazonCategory))
                return string.Empty;

            var category = amazonCategory.ToLower();

            // 项链相关
            if (category.Contains("necklace") || category.Contains("pendant"))
            {
                return "Fashion / Jewelry / Necklaces";
            }
            // 耳饰相关
            else if (category.Contains("earring") || category.Contains("stud") || category.Contains("hoop"))
            {
                return "Fashion / Jewelry / Earrings";
            }
            // 戒指相关
            else if (category.Contains("ring"))
            {
                return "Fashion / Jewelry / Rings";
            }
            // 手链相关
            else if (category.Contains("bracelet") || category.Contains("bangle"))
            {
                return "Fashion / Jewelry / Bracelets";
            }

            // 如果都不匹配，返回空字符串
            return string.Empty;
        }
        /// <summary>
        /// 从URL中提取图片扩展名
        /// </summary>
        /// <param name="imageUrl">图片URL</param>
        /// <returns>文件扩展名（包含点号）</returns>
        private string GetImageExtensionFromUrl(string imageUrl)
        {
            try
            {
                // 移除URL参数部分
                string urlWithoutParams = imageUrl.Split('?')[0];

                // 获取文件扩展名
                string extension = Path.GetExtension(urlWithoutParams).ToLower();

                // 如果没有扩展名或扩展名为空，默认使用.jpg
                if (string.IsNullOrEmpty(extension))
                {
                    extension = ".jpg";
                }

                // 确保是常见的图片格式
                string[] validExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };
                if (!validExtensions.Contains(extension))
                {
                    extension = ".jpg"; // 默认使用jpg
                }

                Console.WriteLine($"从URL提取的扩展名: {extension}");
                return extension;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"提取扩展名失败: {ex.Message}，使用默认扩展名.jpg");
                return ".jpg";
            }
        }
    }
}
