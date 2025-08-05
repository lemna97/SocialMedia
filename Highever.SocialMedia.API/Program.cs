using Highever.SocialMedia.API;
using Highever.SocialMedia.API.Handlers;
using Highever.SocialMedia.Common;
using Highever.SocialMedia.Common.Extension;
using Highever.SocialMedia.Common.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.IdentityModel.Tokens;
using NLog.Web;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    EnvironmentName = Environments.Development
});


// 注册所有配置服务
builder.Services.AddConfigurations(builder.Configuration);
builder.Services.AddLegacyConfigurations(builder.Configuration);
// 获取JWT设置用于认证配置
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
// 配置JWT认证
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidateAudience = true,
        ValidAudience = jwtSettings.Audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    }; 
    options.Events = new JwtBearerEvents
    {
        /// <summary>
        /// OnMessageReceived: 接收到HTTP请求时触发
        /// 作用：自定义Token获取方式，支持从多个地方获取Token
        /// 触发时机：每次HTTP请求到达时，在验证Token之前
        /// </summary>
        OnMessageReceived = context =>
        {
            // 1. 优先从Authorization Header获取 (默认方式)
            // 格式: Authorization: Bearer <token>
            // 这个是JWT Bearer认证的标准方式，框架会自动处理

            // 2. 支持从Cookie中获取token
            var token = context.Request.Cookies["auth_token"];
            if (!string.IsNullOrEmpty(token))
            {
                context.Token = token;
            }

            // 3. 支持从查询参数中获取token
            var accessToken = context.Request.Query["token"];
            if (!string.IsNullOrEmpty(accessToken))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        },

        /// <summary>
        /// OnAuthenticationFailed: Token验证失败时触发
        /// 作用：处理Token验证失败的情况（如Token格式错误、签名无效等）
        /// 触发时机：Token解析或验证过程中发生异常时
        /// 常见原因：Token格式错误、签名无效、密钥不匹配等
        /// </summary>
        OnAuthenticationFailed = CustomAuthenticationHandler.HandleAuthenticationFailed,

        /// <summary>
        /// OnChallenge: 需要身份验证但未提供有效Token时触发
        /// 作用：处理未登录或Token过期的情况
        /// 触发时机：访问需要认证的接口但没有有效Token时
        /// 常见原因：未提供Token、Token已过期、Token无效等
        /// </summary>
        OnChallenge = CustomAuthenticationHandler.HandleChallenge,

        /// <summary>
        /// OnForbidden: 身份验证成功但权限不足时触发
        /// 作用：处理权限不足的情况
        /// 触发时机：Token有效但用户没有访问特定资源的权限时
        /// 常见原因：用户角色不匹配、缺少特定权限等
        /// </summary>
        OnForbidden = CustomAuthenticationHandler.HandleForbidden
    };
});

// Body 参数校验过滤器
builder.Services.AddControllers(options =>
{
    // 全局模型字段过滤器
    options.Filters.Add<ValidateInputAtrribute>();
    
    // 添加自动HTTP方法约定
    options.Conventions.Add(new AutoHttpMethodConvention());
    
    // 添加全局授权过滤器 - 默认所有接口都需要认证
    options.Filters.Add(new AuthorizeFilter());
}) 
.ConfigureApplicationPartManager(manager =>
{
    // 添加 Areas 支持
    manager.FeatureProviders.Add(new ControllerFeatureProvider());
});

// 添加 Areas 服务
builder.Services.Configure<RouteOptions>(options =>
{
    options.LowercaseUrls = true; // URL 小写
    options.LowercaseQueryStrings = false;
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
    options.Cookie.Name = "socialMedia.api.cookie";
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
        // 如果是默认分组，包含所有没有明确分组的接口
        if (docName == SwaggerGroups.Name)
        {
            return string.IsNullOrEmpty(apiDescription.GroupName);
        }
        // 其他分组只包含明确指定的接口
        return !string.IsNullOrEmpty(apiDescription.GroupName) && apiDescription.GroupName == docName;
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
        "Highever.SocialMedia.API.xml",
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
    // 添加控制器文档过滤器
    c.DocumentFilter<ControllerDocumentationFilter>();
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
    // 灵活的可空类型转换器
    options.JsonSerializerOptions.Converters.Add(new TextJsonExtension.FlexibleDateTimeNullableConverter());
    options.JsonSerializerOptions.Converters.Add(new TextJsonExtension.FlexibleLongNullableConverter());
    options.JsonSerializerOptions.Converters.Add(new TextJsonExtension.FlexibleIntNullableConverter());
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
    // 添加灵活的布尔值转换器
    options.JsonSerializerOptions.Converters.Add(new FlexibleBooleanConverter()); 
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

// 配置全局认证策略
builder.Services.AddAuthorization(options =>
{
    // 默认策略：需要认证
    options.DefaultPolicy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()  // 要求用户必须已认证
            .Build();

});

// 指定端口号
builder.WebHost.UseKestrel().UseUrls($"http://*:{AppSettingConifgHelper.ReadAppSettings("ADMIN_PORT") ?? "8326"}").UseIIS();

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

// 1. JWT认证中间件 - 验证Token有效性
app.UseAuthentication();

// 2. Token刷新中间件 - 检查是否需要刷新
app.UseMiddleware<TokenRefreshMiddleware>();

// 3. 数据权限中间件 - 加载用户数据权限上下文
app.UseMiddleware<DataPermissionMiddleware>();

// 4. 授权中间件 - 检查权限
app.UseAuthorization();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger(c =>
    {
        c.SerializeAsV2 = false;
    });
    app.ConfigureKnife4UI();
    
    // 添加详细日志
    app.Use(async (context, next) =>
    {
        if (context.Request.Path.StartsWithSegments("/swagger"))
        {
            Console.WriteLine($"Swagger请求: {context.Request.Path}");
        }
        await next();
    });
}

// CSP配置
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("Content-Security-Policy", "default-src 'self' http://erpauth.amz-marketing.com:10001;");
    await next();
});

// 配置路由映射
app.UseEndpoints(endpoints =>
{
    // Areas API 路由
    endpoints.MapControllerRoute(
        name: "areas-api",
        pattern: "api/{area:exists}/{controller}/{action=Index}/{id?}"
    );
    
    // 默认 API 路由
    endpoints.MapControllerRoute(
        name: "default-api", 
        pattern: "api/{controller}/{action=Index}/{id?}"
    );
    
    // 支持特性路由的控制器
    endpoints.MapControllers();
});

app.Run();
