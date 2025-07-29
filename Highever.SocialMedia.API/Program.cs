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

#region 同步 I/O 配置
// AllowSynchronousIO 配置默认Kestrel服务器是否允许请求和响应使用同步 I/O
// builder.Services.Configure<KestrelServerOptions>(x => x.AllowSynchronousIO = true);
// 配置IIS服务器是否允许请求和响应使用同步 I/O
// .Configure<IISOptions>(x => x.AllowSynchronousIO = true);
#endregion 

// 配置HTTP上下文访问器
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
    options.Cookie.Name = "lemna.api.cookie";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.IdleTimeout = TimeSpan.FromMinutes(30); // 默认是20分钟
});

// 配置内存缓存存储session(只适合单机构) 
builder.Services.AddDistributedMemoryCache();
#endregion

#region UI 配置
builder.Services.AddSwaggerGen(c =>
{
    // 使用 GetSwaggerGroups 配置每个分组
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
    
    // 配置接口唯一标识符
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
    
    // 包含所有 XML 文档
    c.IncludeXmlComments(Path.Combine(xmlPath, "Highever.SocialMedia.API.xml"));
    c.IncludeXmlComments(Path.Combine(xmlPath, "Highever.SocialMedia.Application.Contracts.xml"));
    c.IncludeXmlComments(Path.Combine(xmlPath, "Highever.SocialMedia.Domain.xml"));
    c.IncludeXmlComments(Path.Combine(xmlPath, "Highever.SocialMedia.Common.xml"));
    
    c.SchemaFilter<EnumSchemaFilter>();
    // 自定义 Authorization 参数
    c.OperationFilter<AuthorizationParameterFilter>();
    // 自定义 隐藏接口
    c.DocumentFilter<HiddenApiFilter>();
    // 注册自定义默认方法操作过滤器
    c.OperationFilter<AutoHttpMethodOperationFilter>();
});
#endregion 

#region Nlog 日志配置
builder.Logging.ClearProviders(); // 删除所有已经注册的日志处理程序
builder.Logging.SetMinimumLevel(LogLevel.Trace); // 指定日志级别
builder.Host.UseNLog();
// 添加控制台日志
builder.Logging.AddConsole();
#endregion

#region 依赖注入
builder.Services.Register();
#endregion 

#region 配置 TextJson 序列化
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
    // 数据格式原样输出
    // options.JsonSerializerOptions.PropertyNamingPolicy = null;
    // 忽略空值
    // options.JsonSerializerOptions.IgnoreNullValues = false;
    // 允许尾随逗号(允许在指定对象末尾的逗号，在序列化时忽略注释或逗号，如果抛出异常，可以设置AllowTrailingCommas属性，默认为false）
    // options.JsonSerializerOptions.AllowTrailingCommas = false;
    // 反序列化过程中属性名称是否使用不区分大小写的比较
    // options.JsonSerializerOptions.PropertyNameCaseInsensitive = false;
    options.JsonSerializerOptions.Converters.Add(new LongToStringConverter()); // 自定义 long 转 string Converter
    // 所有接收的数字字段都会被解析为 string 类型，这样前端发送的数字只要是参数值
    // options.JsonSerializerOptions.Converters.Add(new StringJsonConverter());
});
#endregion

#region 配置 CORS 跨域
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        policy.WithOrigins("http://localhost:8068", "https://example.com", "http://127.0.0.1", "http://erpauth.amz-marketing.com:10001") // 允许特定源
              .AllowAnyMethod() // 允许所有 HTTP 方法 (GET, POST, PUT, DELETE 等)
              .AllowAnyHeader() // 允许所有请求头
              .AllowCredentials(); // 如果需要支持传递凭据的请求 (如 Cookies 或 Authorization Header)
    });
    // 如果需要允许所有源，可以在开发时使用
    // options.AddPolicy("AllowAll", policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});
#endregion

// 指定端口号
builder.WebHost.UseKestrel().UseUrls($"http://*:{AppSettingConifgHelper.ReadAppSettings("HOST_PORT")}").UseIIS();

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

// CSP 内容安全策略配置
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("Content-Security-Policy", "default-src 'self' http://erpauth.amz-marketing.com:10001;");
    await next();
});

app.UseAuthorization();
app.MapControllers();

app.Run();
