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
    [AllowAnonymous] 
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
        [HttpGet("roleMenus")]
        [ProducesResponseType(typeof(AjaxResult<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMenusByRoleId([FromQuery] GetMenusByRoleRequest request)
        {
            var menus = await _menuPermsService.GetMenusByRoleIdAsync(request.RoleId);
            return this.Success(menus);
        }

        /// <summary>
        /// 为角色分配菜单
        /// </summary>
        /// <param name="request">分配请求</param>
        /// <returns>分配结果</returns>
        [HttpPost("roleAssign")]
        [ProducesResponseType(typeof(AjaxResult<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> AssignMenusToRole([FromBody] AssignMenusToRoleRequest request)
        {
            var result = await _menuPermsService.AssignMenusToRoleAsync(request.RoleId, request.MenuIds);
            return this.Ok();
        }

        /// <summary>
        /// 添加角色菜单关联
        /// </summary>
        /// <param name="request">关联信息</param>
        /// <returns>添加结果</returns>
        [HttpPost("addMenuPerm")]
        [ProducesResponseType(typeof(AjaxResult<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> AddMenuPerm([FromBody] AddMenuPermRequest request)
        {
            var menuPerm = new MenuPerms
            {
                RoleId = request.RoleId,
                MenuId = request.MenuId
            };
            var result = await _menuPermsService.CreateAsync(menuPerm);
            return this.Ok();
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
            var result = await _menuPermsService.DeleteAsync(mp => mp.RoleId == request.RoleId && mp.MenuId == request.MenuId);
            return this.Ok();
        }

        /// <summary>
        /// 获取所有角色菜单关联
        /// </summary>
        /// <returns>关联列表</returns>
        [HttpGet("getAllMenuPerms")]
        [ProducesResponseType(typeof(AjaxResult<List<MenuPerms>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllMenuPerms()
        {
            var menuPerms = await _menuPermsService.GetQueryListAsync(); 
            return this.Success(menuPerms, "获取Cookie成功");
        }
    }
}

