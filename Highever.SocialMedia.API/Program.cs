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
//Body����У������
builder.Services.AddControllers(options =>
{
    //ȫ��ģ���ֶι��������ѡ�SuppressModelStateInvalidFilter�����ó� True
    options.Filters.Add<ValidateInputAtrribute>();
});
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    //���������ValidateInputAtrributeȫ��ģ���ֶ���֤���·��� ApiBehaviorOptions  �Զ���һ��Ҫ����
    options.SuppressModelStateInvalidFilter = true;
});
#region ͬ�� I/O
//AllowSynchronousIO ����Ĭ��Kestrel�������Ƿ��������������Ӧʹ��ͬ�� I/O
//builder.Services.Configure<KestrelServerOptions>(x => x.AllowSynchronousIO = true);
//����IIS�������Ƿ��������������Ӧʹ��ͬ�� I/O
//.Configure<IISOptions>(x => x.AllowSynchronousIO = true);
#endregion 
//����HTTP������
builder.Services.AddHttpContextAccessor();
builder.Services.AddEndpointsApiExplorer();
//����ȫ��ѡ��
builder.Services.AddOptions();
#region  �ڴ滺�� / Session ���
//�����ڴ滺��
builder.Services.AddMemoryCache();
//����session
builder.Services.AddSession(options =>
{
    options.Cookie.Name = "lemna.api.cookie";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.IdleTimeout = TimeSpan.FromMinutes(30); //Ĭ����20����
});
//�����ڴ滺��洢session(ֻ�ʺϵ���ܹ�) 
builder.Services.AddDistributedMemoryCache();
#endregion
#region UI
builder.Services.AddSwaggerGen(c =>
{
    // ʹ�� GetSwaggerGroups ����ÿ������
    c.ConfigureSwaggerGroups();
    // �жϽӿڹ����ĸ�����
    c.DocInclusionPredicate((docName, apiDescription) =>
    {
        if (docName == SwaggerGroups.Name)
        {
            return string.IsNullOrEmpty(apiDescription.GroupName)
                   || string.IsNullOrWhiteSpace(apiDescription.GroupName);
        }
        return apiDescription.GroupName == docName;
    });
    // ����ӿ���ͬ�����
    c.CustomOperationIds(apiDesc =>
    {
        var controllerAction = apiDesc.ActionDescriptor as ControllerActionDescriptor;
        var parameters = string.Join("-", apiDesc.ParameterDescriptions.Select(p => p.Name));
        return $"{controllerAction.ControllerName}-{controllerAction.ActionName}-{parameters}";
    });
    // ��ȡ XML �ļ�·�������ݿ����ͷ���������
    var projectRoot = Directory.GetCurrentDirectory();
    var xmlPath = Path.Combine(projectRoot);
#if DEBUG
    // ������������ȡ�ϼ����Ŀ¼ 
    projectRoot = Path.Combine(Directory.GetParent(AppContext.BaseDirectory).Parent.Parent.Parent.Parent.FullName);// ���������ĸ�Ŀ¼ 
    xmlPath = Path.Combine(projectRoot, "XML");
#endif
    // �������� XML �ĵ�
    c.IncludeXmlComments(Path.Combine(xmlPath, "Highever.SocialMedia.API.xml"));
    c.IncludeXmlComments(Path.Combine(xmlPath, "Highever.SocialMedia.Application.Contracts.xml"));
    c.IncludeXmlComments(Path.Combine(xmlPath, "Highever.SocialMedia.Domain.xml"));
    c.IncludeXmlComments(Path.Combine(xmlPath, "Highever.SocialMedia.Common.xml"));
    c.SchemaFilter<EnumSchemaFilter>();
    //�Զ��� Authorization ����
    c.OperationFilter<AuthorizationParameterFilter>();
    //�Զ��� ��������
    c.DocumentFilter<HiddenApiFilter>();
    // ע���Զ����Ĭ�Ϸ���������
    c.OperationFilter<AutoHttpMethodOperationFilter>();
});
#endregion 
#region Nlog 
builder.Logging.ClearProviders();//ɾ����������������־������
builder.Logging.SetMinimumLevel(LogLevel.Trace);//ָ������
builder.Host.UseNLog();
//���ӿ���̨��־
builder.Logging.AddConsole();
#endregion
#region ����ע��
builder.Services.Register();
#endregion 
#region ����TextJson
builder.Services.AddControllers().AddJsonOptions(options =>
{
    //��ʽ������ʱ���ʽ
    options.JsonSerializerOptions.Converters.Add(new SystemTextJsonExtension.DatetimeJsonConverter());
    //����ѭ������
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    //��ʽ�����
    options.JsonSerializerOptions.WriteIndented = true;
    //���ݸ�ʽ����ĸСд
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    //ȡ��Unicode����
    options.JsonSerializerOptions.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);
    //���ݸ�ʽԭ�����
    //options.JsonSerializerOptions.PropertyNamingPolicy = null;
    //���Կ�ֵ
    //options.JsonSerializerOptions.IgnoreNullValues = false;
    //�����������(������ָ����ĩβ�Ķ��ţ������л�ʱ����ע�ͻ򶺺ţ����׳��쳣������������AllowTrailingCommas������Ĭ��Ϊfalse��)
    //options.JsonSerializerOptions.AllowTrailingCommas = false;
    //�����л����������������Ƿ�ʹ�ò����ִ�Сд�ıȽ�
    //options.JsonSerializerOptions.PropertyNameCaseInsensitive = false;
    options.JsonSerializerOptions.Converters.Add(new LongToStringConverter()); // �Զ��� long ת string Converter
    //���н��յ����ֶζ��ᱻ����Ϊ string ���ͣ�����ǰ�˷��͵������ֻ��ǲ���ֵ
    //options.JsonSerializerOptions.Converters.Add(new StringJsonConverter());
});
#endregion

#region ���� CORS ����
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        policy.WithOrigins("http://localhost:8068", "https://example.com", "http://127.0.0.1", "http://erpauth.amz-marketing.com:10001") // ��������Դ
              .AllowAnyMethod() // �������� HTTP ���� (GET, POST, PUT, DELETE ��)
              .AllowAnyHeader() // ������������ͷ
              .AllowCredentials(); // �����Ҫ֧�ִ�ƾ�ݵ����� (�� Cookies �� Authorization Header)
    });
    // ���Ҫ����������Դ������ȫ�������ڿ���ʱ��
    // options.AddPolicy("AllowAll", policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});
#endregion


//ָ���˿ں�
builder.WebHost.UseKestrel().UseUrls($"http://*:{AppSettingConifgHelper.ReadAppSettings("HOST_PORT")}").UseIIS();
var app = builder.Build();
//�쳣���ܵ���ʼ�ĵ�һ���м�������룩
app.UseCustomExceptionHandler();
//����λ��
ServiceLocator.SetLocatorProvider(app.Services);
//DI�����ķ����б�
app.RegisteredServicesPage(builder.Services);
//���ؾ�̬��Դ
app.UseStaticFiles();
//����·�� 
app.UseRouting();
// ʹ�� CORS �����м��
app.UseCors("AllowSpecificOrigins");
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.ConfigureKnife4UI();
}
//CSP����
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("Content-Security-Policy", "default-src 'self' http://erpauth.amz-marketing.com:10001;");
    await next();
});
app.UseAuthorization();

app.MapControllers();

app.Run();
