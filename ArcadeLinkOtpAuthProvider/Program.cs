using Appwrite;
using Appwrite.Enums;
using ArcadeLinkOtpAuthProvider;
using Carter;
using Config.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

var settings = new ConfigurationBuilder<IApiConfig>()
    .UseAppConfig()
    .Build();

Console.Out.WriteLine("Project ID: " + settings.ProjectId);

// 配置 Appwrite 服务端 SDK
var client = new Client()
    .SetEndpoint(settings.Endpoint)  
    .SetProject(settings.ProjectId)
    .SetKey(settings.ProjectKey);

// 创建 Web 服务 (使用 Carter)
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCarter();
builder.Services.AddSingleton(new NameProvider()
{
    FirstNames = settings.FirstNames,
    LastName = settings.LastNames
});
builder.Services.AddSingleton(client);

var app = builder.Build();

app.MapCarter();
app.Run();