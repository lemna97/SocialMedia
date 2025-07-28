using Highever.SocialMedia.Domain.Entity;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Highever.SocialMedia.Application.Contracts;
using Highever.SocialMedia.Common;

namespace Highever.SocialMedia.API.Controllers
{
    [Route("/")]
    [ApiGroup(SwaggerApiGroup.AIRecruit)]
    [ApiController]
    public class CSVTestController : ControllerBase
    {
        ICSVTestService _csvTestService => _serviceProvider.GetRequiredService<ICSVTestService>();

        public readonly IServiceProvider _serviceProvider;
        public CSVTestController(IServiceProvider serviceProvider)
        {
            this._serviceProvider = serviceProvider;
        }
        /// <summary>
        /// 查询CSVTest所有数据（GET /data）
        /// </summary>
        [HttpGet("data")]
        public async Task<IActionResult> GetData()
        {
            var list = await _csvTestService.GetQueryListAsync(_ => true);
            return Ok(list);
        }

        /// <summary>
        /// 插入一条CSVTest数据（POST /senddata）
        /// </summary>
        [HttpPost("senddata")]
        public async Task<IActionResult> SendData([FromBody] CSVTest entity)
        {
            if (entity == null) return BadRequest("请求体为空");
            await _csvTestService.CreateAsync(entity);
            return Ok(new { msg = "Data inserted successfully" });
        }

        /// <summary>
        /// 根据UserId查询该用户所有的Match（GET /search?name=xxx）
        /// </summary>
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return BadRequest("参数name不能为空");
            // 只返回Match字段，列表
            var list = await _csvTestService.GetQueryListAsync(x => x.UserId == name);
            var matches = list.Select(x => new { Match = x.Match }).ToList();
            return Ok(matches);
        }
    }
}
