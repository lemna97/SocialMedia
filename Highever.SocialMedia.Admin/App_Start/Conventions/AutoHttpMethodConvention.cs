using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Routing;

namespace Highever.SocialMedia.Admin
{
    /// <summary>
    /// 自动HTTP方法约定 - 在路由构建时自动为Action分配HTTP方法
    /// </summary>
    public class AutoHttpMethodConvention : IActionModelConvention
    {
        public void Apply(ActionModel action)
        {
            // 如果Action已经有HTTP方法特性，跳过
            if (HasHttpMethodAttribute(action))
            {
                return;
            }

            // 根据Action名称推断HTTP方法并添加选择器
            var httpMethod = GetHttpMethod(action.ActionName);
            var selectorModel = new SelectorModel
            {
                ActionConstraints = { new HttpMethodActionConstraint(new[] { httpMethod }) }
            };
            
            action.Selectors.Add(selectorModel);
        }

        /// <summary>
        /// 检查Action是否已有HTTP方法特性
        /// </summary>
        private bool HasHttpMethodAttribute(ActionModel action)
        {
            return action.Selectors.Any(s => 
                s.ActionConstraints.Any(c => c is HttpMethodActionConstraint));
        }

        /// <summary>
        /// 根据Action名称获取对应的HTTP方法
        /// </summary>
        private string GetHttpMethod(string actionName)
        {
            var lowerActionName = actionName.ToLower();

            // 根据 Action 名称前缀自动推断 HTTP 方法
            if (lowerActionName.StartsWith("get") ||
                lowerActionName.Equals("index") ||
                lowerActionName.Equals("details") ||
                lowerActionName.Equals("list"))
            {
                return "GET";
            }
            else if (lowerActionName.StartsWith("put") ||
                     lowerActionName.StartsWith("update") ||
                     lowerActionName.StartsWith("edit"))
            {
                return "PUT";
            }
            else if (lowerActionName.StartsWith("delete") ||
                     lowerActionName.StartsWith("remove"))
            {
                return "DELETE";
            }
            else if (lowerActionName.StartsWith("patch"))
            {
                return "PATCH";
            }
            else
            {
                // 默认给 POST（包括post、create、add等）
                return "POST";
            }
        }
    }
}
