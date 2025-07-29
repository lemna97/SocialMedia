using Highever.SocialMedia.Admin;
using Highever.SocialMedia.Application.Contracts.Services;
using Highever.SocialMedia.Common;
using Highever.SocialMedia.Domain.Entity;
using Microsoft.AspNetCore.Mvc;

namespace Highever.SocialMedia.API.Controllers.System
{
    /// <summary>
    /// 角色菜单关联管理
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [ApiExplorerSettings(GroupName = nameof(SwaggerApiGroup.Login))]
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
        [HttpGet("role/{roleId}/menus")]
        public async Task<AjaxResult<List<Menus>>> GetMenusByRoleId(long roleId)
        {
            var menus = await _menuPermsService.GetMenusByRoleIdAsync(roleId);
            return new AjaxResult<List<Menus>> { Data = menus };
        }

        /// <summary>
        /// 为角色分配菜单
        /// </summary>
        /// <param name="roleId">角色ID</param>
        /// <param name="menuIds">菜单ID列表</param>
        /// <returns>分配结果</returns>
        [HttpPost("role/{roleId}/assign")]
        public async Task<AjaxResult<string>> AssignMenusToRole(long roleId, [FromBody] List<long> menuIds)
        {
            var result = await _menuPermsService.AssignMenusToRoleAsync(roleId, menuIds);
            return new AjaxResult<string> {};
        }

        /// <summary>
        /// 添加角色菜单关联
        /// </summary>
        /// <param name="menuPerm">关联信息</param>
        /// <returns>添加结果</returns>
        [HttpPost]
        public async Task<AjaxResult<string>> AddMenuPerm([FromBody] MenuPerms menuPerm)
        {
            var result = await _menuPermsService.CreateAsync(menuPerm);
            return new AjaxResult<string> {};
        }

        /// <summary>
        /// 删除角色菜单关联
        /// </summary>
        /// <param name="roleId">角色ID</param>
        /// <param name="menuId">菜单ID</param>
        /// <returns>删除结果</returns>
        [HttpDelete("role/{roleId}/menu/{menuId}")]
        public async Task<AjaxResult<string>> DeleteMenuPerm(long roleId, long menuId)
        {
            var result = await _menuPermsService.DeleteAsync(mp => mp.RoleId == roleId && mp.MenuId == menuId);
            return new AjaxResult<string> {};
        }

        /// <summary>
        /// 获取所有角色菜单关联
        /// </summary>
        /// <returns>关联列表</returns>
        [HttpGet]
        public async Task<AjaxResult<List<MenuPerms>>> GetAllMenuPerms()
        {
            var menuPerms = await _menuPermsService.GetQueryListAsync();
            return new AjaxResult<List<MenuPerms>> { Data = menuPerms };
        }
    }
}
