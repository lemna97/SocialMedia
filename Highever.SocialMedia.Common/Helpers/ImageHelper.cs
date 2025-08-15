using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Openize.Heic;
using Openize.Heic.Decoder;

namespace Highever.SocialMedia.Common
{ 
    /// <summary>
    /// 图片处理工具（下载/保存/压缩/格式转换），集成 HEIF 编码。
    /// 说明：
    /// - 依赖 IHttpClientFactory
    /// - 使用 HostingEnvironmentHelper.WebPath 获取 wwwroot 路径（保持你的现有约定）
    /// </summary>
    public class ImageHelper
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly INLogger? _Logger;

        public ImageHelper(IHttpClientFactory httpClientFactory, INLogger? logger = null)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _Logger = logger;
        }

        /// <summary>
        /// 支持的图片格式（作为输入/解码）
        /// </summary>
        private static readonly string[] SupportedFormats = { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };

        /// <summary>
        /// 需要进行解码转换的源格式（常见手机/苹果端）
        /// </summary>
        private static readonly string[] ConvertFormats = { ".heic", ".heif" };

        /// <summary>
        /// 检查是否需要从 HEIC/HEIF 转 JPG（用于输入兼容）
        /// </summary>
        public static bool NeedsConversion(string extension)
        {
            return ConvertFormats.Contains(extension?.ToLowerInvariant() ?? string.Empty);
        }

        /// <summary>
        /// 检查是否为支持的输入图片格式
        /// </summary>
        public static bool IsSupportedFormat(string extension)
        {
            extension = extension?.ToLowerInvariant() ?? string.Empty;
            return SupportedFormats.Contains(extension) || ConvertFormats.Contains(extension);
        }

        /// <summary>
        /// 将 HEIC/HEIF 解码并转为 JPG 字节（使用 Openize.HEIC）
        /// https://products.openize.com/zh/heic/net/
        /// </summary>
        public static async Task<byte[]> ConvertHeicToJpgAsync(byte[] inputBytes, int quality = 85)
        {
            try
            {
                return await Task.Run(() =>
                {
                    using var inputStream = new MemoryStream(inputBytes);

                    // 使用 Openize.HEIC 加载 HEIC 图像
                    var heicImage = HeicImage.Load(inputStream);

                    // 获取图像尺寸
                    var width = (int)heicImage.Width;
                    var height = (int)heicImage.Height;

                    // 提取像素数据（ARGB32 格式）
                    var pixels = heicImage.GetInt32Array(PixelFormat.Argb32);

                    // 创建 ImageSharp 图像对象
                    using var image = new Image<Argb32>(width, height);

                    // 使用正确的像素访问方法
                    image.ProcessPixelRows(accessor =>
                    {
                        for (int y = 0; y < height; y++)
                        {
                            var pixelRow = accessor.GetRowSpan(y);
                            for (int x = 0; x < width; x++)
                            {
                                var pixelIndex = y * width + x;
                                var argb = pixels[pixelIndex];

                                pixelRow[x] = new Argb32(
                                    (byte)((argb >> 16) & 0xFF), // R
                                    (byte)((argb >> 8) & 0xFF),  // G
                                    (byte)(argb & 0xFF),         // B
                                    (byte)((argb >> 24) & 0xFF)  // A
                                );
                            }
                        }
                    });

                    // 转换为 JPG
                    using var outputStream = new MemoryStream();
                    var encoder = new JpegEncoder { Quality = Math.Clamp(quality, 1, 100) };
                    image.SaveAsJpeg(outputStream, encoder);

                    return outputStream.ToArray();
                });
            }
            catch (Exception ex)
            {
                throw new Exception($"Openize.HEIC 转 JPG 失败：{ex.Message}", ex);
            }
        }

        /// <summary>
        /// 多重备用 HEIC 转 JPG 方案
        /// </summary>
        /// <param name="inputBytes">输入字节</param>
        /// <param name="quality">质量</param>
        /// <returns>JPG 字节</returns>
        public static async Task<byte[]> ConvertHeicToJpgWithFallbackAsync(byte[] inputBytes, int quality = 85)
        {
            var exceptions = new List<Exception>();

            // 方案1：Openize.HEIC（推荐）
            try
            {
                return await ConvertHeicToJpgAsync(inputBytes, quality);
            }
            catch (Exception ex)
            {
                exceptions.Add(new Exception($"Openize.HEIC 转换失败: {ex.Message}", ex));
            }

            // 方案2：尝试直接用 ImageSharp（可能有些 HEIC 文件支持）
            try
            {
                using var image = Image.Load<Rgba32>(inputBytes);
                using var ms = new MemoryStream();
                var encoder = new JpegEncoder { Quality = Math.Clamp(quality, 1, 100) };
                await image.SaveAsJpegAsync(ms, encoder);
                return ms.ToArray();
            }
            catch (Exception ex)
            {
                exceptions.Add(new Exception($"ImageSharp 直接转换失败: {ex.Message}", ex));
            }

            // 所有方案都失败
            var errorMessage = string.Join("; ", exceptions.Select(e => e.Message));
            throw new Exception($"HEIC 转换失败，尝试了多种方案: {errorMessage}");
        }

        /// <summary>
        /// 图片压缩（保持宽高比；输出 JPG）
        /// </summary>
        public static async Task<byte[]> CompressImageAsync(byte[] inputBytes, int maxWidth = 1920, int maxHeight = 1080, int quality = 85)
        {
            try
            {
                using var image = Image.Load<Rgba32>(inputBytes);

                (int newW, int newH) = CalculateNewSize(image.Width, image.Height, maxWidth, maxHeight);
                if (newW != image.Width || newH != image.Height)
                {
                    image.Mutate(x => x.Resize(newW, newH));
                }

                using var ms = new MemoryStream();
                var encoder = new JpegEncoder { Quality = Math.Clamp(quality, 1, 100) };
                await image.SaveAsJpegAsync(ms, encoder);
                return ms.ToArray();
            }
            catch (Exception ex)
            {
                throw new Exception($"图片压缩失败：{ex.Message}", ex);
            }
        }

        private static (int width, int height) CalculateNewSize(int originalWidth, int originalHeight, int maxWidth, int maxHeight)
        {
            if (originalWidth <= maxWidth && originalHeight <= maxHeight)
                return (originalWidth, originalHeight);

            var ratioX = (double)maxWidth / originalWidth;
            var ratioY = (double)maxHeight / originalHeight;
            var ratio = Math.Min(ratioX, ratioY);

            return ((int)(originalWidth * ratio), (int)(originalHeight * ratio));
        }

        /// <summary>
        /// 下载图片并保存（移除HEIF编码，只保留常规格式）
        /// </summary>
        /// <param name="imageUrl">图片URL地址</param>
        /// <param name="saveDirectory">保存目录（相对于wwwroot）</param>
        /// <param name="fileName">文件名（不含扩展名），为空则自动生成</param>
        /// <param name="convertHeic">是否对 HEIC/HEIF 输入做转 JPG</param>
        /// <param name="compressImage">是否压缩图片</param>
        /// <param name="maxWidth">最大宽度</param>
        /// <param name="maxHeight">最大高度</param>
        /// <param name="quality">JPG质量</param>
        /// <returns>返回相对路径</returns>
        public async Task<string> DownloadAndSaveImageAsync(
            string imageUrl,
            string saveDirectory = "uploads/avatars",
            string? fileName = null,
            bool convertHeic = true,
            bool compressImage = true,
            int maxWidth = 1920,
            int maxHeight = 1080,
            int quality = 85)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(imageUrl))
                    throw new ArgumentException("图片URL不能为空", nameof(imageUrl));

                if (!HostingEnvironmentHelper.IsConfigured)
                    throw new InvalidOperationException("HostingEnvironmentHelper未初始化。请在 Startup 中调用 app.UseStaticHostEnviroment()。");

                var httpClient = _httpClientFactory.CreateClient();
                httpClient.Timeout = TimeSpan.FromMinutes(2);

                // 下载
                var response = await httpClient.GetAsync(imageUrl);
                response.EnsureSuccessStatusCode();
                var imageBytes = await response.Content.ReadAsByteArrayAsync();

                // 获取原始扩展名
                var originalExtension = Path.GetExtension(imageUrl.Split('?')[0])?.ToLowerInvariant() ?? ".jpg";
                if (string.IsNullOrEmpty(originalExtension) || originalExtension == ".")
                    originalExtension = ".jpg";

                var finalBytes = imageBytes;
                var finalExtension = originalExtension;

                // 1) HEIC 输入 → 转 JPG
                if (convertHeic && NeedsConversion(originalExtension))
                {
                    _Logger?.ApiInfo($"检测到 HEIC/HEIF 输入，开始使用 Openize.HEIC 转为 JPG：{imageUrl}");
                    finalBytes = await ConvertHeicToJpgAsync(finalBytes, quality);
                    finalExtension = ".jpg";
                    _Logger?.ApiInfo($"HEIC→JPG 完成，原大小 {imageBytes.Length} bytes，新大小 {finalBytes.Length} bytes");
                }

                // 2) 压缩处理
                if (compressImage && IsSupportedFormat(finalExtension))
                {
                    try
                    {
                        var compressed = await CompressImageAsync(finalBytes, maxWidth, maxHeight, quality);
                        if (compressed.Length < finalBytes.Length)
                        {
                            finalBytes = compressed;
                            finalExtension = ".jpg";
                            _Logger?.ApiInfo($"压缩完成，原大小 {imageBytes.Length} bytes，新大小 {finalBytes.Length} bytes");
                        }
                    }
                    catch (Exception ex)
                    {
                        _Logger?.ApiInfo($"图片压缩失败，使用原图：{ex.Message}");
                    }
                }

                // 3) 生成文件名并保存
                if (string.IsNullOrWhiteSpace(fileName))
                {
                    fileName = $"avatar_{DateTime.Now:yyyyMMddHHmmss}_{Guid.NewGuid():N}";
                }
                var fullFileName = fileName + finalExtension;

                var webRootPath = HostingEnvironmentHelper.WebPath;
                var fullSaveDirectory = Path.Combine(webRootPath, saveDirectory);
                if (!Directory.Exists(fullSaveDirectory))
                {
                    Directory.CreateDirectory(fullSaveDirectory);
                }
                var fullFilePath = Path.Combine(fullSaveDirectory, fullFileName);
                await File.WriteAllBytesAsync(fullFilePath, finalBytes);

                var relativePath = $"/{saveDirectory}/{fullFileName}".Replace("\\", "/");
                _Logger?.ApiInfo($"图片保存成功: {relativePath}，文件大小: {finalBytes.Length} bytes");
                return relativePath;
            }
            catch (Exception ex)
            {
                _Logger?.ApiError($"下载并保存图片失败: {imageUrl}, 错误: {ex.Message}");
                throw new Exception($"下载并保存图片失败: {ex.Message}", ex);
            }
        }
    }
}
