using Highever.SocialMedia.Application.Contracts;
using Highever.SocialMedia.Common;
using Highever.SocialMedia.Domain;
using Highever.SocialMedia.Domain.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace Highever.SocialMedia.Admin.Controllers
{
    /// <summary>
    /// AI-Boss直聘平台
    /// </summary>
    [EnableCors("AllowSpecificOrigins")] // 应用指定的 CORS 策略
    [ApiGroup(SwaggerApiGroup.AIRecruit)]
    [Route("Recruit")]
    [ApiController]
    public class AIRecruitHomeController : Controller
    {
        IJobTitleService _jobTitleService => _serviceProvider.GetRequiredService<IJobTitleService>();
        IJobSeekerService _jobSeekerService => _serviceProvider.GetRequiredService<IJobSeekerService>();
        INLogger _logger => _serviceProvider.GetRequiredService<INLogger>();

        public readonly IServiceProvider _serviceProvider;
        public AIRecruitHomeController(IServiceProvider serviceProvider)
        {
            this._serviceProvider = serviceProvider;
        }
        /// <summary>
        ///  录入职位信息
        /// </summary>
        /// <param name="jobTitle"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        [HttpPost("AddOrUpdateJobTitle")]
        [Produces("application/json")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(List<JobTitle>), StatusCodes.Status200OK)]
        public async Task<IActionResult> AddOrUpdateJobTitle(List<JobTitle> jobTitle)
        {
            if (jobTitle == null)
            {
                throw new ArgumentNullException(nameof(jobTitle));
            } 
            await _jobTitleService.CreateAsync(jobTitle);
            return Json(new AjaxResult<object>() { data = { }, code = HttpCode.成功 });
        }

        /// <summary>
        /// 录入打招呼记录
        /// </summary>
        /// <param name="jobSeekers"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        [HttpPost("AddOrUpdateJobSeeker")]
        [Produces("application/json")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(List<JobSeeker>), StatusCodes.Status200OK)]
        public async Task<IActionResult> AddOrUpdateJobSeeker([FromBody] List<JobSeeker> jobSeekers)
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);
            if (jobSeekers == null)
            {
                throw new ArgumentNullException(nameof(jobSeekers));
            }
            var temp_data = jobSeekers.Select(t => t.EncryptGeekId);
            var data = await _jobSeekerService.GetQueryListAsync(t => temp_data.Contains(t.EncryptGeekId));
            if (data != null && data.Any())
            {
                jobSeekers = jobSeekers.Where(t => !temp_data.Contains(t.EncryptGeekId)).ToList();
            }
            if (jobSeekers?.Count > 0)
            {
                await _jobSeekerService.CreateAsync(jobSeekers);
            }
            var temp_count = await _jobSeekerService.GetQueryListAsync(
      t => t.CollectedAt >= today && t.CollectedAt < tomorrow
  );
            return Json(new AjaxResult<object>() { data = temp_count?.Count, code = HttpCode.成功 });
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="encryptJobId"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        [HttpGet("GetJobTitle")]
        [Produces("application/json")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(JobTitle), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetJobTitle([FromQuery] string encryptJobId)
        {
            if (!encryptJobId.IsNullOrEmpty())
            {
                throw new ArgumentNullException(nameof(encryptJobId));
            }
            var temp_data = await _jobTitleService.GetQueryListAsync(t => t.EncryptJobId == encryptJobId);
            return Json(new AjaxResult<object>() { data = temp_data?.OrderByDescending(t=>t.CollectedAt).FirstOrDefault(), code = HttpCode.成功 });
        }
    }
}
