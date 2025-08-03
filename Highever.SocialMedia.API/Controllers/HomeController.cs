using Highever.SocialMedia.Common;
using Highever.SocialMedia.Domain.Entity;
using Highever.SocialMedia.Domain.Repository;
using Highever.SocialMedia.Domain.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;

namespace Highever.SocialMedia.API.Controllers
{
    /// <summary>
    /// 系统设置
    /// </summary>
    [ApiController]
    [Route("api/user")] 
    [ApiGroup(SwaggerApiGroup.System)]
    public class HomeController : ControllerBase
    {
        INLogger _logger => _serviceProvider.GetRequiredService<INLogger>();
        private IRepository<Users> _repositoryUsers => _serviceProvider.GetRequiredService<IRepository<Users>>();
        private IRepository<UserRoles> _repositoryUserRoles => _serviceProvider.GetRequiredService<IRepository<UserRoles>>();
        private JwtHelper _jwtHelper => _serviceProvider.GetRequiredService<JwtHelper>();

        /// <summary>
        /// 
        /// </summary>
        public readonly IServiceProvider _serviceProvider;
        /// <summary>
        /// 是是是
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
                if (userRoles==null)
                {
                    return this.JsonError("暂无权限！");
                } 

                var token = _jwtHelper.GenerateToken(
                    userId: user.Id,
                    userName: user.Account,
                    roles: userRoles.Select(t => t.RoleId.ToString()).ToList(),
                    permissions: new List<string>() { }
                );

                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = false,
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTime.UtcNow.AddMinutes(60)
                };

                Response.Cookies.Append("auth_token", token, cookieOptions);

                user.CreatedAt = DateTime.Now;
                await _repositoryUsers.UpdateAsync(user);

                var loginResult = new
                {
                    Token = token,
                    UserId = user.Id,
                    UserName = user.Account,
                    LoginTime = DateTime.Now,
                    ExpiresIn = 3600
                };

                return this.Success(loginResult, "登录成功");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "用户登录过程中发生错误");
                return this.JsonError("登录过程中发生错误，请稍后重试");
            }
        }

        /// <summary>
        /// MD5加密方法
        /// </summary>
        /// <param name="input">要加密的字符串</param>
        /// <returns>MD5加密后的字符串</returns>
        private string ComputeMD5Hash(string input)
        {
            using (var md5 = MD5.Create())
            {
                var inputBytes = Encoding.UTF8.GetBytes(input);
                var hashBytes = md5.ComputeHash(inputBytes);
                return Convert.ToHexString(hashBytes).ToLower();
            }
        }
    }
}
