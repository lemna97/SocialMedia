using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;
using System.Xml;

namespace Highever.SocialMedia.API
{
    /// <summary>
    /// 控制器文档过滤器 - 为控制器添加标签描述
    /// </summary>
    public class ControllerDocumentationFilter : IDocumentFilter
    {
        private readonly Dictionary<string, XmlDocument> _xmlDocs;
        private readonly ILogger<ControllerDocumentationFilter> _logger;

        public ControllerDocumentationFilter(ILogger<ControllerDocumentationFilter> logger)
        {
            _logger = logger;
            _xmlDocs = LoadAllXmlDocumentation();
        }

        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            var controllerTypes = context.ApiDescriptions
                .Where(desc => desc.ActionDescriptor is ControllerActionDescriptor)
                .Select(desc => ((ControllerActionDescriptor)desc.ActionDescriptor).ControllerTypeInfo)
                .Distinct()
                .ToList();

            foreach (var controllerType in controllerTypes)
            {
                var controllerName = controllerType.Name.Replace("Controller", "");
                var summary = GetControllerSummaryFromXml(controllerType);
                
                if (!string.IsNullOrEmpty(summary))
                {
                    // 查找或创建标签
                    var existingTag = swaggerDoc.Tags?.FirstOrDefault(t => t.Name == controllerName);
                    if (existingTag != null)
                    {
                        existingTag.Description = summary;
                    }
                    else
                    {
                        swaggerDoc.Tags ??= new List<OpenApiTag>();
                        swaggerDoc.Tags.Add(new OpenApiTag
                        {
                            Name = controllerName,
                            Description = summary
                        });
                    }
                    
                    _logger.LogInformation($"设置控制器 {controllerName} 的描述: {summary}");
                }
                else
                {
                    _logger.LogWarning($"未找到控制器 {controllerType.FullName} 的XML注释");
                }
            }
        }

        /// <summary>
        /// 加载所有XML文档
        /// </summary>
        private Dictionary<string, XmlDocument> LoadAllXmlDocumentation()
        {
            var xmlDocs = new Dictionary<string, XmlDocument>();
            
            // 获取所有可能的XML文件路径
            var xmlPaths = GetXmlDocumentationPaths();
            
            foreach (var xmlPath in xmlPaths)
            {
                try
                {
                    if (File.Exists(xmlPath))
                    {
                        var doc = new XmlDocument();
                        doc.Load(xmlPath);
                        var fileName = Path.GetFileNameWithoutExtension(xmlPath);
                        xmlDocs[fileName] = doc;
                        _logger.LogInformation($"成功加载XML文档: {xmlPath}");
                    }
                    else
                    {
                        _logger.LogWarning($"XML文档不存在: {xmlPath}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"加载XML文档失败: {xmlPath}");
                }
            }
            
            return xmlDocs;
        }

        /// <summary>
        /// 获取XML文档路径列表
        /// </summary>
        private List<string> GetXmlDocumentationPaths()
        {
            var paths = new List<string>();
            var baseDirectory = AppContext.BaseDirectory;
            
            // 当前程序集的XML文档
            var currentAssemblyName = Assembly.GetExecutingAssembly().GetName().Name;
            paths.Add(Path.Combine(baseDirectory, $"{currentAssemblyName}.xml"));
            
            // 其他相关程序集的XML文档
            var xmlFiles = new[]
            {
                "Highever.SocialMedia.API.xml",
                "Highever.SocialMedia.Application.Contracts.xml",
                "Highever.SocialMedia.Domain.xml",
                "Highever.SocialMedia.Common.xml"
            };
            
            foreach (var xmlFile in xmlFiles)
            {
                paths.Add(Path.Combine(baseDirectory, xmlFile));
            }
            
            // 开发环境下的XML路径
#if DEBUG
            var projectRoot = Path.Combine(Directory.GetParent(AppContext.BaseDirectory).Parent.Parent.Parent.Parent.FullName);
            var xmlDirectory = Path.Combine(projectRoot, "XML");
            
            foreach (var xmlFile in xmlFiles)
            {
                paths.Add(Path.Combine(xmlDirectory, xmlFile));
            }
#endif
            
            return paths.Distinct().ToList();
        }

        /// <summary>
        /// 从XML文档获取控制器的Summary注释
        /// </summary>
        private string GetControllerSummaryFromXml(Type controllerType)
        {
            try
            {
                var xmlKey = $"T:{controllerType.FullName}";
                _logger.LogDebug($"查找XML键: {xmlKey}");
                
                // 在所有加载的XML文档中查找
                foreach (var kvp in _xmlDocs)
                {
                    var xmlDoc = kvp.Value;
                    var memberNode = xmlDoc.SelectSingleNode($"//member[@name='{xmlKey}']");
                    
                    if (memberNode != null)
                    {
                        var summaryNode = memberNode.SelectSingleNode("summary");
                        if (summaryNode != null)
                        {
                            var summary = summaryNode.InnerText?.Trim();
                            if (!string.IsNullOrEmpty(summary))
                            {
                                _logger.LogDebug($"在 {kvp.Key} 中找到控制器注释: {summary}");
                                return summary;
                            }
                        }
                    }
                }
                
                _logger.LogWarning($"未在任何XML文档中找到控制器 {controllerType.FullName} 的注释");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"解析控制器 {controllerType.FullName} 的XML注释时发生错误");
                return null;
            }
        }
    }
}

