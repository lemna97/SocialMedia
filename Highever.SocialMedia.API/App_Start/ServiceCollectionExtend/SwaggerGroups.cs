using Highever.SocialMedia.Common;
using IGeekFan.AspNetCore.Knife4jUI;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Highever.SocialMedia.API
{
    public static class SwaggerGroups
    {
        public const string Name = "Default";
        public const string Title = "默认组";
        public const string Url = "https://default.example.com/v1";
        public const string UrlDescription = "默认地址";
        public static void ConfigureSwaggerGroups(this SwaggerGenOptions c)
        {
            var registeredGroups = new HashSet<string>();

            // 遍历 SwaggerApiGroup 枚举值
            typeof(SwaggerApiGroup).GetFields().Skip(1).ToList().ForEach(f =>
            {
                var info = f.GetCustomAttributes(typeof(GroupInfoAttribute), false)
                            .OfType<GroupInfoAttribute>()
                            .FirstOrDefault();

                var groupName = f.Name;
                if (!registeredGroups.Contains(groupName))
                {
                    registeredGroups.Add(groupName);

                    if (info != null)
                    {
                        c.SwaggerDoc(groupName, new OpenApiInfo
                        {
                            Title = info.Title ?? groupName,
                            Version = info.Version ?? "v1",
                            Description = info.Description
                        });

                        c.AddServer(new OpenApiServer
                        {
                            Url = info.Url,
                            Description = info.UrlDescription
                        });
                    }
                }
            });

            // 添加默认分组："Default" - 修复：取消注释并确保注册
            if (!registeredGroups.Contains(Name))
            {
                c.SwaggerDoc(Name, new OpenApiInfo
                {
                    Title = Title,
                    Version = "v1",
                    Description = "未分组的接口"
                });

                c.AddServer(new OpenApiServer
                {
                    Url = Url,
                    Description = UrlDescription
                });
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="app"></param>
        public static void ConfigureKnife4UI(this IApplicationBuilder app)
        {
            app.UseKnife4UI(c =>
            {
                c.ShowExtensions();
                c.DocExpansion(DocExpansion.None);

                // 获取所有已注册的分组
                var registeredGroups = typeof(SwaggerApiGroup).GetFields()
                                      .Skip(1)
                                      .Select(f =>
                                      {
                                          var info = f.GetCustomAttributes(typeof(GroupInfoAttribute), false)
                                                      .OfType<GroupInfoAttribute>()
                                                      .FirstOrDefault();
                                          return new
                                          {
                                              Name = f.Name,
                                              Title = info?.Title ?? f.Name
                                          };
                                      }).ToList();

                // 动态添加 endpoints
                registeredGroups.ForEach(group =>
                {
                    c.SwaggerEndpoint($"/swagger/{group.Name}/swagger.json", group.Title);
                });

                // 添加默认分组 endpoint - 确保最后添加
                c.SwaggerEndpoint($"/swagger/{Name}/swagger.json", Title);

                c.RoutePrefix = string.Empty;
            });
        }

    }
}
