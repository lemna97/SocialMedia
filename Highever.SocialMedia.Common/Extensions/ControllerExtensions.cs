using Microsoft.AspNetCore.Mvc;
using NPOI.SS.Formula.Functions;

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
        public static IActionResult JsonOk(this ControllerBase controller)
        {
            return controller.Ok(AjaxResult<object>.Success(null, "操作成功"));
        } 
        
        /// <summary>
        /// 返回成功响应（无数据）
        /// </summary>
        public static IActionResult JsonOk(this ControllerBase controller, string message = "操作成功")
        {
            return controller.Ok(AjaxResult<object>.Success(null, message));
        } 

        /// <summary>
        /// 返回失败响应
        /// </summary>
        public static IActionResult JsonOk(this ControllerBase controller, string message = "操作失败", HttpCode code = HttpCode.成功)
        {
            return controller.Ok(AjaxResult<object>.Fail(message, code));
        }

        /// <summary>
        /// 返回失败响应（指定类型）
        /// </summary>
        public static IActionResult Fail(this ControllerBase controller, string message = "操作失败")
        {
            return controller.Ok(AjaxResult<T>.Fail(message));
        }
        /// <summary>
        /// 返回失败响应（指定类型）
        /// </summary>
        public static IActionResult Fail<T>(this ControllerBase controller, string message = "操作失败", HttpCode code = HttpCode.失败) where T : class
        {
            return controller.Ok(AjaxResult<T>.Fail(message, code));
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

        /// <summary>
        /// 返回完整分页响应（包含页码、页大小、总页数等信息）
        /// </summary>
        public static IActionResult PagedResult<T>(this ControllerBase controller, PageResult<T> pageResult, string message = "查询成功") where T : class
        {
            return controller.Ok(new AjaxResult<object>
            {
                code = HttpCode.成功,
                msg = message,
                data = new
                {
                    items = pageResult.Items,
                    totalCount = pageResult.totalCount,
                    pageIndex = pageResult.PageIndex,
                    pageSize = pageResult.PageSize,
                    totalPages = pageResult.TotalPages
                }
            });
        }

        /// <summary>
        /// 返回完整分页响应（直接传入参数）
        /// </summary>
        public static IActionResult PagedResult<T>(this ControllerBase controller, 
            List<T> items, 
            int totalCount, 
            int pageIndex, 
            int pageSize, 
            string message = "查询成功") where T : class
        {
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            
            return controller.Ok(new AjaxResult<object>
            {
                code = HttpCode.成功,
                msg = message,
                data = new
                {
                    items,
                    totalCount,
                    pageIndex,
                    pageSize,
                    totalPages
                }
            });
        } 
    }
}








