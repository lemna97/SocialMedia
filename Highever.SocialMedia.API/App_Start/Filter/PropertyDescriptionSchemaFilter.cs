using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;
using System.Xml.Linq;

namespace Highever.SocialMedia.API
{
    /// <summary>
    /// 属性描述增强过滤器
    /// </summary>
    public class PropertyDescriptionSchemaFilter : ISchemaFilter
    {
        private readonly Dictionary<string, XDocument> _xmlDocs;

        public PropertyDescriptionSchemaFilter()
        {
            _xmlDocs = new Dictionary<string, XDocument>();
            LoadXmlDocuments();
        }

        private void LoadXmlDocuments()
        {
            var xmlFiles = Directory.GetFiles(AppContext.BaseDirectory, "*.xml");
            foreach (var xmlFile in xmlFiles)
            {
                try
                {
                    var doc = XDocument.Load(xmlFile);
                    _xmlDocs[Path.GetFileNameWithoutExtension(xmlFile)] = doc;
                }
                catch
                {
                    // 忽略加载失败的文件
                }
            }
        }

        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (schema.Properties == null) return;

            foreach (var property in schema.Properties)
            {
                var propertyInfo = context.Type.GetProperty(
                    property.Key, 
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase
                );

                if (propertyInfo != null)
                {
                    var memberName = $"P:{propertyInfo.DeclaringType?.FullName}.{propertyInfo.Name}";
                    var description = GetXmlDocumentation(memberName);
                    
                    if (!string.IsNullOrEmpty(description))
                    {
                        property.Value.Description = description;
                        
                        // 如果是复杂类型，添加类型说明
                        if (IsComplexType(propertyInfo.PropertyType))
                        {
                            var typeDescription = GetTypeDescription(propertyInfo.PropertyType);
                            if (!string.IsNullOrEmpty(typeDescription))
                            {
                                property.Value.Description += $" ({typeDescription})";
                            }
                        }
                    }
                }
            }
        }

        private string GetXmlDocumentation(string memberName)
        {
            foreach (var xmlDoc in _xmlDocs.Values)
            {
                var memberElement = xmlDoc.Descendants("member")
                    .FirstOrDefault(x => x.Attribute("name")?.Value == memberName);

                if (memberElement != null)
                {
                    var summaryElement = memberElement.Element("summary");
                    return summaryElement?.Value?.Trim();
                }
            }
            return null;
        }

        private string GetTypeDescription(Type type)
        {
            var typeName = $"T:{type.FullName}";
            return GetXmlDocumentation(typeName);
        }

        private bool IsComplexType(Type type)
        {
            return !type.IsPrimitive && 
                   !type.IsEnum && 
                   type != typeof(string) && 
                   type != typeof(DateTime) && 
                   type != typeof(decimal) && 
                   !type.IsGenericType ||
                   (type.IsGenericType && type.GetGenericTypeDefinition() != typeof(Nullable<>));
        }
    }
}