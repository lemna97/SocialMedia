using Highever.SocialMedia.Application.Contracts.Context;
using Microsoft.AspNetCore.Http;

namespace Highever.SocialMedia.Application.Context
{
    /// <summary>
    /// 数据权限上下文服务实现
    /// </summary>
    public class DataPermissionContextService : IDataPermissionContextService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string CONTEXT_KEY = "DataPermissionContext";

        public DataPermissionContextService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public DataPermissionContext? GetCurrentContext()
        {
            return _httpContextAccessor.HttpContext?.Items[CONTEXT_KEY] as DataPermissionContext;
        }

        public void SetCurrentContext(DataPermissionContext context)
        {
            if (_httpContextAccessor.HttpContext != null)
            {
                _httpContextAccessor.HttpContext.Items[CONTEXT_KEY] = context;
            }
        }

        public void ClearContext()
        {
            if (_httpContextAccessor.HttpContext != null)
            {
                _httpContextAccessor.HttpContext.Items.Remove(CONTEXT_KEY);
            }
        }
    }
}