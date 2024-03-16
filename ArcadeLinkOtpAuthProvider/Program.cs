using System.Configuration;
using Appwrite;
using Appwrite.Services;
using Appwrite.Models;
using Carter;
using Config.Net;
using Microsoft.AspNetCore.Builder;

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

var app = builder.Build();

app.MapCarter();
// app.Run();