using Highever.SocialMedia.Common;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Highever.SocialMedia.API
{
    public class CustomExcludeFilter : ISchemaFilter
    {
        /// <summary>
        ///  自定义请求参数显示
        /// </summary>
        /// <param name="schema"></param>
        /// <param name="context"></param>
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (schema?.Properties?.IsNullOrEmpty() ?? true)
            {
                return;
            }

            var excludedProperties = context.Type.GetProperties();
            foreach (var property in excludedProperties)
            {
                var attributes = property.GetCustomAttributes(true);
                var excludeAttributes = attributes.OfType<SwaggerExcludeAttribute>();
                if (!excludeAttributes.IsNullOrEmpty()
                    && schema.Properties.ContainsKey(property.Name))
                { 
                    schema.Properties.Remove(property.Name);
                }
            };
        }
    }
}
