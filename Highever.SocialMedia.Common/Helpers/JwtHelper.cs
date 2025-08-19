using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Highever.SocialMedia.Common.Model;

namespace Highever.SocialMedia.Common
{
    /// <summary>
    /// JWT帮助类
    /// </summary>
    public class JwtHelper
    {
        private readonly JwtSettings _jwtSettings;

        public JwtHelper(JwtSettings jwtSettings)
        {
            _jwtSettings = jwtSettings;
        }

        /// <summary>
        /// 生成JWT令牌
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="userName">用户名</param>
        /// <param name="roles">角色列表</param>
        /// <param name="permissions">权限列表</param>
        /// <returns>JWT令牌</returns>
        public string GenerateToken(int userId, string userName, List<string> roles = null, List<string> permissions = null)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Name, userName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            };

            // 添加角色声明
            if (roles?.Any() == true)
            {
                foreach (var role in roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }
            }

            // 添加权限声明
            if (permissions?.Any() == true)
            {
                foreach (var permission in permissions)
                {
                    claims.Add(new Claim("permission", permission));
                }
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// 生成刷新令牌
        /// </summary>
        public string GenerateRefreshToken()
        {
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }

        /// <summary>
        /// 生成Token对（访问令牌 + 刷新令牌）
        /// </summary>
        public TokenResult GenerateTokenPair(int userId, string userName, List<string> roles = null, List<string> permissions = null)
        {
            var accessToken = GenerateToken(userId, userName, roles, permissions);
            var refreshToken = GenerateRefreshToken();

            return new TokenResult
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresIn = _jwtSettings.ExpiryMinutes * 60,
                UserId = userId,
                UserName = userName
            };
        }

        /// <summary>
        /// 验证刷新令牌并生成新的Token对
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="userName">用户名</param>
        /// <param name="roles">用户角色列表</param>
        /// <param name="permissions">用户权限列表</param>
        /// <returns>新的Token对</returns>
        public TokenResult RefreshToken(int userId, string userName, List<string> roles = null, List<string> permissions = null)
        {
            return GenerateTokenPair(userId, userName, roles, permissions);
        }

        /// <summary>
        /// 验证刷新令牌的有效性（仅验证格式和基本规则）
        /// </summary>
        /// <param name="refreshToken">刷新令牌</param>
        /// <returns>是否为有效格式</returns>
        public bool IsValidRefreshTokenFormat(string refreshToken)
        {
            if (string.IsNullOrEmpty(refreshToken))
                return false;

            try
            {
                // 检查是否为Base64格式
                var bytes = Convert.FromBase64String(refreshToken);
                return bytes.Length == 64; // 我们生成的RefreshToken是64字节
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 验证JWT令牌
        /// </summary>
        /// <param name="token">JWT令牌</param>
        /// <returns>验证结果</returns>
        public ClaimsPrincipal? ValidateToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = _jwtSettings.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                return principal;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 从令牌中获取用户ID
        /// </summary>
        /// <param name="token">JWT令牌</param>
        /// <returns>用户ID</returns>
        public int? GetUserIdFromToken(string token)
        {
            var principal = ValidateToken(token);
            var userIdClaim = principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : null;
        }

        /// <summary>
        /// 从令牌中获取用户名
        /// </summary>
        /// <param name="token">JWT令牌</param>
        /// <returns>用户名</returns>
        public string? GetUserNameFromToken(string token)
        {
            var principal = ValidateToken(token);
            return principal?.FindFirst(ClaimTypes.Name)?.Value;
        }

        /// <summary>
        /// 检查令牌是否过期
        /// </summary>
        /// <param name="token">JWT令牌</param>
        /// <returns>是否过期</returns>
        public bool IsTokenExpired(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jsonToken = tokenHandler.ReadJwtToken(token);
                return jsonToken.ValidTo < DateTime.UtcNow;
            }
            catch
            {
                return true;
            }
        }

        /// <summary>
        /// 生成包含权限的JWT令牌
        /// </summary>
        public async Task<string> GenerateTokenWithPermissionsAsync(int userId, string userName, List<string> roles = null, Dictionary<string, object> permissionClaims = null)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Name, userName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            };

            // 添加角色声明
            if (roles?.Any() == true)
            {
                foreach (var role in roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }
            }

            // 添加权限Claims
            if (permissionClaims?.Any() == true)
            {
                foreach (var (key, value) in permissionClaims)
                {
                    claims.Add(new Claim(key, value?.ToString() ?? string.Empty));
                }
            }

            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var credentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

            var temp_token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes),
                signingCredentials: credentials
            );
            var token = new JwtSecurityTokenHandler().WriteToken(temp_token);
            await Task.CompletedTask;
            return token;
        }

        /// <summary>
        /// 生成包含权限的Token对
        /// </summary>
        public async Task<TokenResult> GenerateTokenPairWithPermissionsAsync(int userId, string userName, List<string> roles = null, Dictionary<string, object> permissionClaims = null)
        {
            var accessToken = await GenerateTokenWithPermissionsAsync(userId, userName, roles, permissionClaims);
            var refreshToken = GenerateRefreshToken();

            return new TokenResult
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresIn = _jwtSettings.ExpiryMinutes * 60,
                UserId = userId,
                UserName = userName
            };
        }
    }
}







