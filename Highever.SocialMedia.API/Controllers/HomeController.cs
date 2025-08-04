using Highever.SocialMedia.Common;
using Highever.SocialMedia.Domain.Entity;
using Highever.SocialMedia.Domain.Repository;
using Highever.SocialMedia.Domain.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Highever.SocialMedia.Application.Contracts;

namespace Highever.SocialMedia.API.Controllers
{
    /// <summary>
    /// 登录相关
    /// </summary>
    [ApiController]
    [Route("api/user")] 
    [ApiGroup(SwaggerApiGroup.系统功能)]
    public class HomeController : Controller
    {
        INLogger _logger => _serviceProvider.GetRequiredService<INLogger>();
        private IRepository<Users> _repositoryUsers => _serviceProvider.GetRequiredService<IRepository<Users>>();
        private IRepository<UserRoles> _repositoryUserRoles => _serviceProvider.GetRequiredService<IRepository<UserRoles>>();
        private JwtHelper _jwtHelper => _serviceProvider.GetRequiredService<JwtHelper>();
        private ITokenService _tokenService => _serviceProvider.GetRequiredService<ITokenService>();

        /// <summary>
        /// 
        /// </summary>
        public readonly IServiceProvider _serviceProvider;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="serviceProvider"></param>
        public HomeController(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpPost("index")]
        [HiddenAPI]
        public IActionResult Index()
        {
            return Ok();
        }

        /// <summary>
        /// 用户登录
        /// </summary>
        /// <param name="request">登录请求</param>
        /// <returns>登录结果，包含JWT Token</returns>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromForm] LoginRequest request)
        {
            try
            {
                var loginTimestamp = request.Timestamp ?? DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                 
                _logger.Info($"用户 {request.Username} 尝试登录，时间戳: {loginTimestamp}");
                 
                var user = await _repositoryUsers.FirstOrDefaultAsync(u => u.Account == request.Username);
                if (user == null)
                {
                    return this.JsonError("用户名或密码错误");
                }
                if (!user.IsActive)
                {
                    return this.JsonError("用户已被禁用！");
                } 

                var hashedPassword = ComputeMD5Hash(request.Password);
                if (user.PasswordHash != hashedPassword)
                {
                    return this.JsonError("用户名或密码错误");
                }
                var userRoles = await _repositoryUserRoles.QueryListAsync(u => u.UserId == user.Id);
                var roles = userRoles?.Select(t => t.RoleId.ToString()).ToList() ?? new List<string>();

                // 生成Token对
                var tokenResult = _jwtHelper.GenerateTokenPair(
                    userId: user.Id,
                    userName: user.Account,
                    roles: roles,
                    permissions: new List<string>()
                );

                // 保存RefreshToken到数据库
                await _tokenService.UpdateRefreshTokenAsync(
                    user.Id, 
                    tokenResult.RefreshToken, 
                    DateTime.UtcNow.AddDays(7)
                );

                // 设置Cookie
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = false,
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTime.UtcNow.AddMinutes(60)
                };
                Response.Cookies.Append("auth_token", tokenResult.AccessToken, cookieOptions);

                return this.Success(new
                {
                    Token = tokenResult.AccessToken,
                    RefreshToken = tokenResult.RefreshToken,
                    UserId = user.Id,
                    UserName = user.Account,
                    ExpiresIn = tokenResult.ExpiresIn
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "用户登录失败");
                return this.JsonError("登录失败");
            }
        }

        /// <summary>
        /// MD5加密方法
        /// </summary>
        /// <param name="input">要加密的字符串</param>
        /// <returns>MD5加密后的字符串</returns>
        [HiddenAPI]
        private string ComputeMD5Hash(string input)
        {
            using (var md5 = MD5.Create())
            {
                var inputBytes = Encoding.UTF8.GetBytes(input);
                var hashBytes = md5.ComputeHash(inputBytes);
                return Convert.ToHexString(hashBytes).ToLower();
            }
        }

        /// <summary>
        /// 用户登出
        /// </summary>
        /// <returns></returns>
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim != null && long.TryParse(userIdClaim.Value, out var userId))
                {
                    // 撤销RefreshToken
                    await _tokenService.RevokeRefreshTokenAsync(userId);
                }

                Response.Cookies.Delete("auth_token");
                
                _logger.Info($"用户 {userIdClaim?.Value} 已登出");
                return this.Success("登出成功！");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "用户登出过程中发生错误");
                return this.JsonError("登出过程中发生错误");
            }
        }

        /// <summary>
        /// 刷新Token
        /// </summary>
        /// <param name="refreshToken">刷新令牌</param>
        /// <returns></returns>
        [HttpPost("refresh-token")]
        [AllowAnonymous]
        public async Task<IActionResult> RefreshToken([FromForm] string refreshToken)
        {
            try
            {
                if (string.IsNullOrEmpty(refreshToken))
                {
                    return this.JsonError("刷新令牌不能为空");
                }

                // 使用TokenService处理刷新逻辑
                var tokenResult = await _tokenService.RefreshTokenAsync(refreshToken);
                
                // 更新Cookie
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = false,
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTime.UtcNow.AddMinutes(60)
                };
                Response.Cookies.Append("auth_token", tokenResult.AccessToken, cookieOptions);

                return this.Success(new
                {
                    Token = tokenResult.AccessToken,
                    RefreshToken = tokenResult.RefreshToken,
                    UserId = tokenResult.UserId,
                    UserName = tokenResult.UserName,
                    ExpiresIn = tokenResult.ExpiresIn
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return this.JsonError(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "刷新Token失败");
                return this.JsonError("刷新Token失败");
            }
        }
    }
}
