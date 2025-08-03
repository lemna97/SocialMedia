using Highever.SocialMedia.Application.Contracts.Services;
using Highever.SocialMedia.Common;
using Highever.SocialMedia.Domain.Entity;
using Microsoft.AspNetCore.Mvc;

namespace Highever.SocialMedia.API.Controllers
{
    /// <summary>
    /// 角色菜单关联管理
    /// </summary>
    [ApiController]
    [Route("api/menu-perms")]
    [ApiExplorerSettings(GroupName = nameof(SwaggerApiGroup.System))]
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
        /// <param name="roleId">角色ID</param>
        /// <returns>菜单列表</returns>
        [HttpGet("roleMenus")]
        public async Task<IActionResult> GetMenusByRoleId(long roleId)
        {
            var menus = await _menuPermsService.GetMenusByRoleIdAsync(roleId);
            return this.Success(menus);
        }

        /// <summary>
        /// 为角色分配菜单
        /// </summary>
        /// <param name="roleId">角色ID</param>
        /// <param name="menuIds">菜单ID列表</param>
        /// <returns>分配结果</returns>
        [HttpPost("roleAssign")]
        public async Task<IActionResult> AssignMenusToRole(long roleId, [FromBody] List<long> menuIds)
        {
            var result = await _menuPermsService.AssignMenusToRoleAsync(roleId, menuIds);
            return this.Ok();
        }

        /// <summary>
        /// 添加角色菜单关联
        /// </summary>
        /// <param name="menuPerm">关联信息</param>
        /// <returns>添加结果</returns>
        [HttpPost("addMenuPerm")]
        public async Task<IActionResult> AddMenuPerm([FromBody] MenuPerms menuPerm)
        {
            var result = await _menuPermsService.CreateAsync(menuPerm);
            return this.Ok();
        }

        /// <summary>
        /// 删除角色菜单关联
        /// </summary>
        /// <param name="roleId">角色ID</param>
        /// <param name="menuId">菜单ID</param>
        /// <returns>删除结果</returns>
        [HttpDelete("role/{roleId}/menu/{menuId}")]
        public async Task<IActionResult> DeleteMenuPerm(long roleId, long menuId)
        {
            var result = await _menuPermsService.DeleteAsync(mp => mp.RoleId == roleId && mp.MenuId == menuId);
            return this.Ok();
        }

        /// <summary>
        /// 获取所有角色菜单关联
        /// </summary>
        /// <returns>关联列表</returns>
        [HttpGet]
        public async Task<IActionResult> GetAllMenuPerms()
        {
            var menuPerms = await _menuPermsService.GetQueryListAsync(); 
            return this.Success(menuPerms, "获取Cookie成功");
        }
    }
}
