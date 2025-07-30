using Highever.SocialMedia.Admin;
using Highever.SocialMedia.Application.Contracts.Services;
using Highever.SocialMedia.Common;
using Highever.SocialMedia.Domain;
using Highever.SocialMedia.Domain.Entity;
using Microsoft.AspNetCore.Mvc;
using NPOI.POIFS.Crypt.Dsig;

namespace Highever.SocialMedia.API.Controllers
{
    /// <summary>
    /// 菜单管理
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [ApiExplorerSettings(GroupName = nameof(SwaggerApiGroup.Login))]
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
        [HttpGet]
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
        public async Task<IActionResult> GetMenuTree()
        {
            var menus = await _menusService.GetMenuTreeAsync();
            return this.Success(menus);
        }
    }
}