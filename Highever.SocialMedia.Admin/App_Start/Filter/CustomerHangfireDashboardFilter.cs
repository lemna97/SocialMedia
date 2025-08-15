using Hangfire.Dashboard;
using System.Diagnostics.CodeAnalysis;

namespace Highever.SocialMedia.Admin
{
    public class CustomerHangfireDashboardFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize([NotNull] DashboardContext context)
        {

            return true;
        }
    }
}
