using Microsoft.AspNetCore.Mvc;
using Highever.SocialMedia.Common;

namespace Highever.SocialMedia.Admin.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TaskLogTestController : Controller
    {
        private readonly INLogger _logger;

        public TaskLogTestController(INLogger logger)
        {
            _logger = logger;
        }

        [HttpGet("test-task-logs")]
        public IActionResult TestTaskLogs()
        {
            try
            {
                _logger.TaskStart("TestTask", 1, 1001);
                _logger.TaskInfo("TestTask", "开始处理用户数据", 1, 1001);
                _logger.TaskBatchInfo("TestTask", 1, 10, 8, 2, 1001);
                _logger.TaskApiCall("TestTask", "test_user", true, null, 1001);
                _logger.TaskApiCall("TestTask", "test_user2", false, "API限流", 1001);
                _logger.TaskComplete("TestTask", 5000, 10, 8, 2, 10, 1, 1001);
                
                return Ok("任务日志测试完成，请检查 task_logs 表和日志文件");
            }
            catch (Exception ex)
            {
                _logger.TaskError("TestTask", ex, 1, 1001);
                return BadRequest($"任务日志测试失败: {ex.Message}");
            }
        }
    }
}