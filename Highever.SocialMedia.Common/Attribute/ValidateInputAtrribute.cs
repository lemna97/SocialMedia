using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Net;
using System.Text;

namespace Highever.SocialMedia.Common
{
    /// <summary>
    /// 全局模型验证
    /// </summary>
    public class ValidateInputAtrribute: ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                try
                {
                    var logger = context.HttpContext.RequestServices.GetService(typeof(INLogger));
                    var errors = context.ModelState.Where(t => t.Value.ValidationState == ModelValidationState.Invalid).Select(t =>
                    {
                        var sb = new StringBuilder();
                        sb.AppendFormat("{0}：", t.Key);
                        sb.Append(t.Value.Errors.Select(e => e.ErrorMessage).Aggregate((x, y) => x + "；" + y));
                        return sb.ToString();
                    }).Aggregate((x, y) => x + " \n " + y);
                    logger.Equals(errors);
                    context.Result = new JsonResult(new
                    {
                        code = HttpStatusCode.PaymentRequired,
                        data = string.Empty,
                        msg = errors
                    });
                }
                catch (Exception ex)
                {
                    context.Result = new JsonResult(new
                    {
                        code = HttpStatusCode.NotFound,
                        data = string.Empty,
                        msg = ex.Message
                    });
                }
            }
        }
    }
}
