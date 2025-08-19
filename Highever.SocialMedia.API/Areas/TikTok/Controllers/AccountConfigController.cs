using Highever.SocialMedia.API.Areas.TikTok.DTO;
using Highever.SocialMedia.Application.Contracts;
using Highever.SocialMedia.Application.Contracts.Context;
using Highever.SocialMedia.Common;
using Highever.SocialMedia.Domain.Entity;
using Highever.SocialMedia.Domain.Repository;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;

namespace Highever.SocialMedia.API.Areas.TikTok.Controllers
{
    /// <summary>
    /// 账户配置
    /// </summary>
    [Area("Account")]
    [Route("api/accountConfig")]
    [ApiController]
    [ApiExplorerSettings(GroupName = nameof(SwaggerApiGroup.系统功能))]
    public class AccountConfigController : ControllerBase
    {

        private readonly IRepository<AccountConfig> _repositoryAccountConfig;
        private readonly IDataPermissionContextService _dataPermissionContextService;
        private readonly INLogger _logger;
        private readonly ITKAPIService _tKAPIService;

        public AccountConfigController(
            IRepository<AccountConfig> repositoryAccountConfig,
            IDataPermissionContextService dataPermissionContextService,
            INLogger logger,
            ITKAPIService tKAPIService,
            IServiceProvider serviceProvider)
        {
            _repositoryAccountConfig = repositoryAccountConfig;
            _dataPermissionContextService = dataPermissionContextService;
            _logger = logger;
            _tKAPIService = tKAPIService;
        }
        /// <summary>
        /// 分页查询账户配置列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("getList")]
        [ProducesResponseType(typeof(PageResult<AccountConfigResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAccountConfigList([FromBody] AccountConfigQueryRequest request)
        {
            try
            {
                // 获取当前用户的数据权限上下文
                var permissionContext = _dataPermissionContextService.GetCurrentContext();
                if (permissionContext == null)
                {
                    return this.Fail("无法获取用户权限信息");
                }

                // 构建查询条件
                Expression<Func<AccountConfig, bool>>? predicate = null;

                // 数据权限过滤
                if (!permissionContext.IsAdmin)
                {
                    // 非管理员只能查看自己的配置
                    predicate = x => x.SystemUid == permissionContext.UserId;
                }

                // 关键词搜索
                if (!string.IsNullOrWhiteSpace(request.Keyword))
                {
                    var keyword = request.Keyword.Trim();
                    if (predicate == null)
                        predicate = x => x.UniqueId.Contains(keyword);
                    else
                        predicate = x => (permissionContext.IsAdmin || x.SystemUid == permissionContext.UserId) &&
                                       x.UniqueId.Contains(keyword);
                }

                // 状态过滤
                if (request.IsActive.HasValue)
                {
                    if (predicate == null)
                        predicate = x => x.IsActive == request.IsActive.Value;
                    else
                        predicate = x => (permissionContext.IsAdmin || x.SystemUid == permissionContext.UserId) &&
                                       (string.IsNullOrEmpty(request.Keyword) || x.UniqueId.Contains(request.Keyword)) &&
                                       x.IsActive == request.IsActive.Value;
                }

                // 构建排序表达式
                Expression<Func<AccountConfig, object>>? orderBy = null;
                if (!string.IsNullOrWhiteSpace(request.OrderBy))
                {
                    orderBy = request.OrderBy.ToLower() switch
                    {
                        "id" => x => x.Id,
                        "uniqueid" => x => x.UniqueId,
                        "isactive" => x => x.IsActive,
                        "createdat" => x => x.CreatedAt,
                        "updatedat" => x => x.UpdatedAt,
                        _ => x => x.CreatedAt
                    };
                }

                // 执行分页查询
                var result = await _repositoryAccountConfig.GetPagedListAsync(
                    predicate: predicate,
                    pageIndex: request.PageIndex,
                    pageSize: request.PageSize,
                    orderBy: orderBy,
                    ascending: request.Ascending
                );

                // 转换为响应DTO
                var responseItems = result.Items.Select(x => new AccountConfigResponse
                {
                    Id = x.Id,
                    UniqueId = x.UniqueId,
                    SecUid = x.SecUid,
                    SystemUid = x.SystemUid,
                    IsActive = x.IsActive,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt
                }).ToList();

                var pageResult = new PageResult<AccountConfigResponse>
                {
                    Items = responseItems,
                    totalCount = result.TotalCount,
                    PageIndex = result.PageIndex,
                    PageSize = result.PageSize
                };

                return this.PagedResult(pageResult, "查询成功");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "查询账户配置列表失败");
                return this.Fail("查询账户配置列表失败");
            }
        }
        /// <summary>
        /// 获取当前用户的账户配置列表
        /// </summary>
        /// <returns></returns>
        [HttpGet("list")]
        [ProducesResponseType(typeof(List<AccountConfigResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAccountConfigs()
        {
            try
            {
                // 获取当前用户的数据权限上下文
                var permissionContext = _dataPermissionContextService.GetCurrentContext();
                if (permissionContext == null)
                {
                    return this.Fail("无法获取用户权限信息");
                }

                Expression<Func<AccountConfig, bool>>? predicate = null;

                // 数据权限过滤
                if (!permissionContext.IsAdmin)
                {
                    predicate = x => x.SystemUid == permissionContext.UserId && x.IsActive;
                }
                else
                {
                    predicate = x => x.IsActive;
                }

                var configs = await _repositoryAccountConfig.GetListAsync(predicate);

                var response = configs.Select(x => new AccountConfigResponse
                {
                    Id = x.Id,
                    UniqueId = x.UniqueId,
                    SecUid = x.SecUid,
                    SystemUid = x.SystemUid,
                    IsActive = x.IsActive,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt
                }).ToList();

                return this.Success(response, "获取成功");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "获取账户配置失败");
                return this.Fail("获取账户配置失败");
            }
        }
        /// <summary>
        /// 创建账户配置
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("create")]
        [ProducesResponseType(typeof(AjaxResult<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> CreateAccountConfig([FromBody] CreateAccountConfigRequest request)
        {
            try
            {
                // 获取当前用户的数据权限上下文
                var permissionContext = _dataPermissionContextService.GetCurrentContext();
                if (permissionContext == null)
                {
                    return this.Fail("无法获取用户权限信息");
                }

                // 权限验证：非管理员只能为自己创建配置
                if (!permissionContext.IsAdmin)
                {
                    return this.Fail("无权限为其他用户创建配置，请使用管理员进行操作！");
                }

                // 检查 UniqueId 是否已存在
                var existingConfig = await _repositoryAccountConfig.FirstOrDefaultAsync(x => x.UniqueId == request.UniqueId);
                if (existingConfig != null)
                {
                    return this.Fail($"UniqueId '{request.UniqueId}' 已存在");
                }

                var accountConfig = new AccountConfig
                {
                    UniqueId = request.UniqueId,
                    SecUid = request.SecUid,
                    SystemUid = request.SystemUid,
                    IsActive = request.IsActive,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                var result = await _repositoryAccountConfig.InsertAsync(accountConfig);

                if (result > 0)
                {
                    #region API同步
                    var (apiResponse, errorMessage) = await _tKAPIService.FetchUserProfileWithRetryAsync(request.UniqueId);

                    if (apiResponse != null)
                    {
                        await _tKAPIService.UpdateTiktokUsersAsync(apiResponse);
                    }
                    #endregion
                    _logger.Info($"创建账户配置成功: UniqueId={request.UniqueId}, SystemUid={request.SystemUid}");
                    return this.Success("创建成功");
                }
                else
                {
                    return this.Fail("创建失败");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"创建账户配置失败: UniqueId={request.UniqueId}");
                return this.Fail("创建账户配置失败");
            }
        }
        /// <summary>
        /// 更新账户配置
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("update")]
        [ProducesResponseType(typeof(AjaxResult<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateAccountConfig([FromBody] UpdateAccountConfigRequest request)
        {
            try
            {
                // 获取当前用户的数据权限上下文
                var permissionContext = _dataPermissionContextService.GetCurrentContext();
                if (permissionContext == null)
                {
                    return this.Fail("无法获取用户权限信息");
                }

                // 获取现有配置
                var existingConfig = await _repositoryAccountConfig.GetByIdAsync(request.Id);
                if (existingConfig == null)
                {
                    return this.Fail("配置不存在");
                }

                // 权限验证：非管理员只能修改自己的配置
                if (!permissionContext.IsAdmin && existingConfig.SystemUid != permissionContext.UserId)
                {
                    return this.Fail("无权限修改此配置");
                }

                // 权限验证：非管理员不能修改 SystemUid
                if (!permissionContext.IsAdmin && request.SystemUid != existingConfig.SystemUid)
                {
                    return this.Fail("无权限修改SystemUid");
                }

                // 检查 UniqueId 是否与其他记录冲突
                var duplicateConfig = await _repositoryAccountConfig.FirstOrDefaultAsync(
                    x => x.UniqueId == request.UniqueId && x.Id != request.Id);
                if (duplicateConfig != null)
                {
                    return this.Fail($"UniqueId '{request.UniqueId}' 已被其他配置使用");
                }

                // 更新配置
                existingConfig.UniqueId = request.UniqueId;
                existingConfig.SecUid = request.SecUid;
                existingConfig.SystemUid = request.SystemUid;
                existingConfig.IsActive = request.IsActive;
                existingConfig.UpdatedAt = DateTime.Now;

                var result = await _repositoryAccountConfig.UpdateAsync(existingConfig);

                if (result == 1)
                {
                    _logger.Info($"更新账户配置成功: Id={request.Id}, UniqueId={request.UniqueId}");
                    return this.Success("更新成功");
                }
                else
                {
                    return this.Fail("更新失败");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"更新账户配置失败: Id={request.Id}");
                return this.Fail("更新账户配置失败");
            }
        }
        /// <summary>
        /// 删除账户配置
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("delete")]
        [ProducesResponseType(typeof(AjaxResult<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> DeleteAccountConfig([FromBody] DeleteAccountConfigRequest request)
        {
            try
            {
                // 获取当前用户的数据权限上下文
                var permissionContext = _dataPermissionContextService.GetCurrentContext();
                if (permissionContext == null)
                {
                    return this.Fail("无法获取用户权限信息");
                }

                // 获取现有配置
                var existingConfig = await _repositoryAccountConfig.GetByIdAsync(request.Id);
                if (existingConfig == null)
                {
                    return this.Fail("配置不存在");
                }

                // 权限验证：非管理员只能删除自己的配置
                if (!permissionContext.IsAdmin && existingConfig.SystemUid != permissionContext.UserId)
                {
                    return this.Fail("无权限删除此配置");
                }

                var result = await _repositoryAccountConfig.DeleteAsync(t => t.Id == request.Id);

                if (result == 1)
                {
                    _logger.Info($"删除账户配置成功: Id={request.Id}, UniqueId={existingConfig.UniqueId}");
                    return this.Success("删除成功");
                }
                else
                {
                    return this.Fail("删除失败");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"删除账户配置失败: Id={request.Id}");
                return this.Fail("删除账户配置失败");
            }
        }

        /// <summary>
        /// 更改账户配置状态
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("changeStatus")]
        [ProducesResponseType(typeof(AjaxResult<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ChangeAccountConfigStatus([FromBody] ChangeAccountConfigStatusRequest request)
        {
            try
            {
                // 获取当前用户的数据权限上下文
                var permissionContext = _dataPermissionContextService.GetCurrentContext();
                if (permissionContext == null)
                {
                    return this.Fail("无法获取用户权限信息");
                }

                // 获取现有配置
                var existingConfig = await _repositoryAccountConfig.GetByIdAsync(request.Id);
                if (existingConfig == null)
                {
                    return this.Fail("配置不存在");
                }

                // 权限验证：非管理员只能修改自己的配置状态
                if (!permissionContext.IsAdmin && existingConfig.SystemUid != permissionContext.UserId)
                {
                    return this.Fail("无权限修改此配置状态");
                }

                // 更新状态
                existingConfig.IsActive = request.IsActive;
                existingConfig.UpdatedAt = DateTime.Now;

                var result = await _repositoryAccountConfig.UpdateAsync(existingConfig);

                if (result == 1)
                {
                    var statusText = request.IsActive ? "启用" : "禁用";
                    _logger.Info($"{statusText}账户配置成功: Id={request.Id}, UniqueId={existingConfig.UniqueId}");
                    return this.Success($"{statusText}成功");
                }
                else
                {
                    return this.Fail("状态更新失败");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"更改账户配置状态失败: Id={request.Id}");
                return this.Fail("更改账户配置状态失败");
            }
        }
    }
}
