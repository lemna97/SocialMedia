using Highever.SocialMedia.Common.Model;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Highever.SocialMedia.Common
{
    public class HttpClientHelper
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly INLogger _Logger;

        /// <summary>
        /// 构造函数注入 IHttpClientFactory
        /// </summary>
        /// <param name="httpClientFactory">IHttpClientFactory 实例</param>
        /// <param name="logger"></param>
        public HttpClientHelper(IHttpClientFactory httpClientFactory, INLogger logger)
        {
            _httpClientFactory = httpClientFactory;
            _Logger = logger;
        }

        /// <summary>
        /// 向目标地址提交图片文件参数数据
        /// </summary>
        /// <param name="requestUrl">请求地址</param>
        /// <param name="bmpBytes">图片字节流</param>
        /// <param name="imgType">上传图片类型</param>
        /// <param name="fileName">图片名称</param>
        /// <returns>服务器返回的响应内容</returns>
        public async Task<string> PostImageAsync(string requestUrl, byte[] bmpBytes, string imgType, string fileName)
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                using var content = new MultipartFormDataContent();

                // 添加 imgType 字段
                content.Add(new StringContent(imgType), "imgType");

                // 添加文件字段
                var fileContent = new ByteArrayContent(bmpBytes);
                fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
                {
                    Name = "file",
                    FileName = fileName
                };
                content.Add(fileContent);

                // 发送 POST 请求
                var response = await httpClient.PostAsync(requestUrl, content);
                response.EnsureSuccessStatusCode(); // 如果非 2xx 状态码抛出异常
                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                throw new HttpRequestException($"上传图片失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 发送 JSON 格式的 POST 请求
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="postData">POST 数据（JSON 格式）</param>
        /// <returns>响应结果</returns>
        public async Task<string> PostAsync(string url, string postData)
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                using var content = CreateJsonContent(postData);

                var response = await httpClient.PostAsync(url, content);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                throw new HttpRequestException($"POST 请求失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <param name="postData"></param>
        /// <param name="headers"></param>
        /// <param name="mediaType"></param>
        /// <param name="isLogrecord"></param>
        /// <returns></returns>
        /// <exception cref="HttpRequestException"></exception>
        public async Task<T?> PostAsync<T>(string url, string postData, Dictionary<string, string>? headers = null, string mediaType = "application/json", bool isLogrecord = false) where T : class
        {
            string jsonResponse = string.Empty;
            try
            {
                using var httpClient = new HttpClient();
                using var request = new HttpRequestMessage(HttpMethod.Post, url);

                // 设置 POST 数据，并同时指定 Content-Type
                request.Content = new StringContent(postData, Encoding.UTF8, mediaType);

                // 添加 Headers
                if (headers != null)
                {
                    foreach (var header in headers)
                    {
                        if (header.Key.Equals("Authorization", StringComparison.OrdinalIgnoreCase))
                        {
                            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", header.Value);
                        }
                        else
                        {
                            request.Headers.Add(header.Key, header.Value);
                        }
                    }
                }

                // 发送请求
                var response = await httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();
                // 读取并解析响应内容
                jsonResponse = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(jsonResponse);
            }
            catch (Exception ex)
            {
                throw new HttpRequestException($"POST请求返回泛型对象失败: {ex.Message}", ex);
            }
            finally
            {
                if (isLogrecord)
                {
                    //待补充完善
                    _Logger.ApiInfo(url);
                }
            }
        }
         
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <param name="queryParams"></param>
        /// <param name="headers"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        /// <exception cref="ApiException"></exception>
        public async Task<T?> GetAsync<T>(
    string url,
    object? queryParams = null,
    Dictionary<string, string>? headers = null,
    CancellationToken ct = default) where T : class
        {
            // 1. 拼接查询字符串（自动转义）
            static string Kv(string k, object? v) =>
                $"{Uri.EscapeDataString(k)}={Uri.EscapeDataString(v?.ToString() ?? string.Empty)}";

            string query = queryParams switch
            {
                null => string.Empty,
                string s when !string.IsNullOrWhiteSpace(s) => s,
                IDictionary<string, object?> dict => string.Join('&', dict.Select(kv => Kv(kv.Key, kv.Value))),
                _ => string.Join('&',
                                                       queryParams.GetType().GetProperties()
                                                                  .Select(p => Kv(p.Name, p.GetValue(queryParams))))
            };

            string fullUrl = string.IsNullOrWhiteSpace(query)
                ? url
                : $"{url}{(url.Contains('?') ? "&" : "?")}{query}";

            // 2. 构造 HttpRequestMessage（避免 DefaultHeaders 污染其它请求）
            using var request = new HttpRequestMessage(HttpMethod.Get, fullUrl);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (headers is { Count: > 0 })
            {
                foreach (var kv in headers)
                {
                    if (kv.Key.Equals("Authorization", StringComparison.OrdinalIgnoreCase))
                        request.Headers.Authorization = new AuthenticationHeaderValue(
                            "Bearer", kv.Value.Replace("Bearer ", "", StringComparison.OrdinalIgnoreCase));
                    else
                        request.Headers.TryAddWithoutValidation(kv.Key, kv.Value);
                }
            }

            using var httpClient = _httpClientFactory.CreateClient();

            // 3. 发送
            using var response = await httpClient.SendAsync(request, ct);
            var json = await response.Content.ReadAsStringAsync(ct);

            // 4. 成功路径
            if (response.IsSuccessStatusCode)
            {
                if (typeof(T) == typeof(string))
                    return (T)(object)json; // 原始文本

                return JsonSerializer.Deserialize<T>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }

            // 5. 解析错误体（若符合 FastAPI 格式）
            ApiErrorResponse? apiErr = null;
            try
            {
                apiErr = JsonSerializer.Deserialize<ApiErrorResponse>(json);
            }
            catch { /* ignore */ }

            throw new ApiException(
                response.StatusCode,
                $"GET {fullUrl} failed with {(int)response.StatusCode} {response.ReasonPhrase}",
                apiErr);
        }


        /// <summary>
        /// 发送 GET 请求并解析为泛型对象
        /// </summary>
        /// <typeparam name="T">响应数据的类型</typeparam>
        /// <param name="url">请求地址</param>
        /// <returns>反序列化后的响应对象</returns>
        public async Task<T?> GetAsync<T>(string url) where T : class
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var response = await httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(json);
            }
            catch (Exception ex)
            {
                throw new HttpRequestException($"GET 请求失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 发送 PUT 请求，提交 JSON 数据
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="putData">PUT 数据（JSON 格式）</param>
        /// <returns>响应结果</returns>
        public async Task<string> PutAsync(string url, string putData)
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                using var content = CreateJsonContent(putData);

                var response = await httpClient.PutAsync(url, content);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                throw new HttpRequestException($"PUT 请求失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 发送 PUT 请求并解析为泛型对象
        /// </summary>
        /// <typeparam name="T">响应数据的类型</typeparam>
        /// <param name="url">请求地址</param>
        /// <param name="putData">PUT 数据（JSON 格式）</param>
        /// <returns>反序列化后的响应对象</returns>
        public async Task<T?> PutAsync<T>(string url, string putData) where T : class
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                using var content = CreateJsonContent(putData);

                var response = await httpClient.PutAsync(url, content);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(json);
            }
            catch (Exception ex)
            {
                throw new HttpRequestException($"PUT 请求返回泛型对象失败: {ex.Message}", ex);
            }
        }

        #region 辅助方法

        /// <summary>
        /// 创建 JSON 格式的 HttpContent
        /// </summary>
        private static StringContent CreateJsonContent(string data, Encoding? encoding = null)
        {
            return new StringContent(data, encoding ?? Encoding.UTF8, "application/json");
        }

        /// <summary>
        /// 将对象转换为查询字符串格式
        /// </summary>
        /// <param name="src">源对象</param>
        /// <returns>查询字符串</returns>
        public static string BuildQueryString(object? src)
        {
            if (src == null) return string.Empty;

            return src switch
            {
                string s when !string.IsNullOrWhiteSpace(s) => s,
                IDictionary<string, object?> dict => string.Join('&', 
                    dict.Select(kv => $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value?.ToString() ?? string.Empty)}")),
                _ => string.Join('&',
                    src.GetType().GetProperties()
                       .Where(p => p.GetValue(src) != null)
                       .Select(p => $"{Uri.EscapeDataString(p.Name)}={Uri.EscapeDataString(p.GetValue(src)!.ToString()!)}"))
            };
        }

        #endregion

        /// <summary>
        /// 发送 GET 请求并返回字符串响应
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <returns>响应字符串</returns>
        public async Task<string> GetAsync(string url)
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                var response = await httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                throw new HttpRequestException($"GET 请求失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 发送 GET 请求并返回字符串响应（支持自定义Headers）
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="headers">请求头</param>
        /// <param name="timeout">超时时间</param>
        /// <returns>响应字符串</returns>
        public async Task<string> GetAsync(string url, Dictionary<string, string>? headers = null, TimeSpan? timeout = null)
        {
            try
            {
                using var httpClient = _httpClientFactory.CreateClient();
                
                // 设置超时
                if (timeout.HasValue)
                {
                    httpClient.Timeout = timeout.Value;
                }

                using var request = new HttpRequestMessage(HttpMethod.Get, url);

                // 添加Headers
                if (headers != null)
                {
                    foreach (var header in headers)
                    {
                        if (header.Key.Equals("Authorization", StringComparison.OrdinalIgnoreCase))
                        {
                            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", 
                                header.Value.Replace("Bearer ", "", StringComparison.OrdinalIgnoreCase));
                        }
                        else
                        {
                            request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                        }
                    }
                }

                var response = await httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                throw new HttpRequestException($"GET 请求失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 发送 GET 请求并返回字符串响应（支持查询参数）
        /// </summary>
        /// <param name="url">基础URL</param>
        /// <param name="queryParams">查询参数对象</param>
        /// <param name="headers">请求头</param>
        /// <param name="timeout">超时时间</param>
        /// <returns>响应字符串</returns>
        public async Task<string> GetWithQueryAsync(string url, object? queryParams = null, 
            Dictionary<string, string>? headers = null, TimeSpan? timeout = null)
        {
            // 构建查询字符串
            string query = BuildQueryString(queryParams);
            string fullUrl = string.IsNullOrWhiteSpace(query) 
                ? url 
                : $"{url}{(url.Contains('?') ? "&" : "?")}{query}";

            return await GetAsync(fullUrl, headers, timeout);
        }

        /// <summary>
        /// 下载图片并保存到本地（支持HEIC转换）
        /// </summary>
        /// <param name="imageUrl">图片URL地址</param>
        /// <param name="saveDirectory">保存目录（相对于wwwroot）</param>
        /// <param name="fileName">文件名（不含扩展名）</param>
        /// <param name="convertHeic">是否转换HEIC格式</param>
        /// <param name="compressImage">是否压缩图片</param>
        /// <param name="overwriteExisting">是否覆盖已存在的文件，默认true</param>
        /// <returns>返回相对路径</returns>
        public async Task<string> DownloadAndSaveImageAsync(string imageUrl, string saveDirectory = "uploads/avatars", 
            string fileName = null, bool convertHeic = true, bool compressImage = true, bool overwriteExisting = true)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(imageUrl))
                    throw new ArgumentException("图片URL不能为空", nameof(imageUrl));

                if (!HostingEnvironmentHelper.IsConfigured)
                {
                    throw new InvalidOperationException("HostingEnvironmentHelper未初始化，无法获取Web根路径。请确保在Startup.cs的Configure方法中调用了app.UseStaticHostEnviroment()");
                }

                // 获取原始文件扩展名
                var originalExtension = Path.GetExtension(imageUrl.Split('?')[0])?.ToLower() ?? ".jpg";
                if (string.IsNullOrEmpty(originalExtension) || originalExtension == ".")
                    originalExtension = ".jpg";

                // 生成文件名
                if (string.IsNullOrEmpty(fileName))
                {
                    fileName = $"avatar_{DateTime.Now:yyyyMMddHHmmss}_{Guid.NewGuid():N}";
                }

                // 构建保存路径
                var webRootPath = HostingEnvironmentHelper.WebPath;
                var fullSaveDirectory = Path.Combine(webRootPath, saveDirectory);
                
                // 确保目录存在
                if (!Directory.Exists(fullSaveDirectory))
                {
                    Directory.CreateDirectory(fullSaveDirectory);
                }

                // 预估最终扩展名（可能会因为转换而改变）
                var estimatedExtension = (convertHeic && ImageHelper.NeedsConversion(originalExtension)) ? ".jpg" : originalExtension;
                var fullFileName = fileName + estimatedExtension;
                var fullFilePath = Path.Combine(fullSaveDirectory, fullFileName);

                // 检查文件是否已存在
                if (!overwriteExisting && File.Exists(fullFilePath))
                {
                    var relativePath = $"/{saveDirectory}/{fullFileName}".Replace("\\", "/");
                    _Logger?.ApiInfo($"文件已存在，跳过下载: {relativePath}");
                    return relativePath;
                }

                var httpClient = _httpClientFactory.CreateClient();
                httpClient.Timeout = TimeSpan.FromMinutes(2);

                // 下载图片
                var response = await httpClient.GetAsync(imageUrl);
                response.EnsureSuccessStatusCode();
                
                var imageBytes = await response.Content.ReadAsByteArrayAsync();
                var finalBytes = imageBytes;
                var finalExtension = originalExtension;

                // 处理HEIC格式转换
                if (convertHeic && ImageHelper.NeedsConversion(originalExtension))
                {
                    _Logger?.ApiInfo($"检测到HEIC格式图片，开始转换: {imageUrl}");
                    finalBytes = await ImageHelper.ConvertHeicToJpgAsync(imageBytes, 85);
                    finalExtension = ".jpg";
                    _Logger?.ApiInfo($"HEIC转换完成，原大小: {imageBytes.Length} bytes，新大小: {finalBytes.Length} bytes");
                }
                // 压缩图片（可选）
                else if (compressImage && ImageHelper.IsSupportedFormat(originalExtension))
                {
                    try
                    {
                        var compressedBytes = await ImageHelper.CompressImageAsync(finalBytes, 1920, 1080, 85);
                        if (compressedBytes.Length < finalBytes.Length)
                        {
                            finalBytes = compressedBytes;
                            finalExtension = ".jpg"; // 压缩后统一为JPG格式
                            _Logger?.ApiInfo($"图片压缩完成，原大小: {imageBytes.Length} bytes，新大小: {finalBytes.Length} bytes");
                        }
                    }
                    catch (Exception ex)
                    {
                        _Logger?.ApiInfo($"图片压缩失败，使用原图: {ex.Message}");
                    }
                }

                // 重新计算最终文件名（如果扩展名发生了变化）
                var actualFullFileName = fileName + finalExtension;
                var actualFullFilePath = Path.Combine(fullSaveDirectory, actualFullFileName);

                // 如果最终文件名与预估不同，再次检查是否存在
                if (!overwriteExisting && actualFullFilePath != fullFilePath && File.Exists(actualFullFilePath))
                {
                    var relativePath = $"/{saveDirectory}/{actualFullFileName}".Replace("\\", "/");
                    _Logger?.ApiInfo($"转换后文件已存在，跳过保存: {relativePath}");
                    return relativePath;
                }

                // 保存文件
                await File.WriteAllBytesAsync(actualFullFilePath, finalBytes);
                
                // 返回相对路径
                var finalRelativePath = $"/{saveDirectory}/{actualFullFileName}".Replace("\\", "/");
                
                _Logger?.ApiInfo($"图片保存成功: {finalRelativePath}，文件大小: {finalBytes.Length} bytes");
                
                return finalRelativePath;
            }
            catch (Exception ex)
            {
                _Logger?.ApiError($"下载并保存图片失败: {imageUrl}, 错误: {ex.Message}");
                throw new Exception($"下载并保存图片失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 检查本地文件是否存在
        /// </summary>
        /// <param name="saveDirectory">保存目录</param>
        /// <param name="fileName">文件名（不含扩展名）</param>
        /// <param name="possibleExtensions">可能的扩展名列表</param>
        /// <returns>存在的文件相对路径，不存在返回null</returns>
        public string? CheckLocalFileExists(string saveDirectory, string fileName, params string[] possibleExtensions)
        {
            try
            {
                if (!HostingEnvironmentHelper.IsConfigured)
                    return null;

                var webRootPath = HostingEnvironmentHelper.WebPath;
                var fullSaveDirectory = Path.Combine(webRootPath, saveDirectory);

                if (!Directory.Exists(fullSaveDirectory))
                    return null;

                // 如果没有指定扩展名，使用常见的图片扩展名
                if (possibleExtensions == null || possibleExtensions.Length == 0)
                {
                    possibleExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                }

                foreach (var ext in possibleExtensions)
                {
                    var fullFileName = fileName + ext;
                    var fullFilePath = Path.Combine(fullSaveDirectory, fullFileName);
                    
                    if (File.Exists(fullFilePath))
                    {
                        return $"/{saveDirectory}/{fullFileName}".Replace("\\", "/");
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                _Logger?.ApiError($"检查本地文件失败: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 智能下载图片（先检查本地是否存在）- 通用版本
        /// </summary>
        /// <param name="imageUrl">图片URL</param>
        /// <param name="fileName">文件名（不含扩展名）</param>
        /// <param name="saveDirectory">保存目录</param>
        /// <param name="convertHeic">是否转换HEIC格式</param>
        /// <param name="compressImage">是否压缩图片</param>
        /// <returns>本地文件相对路径</returns>
        public async Task<string> SmartDownloadImageAsync(string imageUrl, string fileName, 
            string saveDirectory = "uploads/images", bool convertHeic = true, bool compressImage = true)
        {
            // 先检查本地是否已存在
            var existingFile = CheckLocalFileExists(saveDirectory, fileName);
            if (!string.IsNullOrEmpty(existingFile))
            {
                _Logger?.ApiInfo($"图片文件已存在: {existingFile}");
                return existingFile;
            }

            // 本地不存在，下载新的
            return await DownloadAndSaveImageAsync(imageUrl, saveDirectory, fileName, 
                convertHeic: convertHeic, compressImage: compressImage, overwriteExisting: true);
        }

        /// <summary>
        /// 智能下载头像（专用于用户头像）
        /// </summary>
        /// <param name="imageUrl">图片URL</param>
        /// <param name="uniqueId">用户唯一ID</param>
        /// <param name="saveDirectory">保存目录</param>
        /// <returns>本地文件相对路径</returns>
        public async Task<string> SmartDownloadAvatarAsync(string imageUrl, string uniqueId, 
            string saveDirectory = "uploads/avatars")
        {
            var fileName = $"user_{uniqueId}";
            return await SmartDownloadImageAsync(imageUrl, fileName, saveDirectory);
        }

        /// <summary>
        /// 智能下载视频封面（专用于视频封面）
        /// </summary>
        /// <param name="imageUrl">图片URL</param>
        /// <param name="videoId">视频ID</param>
        /// <param name="saveDirectory">保存目录</param>
        /// <returns>本地文件相对路径</returns>
        public async Task<string> SmartDownloadVideoCoverAsync(string imageUrl, string videoId, 
            string saveDirectory = "uploads/video_coverurl")
        {
            var fileName = $"video_{videoId}";
            return await SmartDownloadImageAsync(imageUrl, fileName, saveDirectory);
        }
    }
}
