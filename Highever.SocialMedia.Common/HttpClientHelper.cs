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

        // ==================  放在 HttpClientHelper 里  ==================

        // 把任意对象拼成 key=value&key2=value2
        private static string BuildQueryString(object? src)
        {
            if (src == null) return string.Empty;

            var props = src.GetType().GetProperties()
                           .Where(p => p.GetValue(src) != null)
                           .Select(p =>
                               $"{Uri.EscapeDataString(p.Name)}={Uri.EscapeDataString(p.GetValue(src)!.ToString()!)}");

            return string.Join("&", props);
        }

        /// <summary> 
        /// 
        /// </summary>
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


        #endregion
    }
}
