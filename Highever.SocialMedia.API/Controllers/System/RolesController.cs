using Highever.SocialMedia.Application.Contracts;
using Highever.SocialMedia.Application.Contracts.DTOs.System;
using Highever.SocialMedia.Application.Contracts.Services;
using Highever.SocialMedia.Common;
using Microsoft.AspNetCore.Mvc;

namespace Highever.SocialMedia.API.Controllers.System
{
    /// <summary>
    /// 角色管理
    /// </summary>
    [ApiController]
    [Route("api/roles")]
    [ApiExplorerSettings(GroupName = nameof(SwaggerApiGroup.系统功能))]
    public class RolesController : Controller
    {
        private readonly IRolesService _rolesService;

        public RolesController(IRolesService rolesService)
        {
            _rolesService = rolesService;
        }

        /// <summary>
        /// 获取所有角色
        /// </summary>
        /// <param name="request">查询请求</param>
        /// <returns>角色列表</returns>
        [HttpGet("getAllRoles")]
        [ProducesResponseType(typeof(AjaxResult<List<RoleResponse>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllRoles([FromQuery] GetRolesRequest request)
        {
            var data = await _rolesService.GetRolesAsync(request);
            return this.Success(data);
        }

        /// <summary>
        /// 创建角色
        /// </summary>
        /// <param name="request">创建请求</param>
        /// <returns>创建结果</returns>
        [HttpPost("create")]
        [ProducesResponseType(typeof(AjaxResult<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> CreateRole([FromBody] CreateRoleRequest request)
        {
            try
            {
                var roleId = await _rolesService.CreateRoleAsync(request);
                return this.Success(new { Id = roleId }, "角色创建成功");
            }
            catch (InvalidOperationException ex)
            {
                return this.Fail(ex.Message);
            }
            catch (Exception ex)
            {
                return this.Fail($"创建角色失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 更新角色
        /// </summary>
        /// <param name="request">更新请求</param>
        /// <returns>更新结果</returns>
        [HttpPost("update")]
        [ProducesResponseType(typeof(AjaxResult<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateRole([FromBody] UpdateRoleRequest request)
        {
            try
            {
                var result = await _rolesService.UpdateRoleAsync(request);
                if (result)
                {
                    return this.Success("角色更新成功");
                }
                return this.Fail("角色更新失败");
            }
            catch (InvalidOperationException ex)
            {
                return this.Fail(ex.Message);
            }
            catch (Exception ex)
            {
                return this.Fail($"更新角色失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 删除角色
        /// </summary>
        /// <param name="request">删除请求</param>
        /// <returns>删除结果</returns>
        [HttpPost("delete")]
        [ProducesResponseType(typeof(AjaxResult<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> DeleteRole([FromBody] DeleteRoleRequest request)
        {
            try
            {
                var result = await _rolesService.DeleteRoleAsync(request);
                if (result)
                {
                    return this.Success("角色删除成功");
                }
                return this.Fail("角色删除失败");
            }
            catch (InvalidOperationException ex)
            {
                return this.Fail(ex.Message);
            }
            catch (Exception ex)
            {
                return this.Fail($"删除角色失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 批量删除角色
        /// </summary>
        /// <param name="request">批量删除请求</param>
        /// <returns>删除结果</returns>
        [HttpPost("batchDelete")]
        [ProducesResponseType(typeof(AjaxResult<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> BatchDeleteRole([FromBody] BatchDeleteRoleRequest request)
        {
            try
            {
                var result = await _rolesService.BatchDeleteRoleAsync(request);
                if (result)
                {
                    return this.Success("角色批量删除成功");
                }
                return this.Fail("角色批量删除失败");
            }
            catch (InvalidOperationException ex)
            {
                return this.Fail(ex.Message);
            }
            catch (Exception ex)
            {
                return this.Fail($"批量删除角色失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 获取角色详情
        /// </summary>
        /// <param name="id">角色ID</param>
        /// <returns>角色详情</returns>
        [HttpGet("getById/{id}")]
        [ProducesResponseType(typeof(AjaxResult<RoleResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetRoleById(int id)
        {
            try
            {
                var role = await _rolesService.GetRoleByIdAsync(id);
                if (role == null)
                {
                    return this.Fail("角色不存在");
                }
                return this.Success(role);
            }
            catch (Exception ex)
            {
                return this.Fail($"获取角色详情失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 检查角色代码是否存在
        /// </summary>
        /// <param name="code">角色代码</param>
        /// <param name="excludeId">排除的角色ID</param>
        /// <returns>检查结果</returns>
        [HttpGet("checkCode")]
        [ProducesResponseType(typeof(AjaxResult<object>), StatusCodes.Status200OK)]
        [HiddenAPI]
        public async Task<IActionResult> CheckRoleCode([FromQuery] string code, [FromQuery] int? excludeId = null)
        {
            try
            {
                var exists = await _rolesService.IsRoleCodeExistAsync(code, excludeId);
                return this.Success(new { Exists = exists });
            }
            catch (Exception ex)
            {
                return this.Fail($"检查角色代码失败：{ex.Message}");
            }
        }
    }
}
