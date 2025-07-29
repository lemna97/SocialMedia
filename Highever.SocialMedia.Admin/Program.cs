using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using Highever.SocialMedia.API;
using Highever.SocialMedia.Common;
using IGeekFan.AspNetCore.Knife4jUI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using NLog.Web;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    EnvironmentName = Environments.Development
});

// Body 参数校验过滤器
builder.Services.AddControllers(options =>
{
    // 全局模型字段过滤器，可选：SuppressModelStateInvalidFilter 设置成 True
    options.Filters.Add<ValidateInputAtrribute>();
});

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    // 如果启用了 ValidateInputAtrribute 全局模型字段验证，则方法 ApiBehaviorOptions 自动验证一定要禁用
    options.SuppressModelStateInvalidFilter = true;
});

// 配置HTTP上下文
builder.Services.AddHttpContextAccessor();
builder.Services.AddEndpointsApiExplorer();

// 配置全局选项
builder.Services.AddOptions();

#region 内存缓存 / Session 配置
// 配置内存缓存
builder.Services.AddMemoryCache();

// 配置session
builder.Services.AddSession(options =>
{
    options.Cookie.Name = "admin.api.cookie";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.IdleTimeout = TimeSpan.FromMinutes(30); // 默认是20分钟
});

// 配置内存缓存存储session(只适合单机构) 
builder.Services.AddDistributedMemoryCache();
#endregion

#region UI配置
builder.Services.AddSwaggerGen(c =>
{
    // 使用 ConfigureSwaggerGroups 配置分组
    c.ConfigureSwaggerGroups();
    
    // 判断接口归属哪个分组
    c.DocInclusionPredicate((docName, apiDescription) =>
    {
        if (docName == SwaggerGroups.Name)
        {
            return string.IsNullOrEmpty(apiDescription.GroupName)
                   || string.IsNullOrWhiteSpace(apiDescription.GroupName);
        }
        return apiDescription.GroupName == docName;
    });
    
    // 配置接口唯一标识
    c.CustomOperationIds(apiDesc =>
    {
        var controllerAction = apiDesc.ActionDescriptor as ControllerActionDescriptor;
        var parameters = string.Join("-", apiDesc.ParameterDescriptions.Select(p => p.Name));
        return $"{controllerAction.ControllerName}-{controllerAction.ActionName}-{parameters}";
    });
    
    // 获取 XML 文件路径，配置数据库实体和方法的注释
    var projectRoot = Directory.GetCurrentDirectory();
    var xmlPath = Path.Combine(projectRoot);
#if DEBUG
    // 开发环境下获取上级项目目录 
    projectRoot = Path.Combine(Directory.GetParent(AppContext.BaseDirectory).Parent.Parent.Parent.Parent.FullName);
    xmlPath = Path.Combine(projectRoot, "XML");
#endif
    
    // 包含所有 XML 文档 - 检查文件是否存在
    var xmlFiles = new[]
    {
        "Highever.SocialMedia.Admin.xml",
        "Highever.SocialMedia.Application.Contracts.xml",
        "Highever.SocialMedia.Domain.xml",
        "Highever.SocialMedia.Common.xml"
    };
    
    foreach (var xmlFile in xmlFiles)
    {
        var xmlFilePath = Path.Combine(xmlPath, xmlFile);
        if (File.Exists(xmlFilePath))
        {
            c.IncludeXmlComments(xmlFilePath);
        }
    }
    
    c.SchemaFilter<EnumSchemaFilter>();
    // 自定义 Authorization 参数
    c.OperationFilter<AuthorizationParameterFilter>();
    // 自定义 隐藏接口
    c.DocumentFilter<HiddenApiFilter>();
    // 注册自定义默认方法操作过滤器
    c.OperationFilter<AutoHttpMethodOperationFilter>();
});
#endregion

#region Nlog 配置
builder.Logging.ClearProviders(); // 删除所有已经注册的日志处理程序
builder.Logging.SetMinimumLevel(LogLevel.Trace); // 指定级别
builder.Host.UseNLog();
// 添加控制台日志
builder.Logging.AddConsole();
#endregion

#region 依赖注入
builder.Services.Register();
#endregion

#region 配置TextJson
builder.Services.AddControllers().AddJsonOptions(options =>
{
    // 格式化日期时间格式
    options.JsonSerializerOptions.Converters.Add(new SystemTextJsonExtension.DatetimeJsonConverter());
    // 忽略循环引用
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    // 格式化缩进
    options.JsonSerializerOptions.WriteIndented = true;
    // 数据格式首字母小写
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    // 取消Unicode编码
    options.JsonSerializerOptions.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);
    // 自定义 long 转 string Converter
    options.JsonSerializerOptions.Converters.Add(new LongToStringConverter());
});
#endregion

#region 配置 CORS 跨域
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        policy.WithOrigins("http://localhost:8068", "https://example.com", "http://127.0.0.1", "http://erpauth.amz-marketing.com:10001")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});
#endregion

// 指定端口号
builder.WebHost.UseKestrel().UseUrls($"http://*:{AppSettingConifgHelper.ReadAppSettings("ADMIN_PORT") ?? "5193"}").UseIIS();

var app = builder.Build();

// 异常处理（放在最开始的第一个中间件里面）
app.UseCustomExceptionHandler();

// 服务定位器
ServiceLocator.SetLocatorProvider(app.Services);

// DI容器中的服务列表
app.RegisteredServicesPage(builder.Services);

// 加载静态资源
app.UseStaticFiles();

// 配置路由 
app.UseRouting();

// 使用 CORS 跨域中间件
app.UseCors("AllowSpecificOrigins");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.ConfigureKnife4UI();
}

// CSP配置
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("Content-Security-Policy", "default-src 'self' http://erpauth.amz-marketing.com:10001;");
    await next();
});

app.UseAuthorization();
app.MapControllers();

app.Run();
