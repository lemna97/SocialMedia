using Microsoft.AspNetCore.Mvc;

namespace Highever.SocialMedia.Common
{
    /// <summary>
    /// 控制器扩展方法
    /// </summary>
    public static class ControllerExtensions
    {
        /// <summary>
        /// 返回成功响应（无数据）
        /// </summary>
        public static IActionResult Ok(this ControllerBase controller, string message = "操作成功")
        {
            return controller.Ok(AjaxResult<object>.Success(null, message));
        }

        /// <summary>
        /// 返回成功响应（带数据）
        /// </summary>
        public static IActionResult Ok<T>(this ControllerBase controller, T data, string message = "操作成功") where T : class
        {
            return controller.Ok(AjaxResult<T>.Success(data, message));
        }

        /// <summary>
        /// 返回失败响应
        /// </summary>
        public static IActionResult Ok(this ControllerBase controller, string message = "操作失败", HttpCode code = HttpCode.失败)
        {
            return controller.Ok(AjaxResult<object>.Fail(message, code));
        }

        /// <summary>
        /// 返回失败响应（指定类型）
        /// </summary>
        public static IActionResult Fail(this ControllerBase controller, string message = "操作失败")
        {
            return controller.Fail(message);
        }
        /// <summary>
        /// 返回失败响应（指定类型）
        /// </summary>
        public static IActionResult Fail<T>(this ControllerBase controller, string message = "操作失败", HttpCode code = HttpCode.失败) where T : class
        {
            return controller.Ok(AjaxResult<T>.Fail(message, code));
        }

        /// <summary>
        /// 返回分页响应
        /// </summary>
        public static IActionResult Page<T>(this ControllerBase controller, List<T> items, int total, string message = "查询成功") where T : class
        {
            var pageResult = new PageResult<T> { Items = items, Total = total };
            return controller.Ok(AjaxResult<PageResult<T>>.Success(pageResult, message));
        }

        /// <summary>
        /// 返回JSON格式的成功响应（兼容现有Json方法）
        /// </summary>
        public static IActionResult JsonOk<T>(this ControllerBase controller, T data, string message = "操作成功") where T : class
        {
            return controller.Ok(AjaxResult<T>.Success(data, message));
        }

        /// <summary>
        /// 返回JSON格式的失败响应
        /// </summary>
        public static IActionResult JsonError(this ControllerBase controller, string message = "操作失败", HttpCode code = HttpCode.失败)
        {
            return controller.Ok(AjaxResult<object>.Fail(message, code));
        }

        /// <summary>
        /// 返回成功响应 
        /// </summary>
        public static IActionResult Success<T>(this ControllerBase controller, T data = null, string message = "操作成功") where T : class
        {
            return controller.Ok(AjaxResult<T>.Success(data, message));
        }
         
        /// <summary>
        /// 返回成功响应（无数据）
        /// </summary>
        public static IActionResult Success(this ControllerBase controller, string message = "操作成功")
        {
            return controller.Ok(AjaxResult<object>.Success(null, message));
        }
    }
}



