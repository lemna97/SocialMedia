using Highever.Amazon.Advertising.Common;
using Highever.SocialMedia.Application.Contracts;
using Highever.SocialMedia.Common;
using Highever.SocialMedia.Common.Model;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace Highever.SocialMedia.API.Controllers
{
    [EnableCors("AllowSpecificOrigins")] // 应用指定的 CORS 策略
    [ApiGroup(SwaggerApiGroup.Login)]
    [Route("Home")]
    [ApiController]
    public class HomeController : Controller
    {
        private readonly HttpClientHelper _httpClientHelper;

        IDistributionProductsAppService _distributionProductsAppService => _serviceProvider.GetRequiredService<IDistributionProductsAppService>();
        /// <summary>
        /// 
        /// </summary>
        public readonly IServiceProvider _serviceProvider;
        /// <summary>
        /// 
        /// </summary>
        public HomeController(IServiceProvider serviceProvider, HttpClientHelper httpClientHelper)
        {
            this._serviceProvider = serviceProvider;
            this._httpClientHelper = httpClientHelper;
        }
        [HttpGet]
        [Route("FetchAsync")]
        public async Task<IActionResult> FetchAsync()
        { 
            string html = await AgencyHttpHelper.FetchAsync("https://www.tiktok.com/@get.whit.it");
            Console.WriteLine(html);
            return this.Ok("获取成功");
        }


        /// <summary>
        /// 转发调用 TikHub 搜索接口的测试端点
        /// </summary>
        /// <remarks>
        /// GET /api/TikHubTest/search?keyword=pubg
        /// </remarks>  
        [HttpGet]
        [Route("tk_search")]
        public async Task<IActionResult> SearchAsync(
            [FromQuery] string keyword, CancellationToken ct)
        {
            const string url =
           "https://api.tikhub.io/api/v1/tiktok/app/v3/fetch_general_search_result";
            const string token =
                "0oMxQX8hxiRs4yYhy1GNiUM46RVb04yFq4s3+j3YqEBmzXIDJ/ls9ii1AA==";

            var query = new TikHubSearchRequest(keyword);
            var headers = new Dictionary<string, string> { ["Authorization"] = token };

            var resp = await _httpClientHelper.GetAsync<TkApiResponse>(url, query, headers, ct);
            return this.Success<TkApiResponse>(resp, "搜索成功");
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetCookie")]
        public async Task<IActionResult> GetCookie()
        {
            var cookie = await CookieDumper.GetCookie();
            return this.Success(cookie, "获取Cookie成功");
        }
        /// <summary>
        /// 测试XML
        /// </summary>
        /// <remarks> 
        /// Sample request:
        /// ```
        ///  POST /hotmap
        ///  { 
        ///      "displayName": "演示名称1",
        ///      "matchRule": 0,
        ///      "matchCondition": "https://www.cnblogs.com/JulianHuang/",
        ///      "targetUrl": "https://www.cnblogs.com/JulianHuang/",
        ///      "versions": [
        ///      {
        ///         "versionName": "ver2020",
        ///         "startDate": "2020-12-13T10:03:09",
        ///         "endDate": "2020-12-13T10:03:09",
        ///         "offlinePageUrl": "3fa85f64-5717-4562-b3fc-2c963f66afa6",  //  没有绑定图片和离线网页的对应属性传 null
        ///         "pictureUrl": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///         "createDate": "2020-12-13T10:03:09"
        ///      }
        ///    ] 
        ///  }
        ///```
        /// </remarks>
        /// <param name="testTable_Para">请求参数</param>
        /// <returns></returns>  
        [HttpPost(Name = "TestMyDbcontext_QueryList")]
        [Consumes("application/json"), Produces("application/json")]
        public async Task<IActionResult> TestMyDbcontext_QueryList([FromBody] SearchForSemiSupplierModel testTable_Para)
        {
            var data = await _distributionProductsAppService.GetQueryListAsync(t => true);
            return this.Success(data.AsQueryable(), "查询成功");
        }
    }
}
