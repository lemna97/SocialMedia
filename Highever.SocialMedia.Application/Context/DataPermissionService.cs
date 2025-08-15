using Highever.SocialMedia.Common;
using Highever.SocialMedia.Domain.Entity;
using Highever.SocialMedia.Domain.Repository;

namespace Highever.SocialMedia.Application.Context
{
    /// <summary>
    /// 数据权限服务实现
    /// </summary>
    public class DataPermissionService : IDataPermissionService
    {
        private readonly IRepository<AccountConfig> _accountConfigConfigRepository;
        private readonly INLogger _logger;

        public DataPermissionService(
            IRepository<AccountConfig> accountConfigConfigRepository,
            INLogger logger)
        {
            _accountConfigConfigRepository = accountConfigConfigRepository;
            _logger = logger;
        }

        /// <summary>
        /// 刷新当用户的数据权限
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="roles"></param>
        /// <returns></returns>
        public async Task<DataPermissionContext> LoadDataPermissionAsync(long userId, List<string> roles)
        {
            var context = new DataPermissionContext
            {
                UserId = userId,
                Roles = roles ?? new List<string>()
            };

            try
            {
                // 如果是管理员，不需要加载具体的配置ID
                if (context.IsAdmin)
                {
                    _logger.Info($"用户 {userId} 是管理员，跳过TikTok配置加载");
                    return context;
                }

                // 加载用户的配置配置
                context.AccountConfigUniqueIds = await GetUserAccountConfigIdsAsync(userId);

                _logger.Info($"用户 {userId} 加载到 {context.AccountConfigUniqueIds.Count} 个账号配置");

                return context;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"加载用户 {userId} 数据权限失败");
                return context; // 返回基础上下文，避免阻塞请求
            }
        }

        public async Task<List<string>> GetUserAccountConfigIdsAsync(long userId)
        {
            var userConfigs = await _accountConfigConfigRepository.QueryListAsync(
                x => x.SystemUid == userId && x.IsActive == true);

            // 返回 UniqueId 而不是 Id
            return userConfigs.Select(x => x.UniqueId).ToList();
        }
    }
}
