using Highever.SocialMedia.Application.Contracts.Services;
using Highever.SocialMedia.Application.Contracts.DTOs.System;
using Highever.SocialMedia.Common;
using Highever.SocialMedia.Domain.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Highever.SocialMedia.API.Controllers
{
    /// <summary>
    /// 角色菜单关联管理
    /// </summary>
    [ApiController]
    [Route("api/menu-perms")]
    [ApiExplorerSettings(GroupName = nameof(SwaggerApiGroup.系统功能))]
    public class MenuPermsController : ControllerBase
    {
        private readonly IMenuPermsService _menuPermsService;

        public MenuPermsController(IMenuPermsService menuPermsService)
        {
            _menuPermsService = menuPermsService;
        }

        /// <summary>
        /// 根据角色ID获取菜单列表
        /// </summary>
        /// <param name="request">查询请求</param>
        /// <returns>菜单列表</returns>
        [HttpGet("getMenusByRoleId")]
        [ProducesResponseType(typeof(AjaxResult<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMenusByRoleId([FromQuery] GetMenusByRoleRequest request)
        {
            var menus = await _menuPermsService.GetMenusByRoleIdAsync(request.RoleId);
            return this.Success(menus);
        }

        /// <summary>
        /// 为角色分配菜单
        /// </summary>
        /// <remarks>
        /// 注意：MenuIds 是当前角色所有的菜单Id
        /// </remarks>
        /// <param name="request">分配请求</param>
        /// <returns>分配结果</returns>
        [HttpPost("roleAssign")]
        [ProducesResponseType(typeof(AjaxResult<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> AssignMenusToRole([FromBody] AssignMenusToRoleRequest request)
        {
            var result = await _menuPermsService.AssignMenusToRoleAsync(request.RoleId, request.MenuIds);
            return this.JsonOk();
        }


        /// <summary>
        /// 删除角色菜单关联
        /// </summary>
        /// <param name="request">删除请求</param>
        /// <returns>删除结果</returns>
        [HttpPost("removeRoleMenu")]
        [ProducesResponseType(typeof(AjaxResult<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> DeleteMenuPerm([FromBody] DeleteMenuPermRequest request)
        {
            var result = await _menuPermsService.DeleteAsync(mp => mp.RoleId == request.RoleId && request.MenuIds.Contains(mp.MenuId));
            return this.JsonOk();
        }
    }
}

