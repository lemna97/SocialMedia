using Highever.SocialMedia.Application.Models;
using Highever.SocialMedia.Domain.Entity;
using Highever.SocialMedia.Domain.Repository;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Highever.SocialMedia.Application.Services.System
{
    /// <summary>
    /// Token权限编码器
    /// </summary>
    public class TokenPermissionEncoder
    {
        private readonly IRepository<Menus> _menusRepository;
        private readonly IRepository<MenuPerms> _menuPermsRepository;

        public TokenPermissionEncoder(
            IRepository<Menus> menusRepository,
            IRepository<MenuPerms> menuPermsRepository)
        {
            _menusRepository = menusRepository;
            _menuPermsRepository = menuPermsRepository;
        }

        /// <summary>
        /// 编码用户权限到Token Claims
        /// </summary>
        public async Task<Dictionary<string, object>> EncodeUserPermissionsAsync(long userId, List<string> roles)
        {
            try
            {
                // 1. 加载用户菜单权限
                var menuPermissions = await LoadUserMenuPermissionsAsync(userId, roles);

                // 2. 构建权限数据结构
                var permissionData = new CompressedPermissionData
                {
                    Version = "1.0",
                    GeneratedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                    MenuPermissions = menuPermissions
                };

                // 3. 计算权限哈希
                permissionData.Hash = CalculatePermissionHash(permissionData);

                // 4. 序列化权限数据
                var jsonData = JsonSerializer.Serialize(permissionData, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                // 5. 压缩权限数据
                var compressedData = CompressPermissionData(jsonData);

                // 6. 构建Claims
                var claims = new Dictionary<string, object>
                {
                    ["menu_perms"] = compressedData,
                    ["menu_hash"] = permissionData.Hash,
                    ["perm_version"] = permissionData.Version
                };

                return claims;
            }
            catch (Exception)
            {
                return new Dictionary<string, object>();
            }
        }

        /// <summary>
        /// 加载用户菜单权限
        /// </summary>
        private async Task<List<TokenMenuPermission>> LoadUserMenuPermissionsAsync(long userId, List<string> roles)
        {
            // 解析角色ID
            var roleIds = roles.Where(r => int.TryParse(r, out _))
                              .Select(int.Parse)
                              .ToList();

            if (!roleIds.Any())
            {
                return new List<TokenMenuPermission>();
            }

            // 查询用户有权限的菜单
            var menuPerms = await _menuPermsRepository.QueryListAsync(mp => roleIds.Contains(mp.RoleId));
            var menuIds = menuPerms.Select(mp => mp.MenuId).Distinct().ToList();
            
            if (!menuIds.Any())
            {
                return new List<TokenMenuPermission>();
            }

            var menus = await _menusRepository.QueryListAsync(m => menuIds.Contains(m.Id) && m.IsActive);

            // 转换为Token权限格式
            var tokenPermissions = menus.Select(menu => new TokenMenuPermission
            {
                MenuId = menu.Id,
                MenuCode = menu.Code ?? "",
                MenuUrl = NormalizeMenuUrl(menu.Url),
                AllowedMethods = ParseAllowedMethods(menu.Description),
                IsWildcard = IsWildcardMenu(menu.Url)
            })
            .Where(p => !string.IsNullOrEmpty(p.MenuUrl))
            .OrderBy(p => p.MenuUrl)
            .ToList();

            return tokenPermissions;
        }

        /// <summary>
        /// 压缩权限数据
        /// </summary>
        private string CompressPermissionData(string jsonData)
        {
            try
            {
                var bytes = Encoding.UTF8.GetBytes(jsonData);
                
                using var inputStream = new MemoryStream(bytes);
                using var outputStream = new MemoryStream();
                using (var gzipStream = new GZipStream(outputStream, CompressionLevel.Optimal))
                {
                    inputStream.CopyTo(gzipStream);
                }

                var compressedBytes = outputStream.ToArray();
                return Convert.ToBase64String(compressedBytes);
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// 解压权限数据
        /// </summary>
        public string DecompressPermissionData(string compressedData)
        {
            try
            {
                var compressedBytes = Convert.FromBase64String(compressedData);
                
                using var inputStream = new MemoryStream(compressedBytes);
                using var outputStream = new MemoryStream();
                using (var gzipStream = new GZipStream(inputStream, CompressionMode.Decompress))
                {
                    gzipStream.CopyTo(outputStream);
                }

                var decompressedBytes = outputStream.ToArray();
                return Encoding.UTF8.GetString(decompressedBytes);
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// 计算权限哈希
        /// </summary>
        private string CalculatePermissionHash(CompressedPermissionData permissionData)
        {
            var hashSource = string.Join("|", permissionData.MenuPermissions
                .OrderBy(p => p.MenuId)
                .Select(p => $"{p.MenuId}:{p.MenuCode}:{p.MenuUrl}:{string.Join(",", p.AllowedMethods)}"));

            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(hashSource));
            return Convert.ToBase64String(hashBytes)[..16];
        }

        /// <summary>
        /// 标准化菜单URL
        /// </summary>
        private string NormalizeMenuUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
                return string.Empty;

            var cleanUrl = url.Split('?', '#')[0];
            
            if (!cleanUrl.StartsWith("/"))
                cleanUrl = "/" + cleanUrl;

            if (cleanUrl.Length > 1 && cleanUrl.EndsWith("/"))
                cleanUrl = cleanUrl[..^1];

            return cleanUrl.ToLowerInvariant();
        }

        /// <summary>
        /// 解析允许的HTTP方法
        /// </summary>
        private List<string> ParseAllowedMethods(string description)
        {
            if (string.IsNullOrEmpty(description))
                return new List<string> { "GET" };

            var methods = new List<string>();
            var upperDesc = description.ToUpperInvariant();

            if (upperDesc.Contains("GET")) methods.Add("GET");
            if (upperDesc.Contains("POST")) methods.Add("POST");
            if (upperDesc.Contains("PUT")) methods.Add("PUT");
            if (upperDesc.Contains("DELETE")) methods.Add("DELETE");
            if (upperDesc.Contains("PATCH")) methods.Add("PATCH");

            return methods.Any() ? methods : new List<string> { "GET" };
        }

        /// <summary>
        /// 判断是否为通配符菜单
        /// </summary>
        private bool IsWildcardMenu(string url)
        {
            return !string.IsNullOrEmpty(url) && 
                   (url.Contains("*") || url.Contains("{") || url.Contains(":"));
        }
    }
}
