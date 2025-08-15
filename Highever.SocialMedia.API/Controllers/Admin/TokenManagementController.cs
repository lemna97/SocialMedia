using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Highever.SocialMedia.Application.Contracts;
using Highever.SocialMedia.Common;

namespace Highever.SocialMedia.API.Controllers.Admin
{
    /// <summary>
    /// Token管理
    /// </summary>
    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = "1")] 
    public class TokenManagementController : ControllerBase
    {
        private readonly ITokenService _tokenService;
        private readonly INLogger _logger;

        public TokenManagementController(ITokenService tokenService, INLogger logger)
        {
            _tokenService = tokenService;
            _logger = logger;
        }

        /// <summary>
        /// 手动清理过期令牌
        /// </summary>
        /// <returns>清理结果</returns>
        [HttpPost("cleanupExpired")]
        [ProducesResponseType(typeof(AjaxResult<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> CleanupExpiredTokens()
        {
            try
            {
                var cleanedCount = await _tokenService.CleanupExpiredTokensAsync();
                
                return Ok(new AjaxResult<object>
                {
                    code = HttpCode.成功,
                    msg = $"清理完成，共清理 {cleanedCount} 个过期令牌",
                    data = new { CleanedCount = cleanedCount }
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "手动清理过期令牌失败");
                return StatusCode(500, new AjaxResult<string>
                {
                    code = HttpCode.失败,
                    msg = "清理过期令牌失败"
                });
            }
        }

        /// <summary>
        /// 撤销指定用户的令牌
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>撤销结果</returns>
        [HttpPost("revokeUser")]
        [ProducesResponseType(typeof(AjaxResult<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> RevokeUserToken([FromQuery] int userId)
        {
            try
            {
                var result = await _tokenService.RevokeRefreshTokenAsync(userId);

                return this.JsonOk();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"撤销用户 {userId} 的令牌失败");
                return this.Fail("撤销用户令牌失败");
            }
        }

        /// <summary>
        /// 获取用户令牌信息
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>令牌信息</returns>
        [HttpGet("userToken")]
        [ProducesResponseType(typeof(AjaxResult<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUserTokenInfo([FromQuery]int userId)
        {
            try
            {
                var tokenInfo = await _tokenService.GetRefreshTokenInfoAsync(userId);
                
                return Ok(new AjaxResult<object>
                {
                    code = HttpCode.成功,
                    msg = "获取成功",
                    data = tokenInfo == null ? null : new
                    {
                        UserId = tokenInfo.UserId,
                        HasRefreshToken = !string.IsNullOrEmpty(tokenInfo.RefreshToken),
                        ExpireTime = tokenInfo.ExpireTime,
                        LastActivityTime = tokenInfo.LastActivityTime,
                        IsExpired = tokenInfo.IsExpired
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"获取用户 {userId} 的令牌信息失败");
                return this.Fail("获取用户令牌信息失败"); 
            }
        }
    }
}