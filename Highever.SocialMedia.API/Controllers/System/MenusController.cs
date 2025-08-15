using Highever.SocialMedia.Application.Contracts.Services;
using Highever.SocialMedia.Application.Contracts.DTOs.System;
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
        [Obsolete]
        public async Task<IActionResult> GetAllMenus()
        {
            var data = await _menusService.GetAllAsync();
            return this.Success(data);
        }

        /// <summary>
        /// 获取所有菜单
        /// </summary>
        /// <param name="request">查询请求</param>
        /// <returns>菜单列表</returns>
        [HttpGet("getMenusWithRoleAsync")]
        [ProducesResponseType(typeof(AjaxResult<List<MenuResponse>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMenusWithRoleAsync([FromQuery] GetMenusRequest request)
        {
            var data = await _menusService.GetMenusWithRoleAsync(request?.RoleId);
            return this.Success(data);
        }

        /// <summary>
        /// 创建菜单
        /// </summary>
        /// <param name="request">创建请求</param>
        /// <returns>创建结果</returns>
        [HttpPost("create")]
        [ProducesResponseType(typeof(AjaxResult<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> CreateMenu([FromBody] CreateMenuRequest request)
        {
            try
            {
                var menuId = await _menusService.CreateMenuAsync(request);
                return this.Success(new { Id = menuId }, "菜单创建成功");
            }
            catch (InvalidOperationException ex)
            {
                return this.Fail(ex.Message);
            }
            catch (Exception ex)
            {
                return this.Fail($"创建菜单失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 更新菜单
        /// </summary>
        /// <param name="request">更新请求</param>
        /// <returns>更新结果</returns>
        [HttpPost("update")]
        [ProducesResponseType(typeof(AjaxResult<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateMenu([FromBody] UpdateMenuRequest request)
        {
            try
            {
                var result = await _menusService.UpdateMenuAsync(request);
                if (result)
                {
                    return this.Success("菜单更新成功");
                }
                return this.Fail("菜单更新失败");
            }
            catch (InvalidOperationException ex)
            {
                return this.Fail(ex.Message);
            }
            catch (Exception ex)
            {
                return this.Fail($"更新菜单失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 删除菜单
        /// </summary>
        /// <param name="request">删除请求</param>
        /// <returns>删除结果</returns>
        [HttpPost("delete")]
        [ProducesResponseType(typeof(AjaxResult<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> DeleteMenu([FromBody] DeleteMenuRequest request)
        {
            try
            {
                var result = await _menusService.DeleteMenuAsync(request);
                if (result)
                {
                    return this.Success("菜单删除成功");
                }
                return this.Fail("菜单删除失败");
            }
            catch (InvalidOperationException ex)
            {
                return this.Fail(ex.Message);
            }
            catch (Exception ex)
            {
                return this.Fail($"删除菜单失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 批量删除菜单
        /// </summary>
        /// <param name="request">批量删除请求</param>
        /// <returns>删除结果</returns>
        [HttpPost("batchDelete")]
        [ProducesResponseType(typeof(AjaxResult<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> BatchDeleteMenu([FromBody] BatchDeleteMenuRequest request)
        {
            try
            {
                var result = await _menusService.BatchDeleteMenuAsync(request);
                if (result)
                {
                    return this.Success("菜单批量删除成功");
                }
                return this.Fail("菜单批量删除失败");
            }
            catch (InvalidOperationException ex)
            {
                return this.Fail(ex.Message);
            }
            catch (Exception ex)
            {
                return this.Fail($"批量删除菜单失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 获取菜单详情
        /// </summary>
        /// <param name="id">菜单ID</param>
        /// <returns>菜单详情</returns>
        [HttpGet("getMenuById")]
        [ProducesResponseType(typeof(AjaxResult<MenuResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMenuById(int id)
        {
            try
            {
                var menu = await _menusService.GetMenuByIdAsync(id);
                if (menu == null)
                {
                    return this.Fail("菜单不存在");
                }
                return this.Success(menu);
            }
            catch (Exception ex)
            {
                return this.Fail($"获取菜单详情失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 检查菜单代码是否存在
        /// </summary>
        /// <param name="code">菜单代码</param>
        /// <param name="excludeId">排除的菜单ID</param>
        /// <returns>检查结果</returns>
        [HttpGet("checkCode")]
        [ProducesResponseType(typeof(AjaxResult<object>), StatusCodes.Status200OK)]
        [HiddenAPI]
        public async Task<IActionResult> CheckMenuCode([FromQuery] string code, [FromQuery] int? excludeId = null)
        {
            try
            {
                var exists = await _menusService.IsMenuCodeExistAsync(code, excludeId);
                return this.Success(new { Exists = exists });
            }
            catch (Exception ex)
            {
                return this.Fail($"检查菜单代码失败：{ex.Message}");
            }
        }
    }
}
