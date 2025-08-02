using Highever.SocialMedia.Common;
using Highever.SocialMedia.Domain.Entity;
using Highever.SocialMedia.Domain.Repository;
using Microsoft.AspNetCore.Mvc;

namespace Highever.SocialMedia.Admin.Controllers
{
    [ApiController]
    [Route("api/user")]
    public class HomeController : Controller
    {
        INLogger _logger => _serviceProvider.GetRequiredService<INLogger>();

        private IRepository<Users> _repositoryUsers  =>_serviceProvider.GetRequiredService<IRepository<Users>>();
        /// <summary>
        /// 
        /// </summary>
        public readonly IServiceProvider _serviceProvider;
        public HomeController(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider; 
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
