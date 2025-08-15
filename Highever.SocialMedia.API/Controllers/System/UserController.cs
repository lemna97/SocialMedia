using Highever.SocialMedia.Application.Contracts.DTOs.System;
using Highever.SocialMedia.Application.Contracts.Services;
using Highever.SocialMedia.Common;
using Highever.SocialMedia.Domain.Entity;
using Highever.SocialMedia.Domain.Repository;
using Microsoft.AspNetCore.Mvc;

namespace Highever.SocialMedia.API.Controllers.System
{
    /// <summary>
    /// 系统用户管理
    /// </summary>
    [ApiController]
    [Route("api/sys_user")]
    [ApiExplorerSettings(GroupName = nameof(SwaggerApiGroup.系统功能))]
    public class UserController : Controller
    {
        private readonly IUsersService _usersService;

        public UserController(IUsersService usersService)
        {
            _usersService = usersService;
        }

        /// <summary>
        /// 获取所有用户
        /// </summary>
        /// <param name="request">查询请求</param>
        /// <returns>用户列表</returns>
        [HttpGet("getAllUsers")]
        [ProducesResponseType(typeof(AjaxResult<List<UserResponse>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllUsers([FromQuery] GetUsersRequest request)
        {
            // 设置默认分页参数
            if (request.PageIndex == null || request.PageIndex <= 0)
            {
                request.PageIndex = 1;
            }
            if (request.PageSize == null || request.PageSize <= 0)
            {
                request.PageSize = 20;
            }

            var data = await _usersService.GetUsersAsync(request);
            return this.Success(data);
        }

        /// <summary>
        /// 创建用户
        /// </summary>
        /// <param name="request">创建请求</param>
        /// <returns>创建结果</returns>
        [HttpPost("create")]
        [ProducesResponseType(typeof(AjaxResult<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
        {
            try
            {
                var userId = await _usersService.CreateUserAsync(request);
                return this.Success(new { Id = userId }, "用户创建成功");
            }
            catch (InvalidOperationException ex)
            {
                return this.Fail(ex.Message);
            }
            catch (Exception ex)
            {
                return this.Fail($"创建用户失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 更新用户
        /// </summary>
        /// <param name="request">更新请求</param>
        /// <returns>更新结果</returns>
        [HttpPost("update")]
        [ProducesResponseType(typeof(AjaxResult<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateUser([FromBody] UpdateUserRequest request)
        {
            try
            {
                var result = await _usersService.UpdateUserAsync(request);
                if (result)
                {
                    return this.Success("用户更新成功");
                }
                return this.Fail("用户更新失败");
            }
            catch (InvalidOperationException ex)
            {
                return this.Fail(ex.Message);
            }
            catch (Exception ex)
            {
                return this.Fail($"更新用户失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 删除用户
        /// </summary>
        /// <param name="request">删除请求</param>
        /// <returns>删除结果</returns>
        [HttpPost("delete")]
        [ProducesResponseType(typeof(AjaxResult<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> DeleteUser([FromBody] DeleteUserRequest request)
        {
            try
            {
                var result = await _usersService.DeleteUserAsync(request);
                if (result)
                {
                    return this.Success("用户删除成功");
                }
                return this.Fail("用户删除失败");
            }
            catch (InvalidOperationException ex)
            {
                return this.Fail(ex.Message);
            }
            catch (Exception ex)
            {
                return this.Fail($"删除用户失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 批量删除用户
        /// </summary>
        /// <param name="request">批量删除请求</param>
        /// <returns>删除结果</returns>
        [HttpPost("batchDelete")]
        [ProducesResponseType(typeof(AjaxResult<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> BatchDeleteUser([FromBody] BatchDeleteUserRequest request)
        {
            try
            {
                var result = await _usersService.BatchDeleteUserAsync(request);
                if (result)
                {
                    return this.Success("用户批量删除成功");
                }
                return this.Fail("用户批量删除失败");
            }
            catch (InvalidOperationException ex)
            {
                return this.Fail(ex.Message);
            }
            catch (Exception ex)
            {
                return this.Fail($"批量删除用户失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 获取用户详情
        /// </summary>
        /// <param name="id">用户ID</param>
        /// <returns>用户详情</returns>
        [HttpGet("getUserById")]
        [ProducesResponseType(typeof(AjaxResult<UserResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUserById(int id)
        {
            try
            {
                var user = await _usersService.GetUserByIdAsync(id);
                if (user == null)
                {
                    return this.Fail("用户不存在");
                }
                return this.Success(user);
            }
            catch (Exception ex)
            {
                return this.Fail($"获取用户详情失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 检查用户名是否存在
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="excludeId">排除的用户ID</param>
        /// <returns>检查结果</returns>
        [HttpGet("checkUsername")]
        [ProducesResponseType(typeof(AjaxResult<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> CheckUsername([FromQuery] string username, [FromQuery] int? excludeId = null)
        {
            try
            {
                var exists = await _usersService.IsUsernameExistAsync(username, excludeId);
                return this.Success(new { Exists = exists });
            }
            catch (Exception ex)
            {
                return this.Fail($"检查用户名失败：{ex.Message}");
            }
        } 

        /// <summary>
        /// 修改密码
        /// </summary>
        /// <param name="request">修改密码请求</param>
        /// <returns>修改结果</returns>
        [HttpPost("changePassword")]
        [ProducesResponseType(typeof(AjaxResult<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            try
            {
                var result = await _usersService.ChangePasswordAsync(request);
                if (result)
                {
                    return this.Success("密码修改成功");
                }
                return this.Fail("密码修改失败");
            }
            catch (InvalidOperationException ex)
            {
                return this.Fail(ex.Message);
            }
            catch (Exception ex)
            {
                return this.Fail($"修改密码失败：{ex.Message}");
            }
        }
    }
}
