using Highever.SocialMedia.Application.Contracts.Services;
using Highever.SocialMedia.Common;
using Highever.SocialMedia.Domain.Entity;
using Microsoft.AspNetCore.Mvc;

namespace Highever.SocialMedia.API.Controllers
{
    /// <summary>
    /// 菜单管理
    /// </summary>
    [ApiController]
    [Route("api/menus")]
    [ApiExplorerSettings(GroupName = nameof(SwaggerApiGroup.系统功能))] 
    public class MenusController : Controller
    {
        private readonly IMenusService _menusService;

        public MenusController(IMenusService menusService)
        {
            _menusService = menusService;
        }

        /// <summary>
        /// 获取所有菜单
        /// </summary>
        /// <returns>菜单列表</returns>
        [HttpGet("getAllMenus")]
        [ProducesResponseType(typeof(AjaxResult<List<Menus>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllMenus()
        {
            var data = await _menusService.GetAllAsync();
            return this.Success(data);
        }

        /// <summary>
        /// 获取菜单树
        /// </summary>
        /// <returns>菜单树</returns>
        [HttpGet("tree")]
        [ProducesResponseType(typeof(AjaxResult<List<Menus>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMenuTree()
        {
            var menus = await _menusService.GetMenuTreeAsync();
            return this.Success(menus);
        }
    }
}