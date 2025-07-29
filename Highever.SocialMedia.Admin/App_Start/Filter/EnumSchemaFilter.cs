using Highever.SocialMedia.Common;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text;

namespace Highever.SocialMedia.Admin
{
    /// <summary>
    /// 枚举注释
    /// </summary>
    public class EnumSchemaFilter : ISchemaFilter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <param name="context"></param>
        public void Apply(OpenApiSchema model, SchemaFilterContext context)
        {

            if (context.Type != null && context.Type.IsEnum)
            {
                StringBuilder stringBuilder = new StringBuilder();
                Enum.GetNames(context.Type)
                    .ToList()
                    .ForEach(name =>
                    {
                        Enum e = (Enum)Enum.Parse(context.Type, name);
                        var data = $"{name}({e.GetDescription()})={Convert.ToInt64(Enum.Parse(context.Type, name))}";

                        stringBuilder.AppendLine(data);
                    });
                model.Description = stringBuilder.ToString();
                model.Type = context.Type.Name;
                model.Format = context.Type.Name;
            }
        }

    }
}
