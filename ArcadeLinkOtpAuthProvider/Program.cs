using Appwrite;
using Appwrite.Enums;
using Appwrite.Services;
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

// 初始化机厅数据库
Console.Out.WriteLine("Initializing database");
var db = new Databases(client);
var locations = await db.ListDocuments("alls", "locations");
var collections = await db.ListCollections("alls");

foreach (var locationDocument in locations.Documents)
{
    // 如果队列功能未启用，则跳过
    locationDocument.Data.TryGetValue("is_queue_enabled", out var isQueueEnabled);
    if (!(bool)(isQueueEnabled ?? false))
    {
        break;
    }
    
    // 检查数据库是否存在, 如果存在则清空
    var collectionName = locationDocument.Id;
    if (collections.Collections.Any(a=> a.Id == collectionName))
    {
        var documents = await db.ListDocuments("alls", collectionName);
        documents.Documents.ForEach(d => db.DeleteDocument("alls", d.CollectionId, d.Id));
        break;
    }
    
    // 创建队列数据库
    await db.CreateCollection("alls", collectionName, (string)locationDocument.Data["name"] );
    
    // 创建队列属性
    // int queueId
    // string name 32
    // string userId 64
    // bool passed
    await db.CreateIntegerAttribute("alls", collectionName, "queueId", true);
    await db.CreateStringAttribute("alls", collectionName, "name", 32, true);
    await db.CreateStringAttribute("alls", collectionName, "userId",  64, true);
    await db.CreateIntegerAttribute("alls", collectionName, "isRight", true);
    await db.CreateBooleanAttribute("alls", collectionName, "passed", true);
    
    // 创建队列索引
    await db.CreateIndex("alls", collectionName, "index_queueId", IndexType.Unique, ["queueId"]);
}

// 创建 Web 服务 (使用 Carter)
Console.Out.WriteLine("Creating web server");
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCarter();
builder.Services.AddSingleton(new NameDictionary()
{
    FirstNames = settings.FirstNames,
    LastName = settings.LastNames
});
builder.Services.AddSingleton(client);

var app = builder.Build();

app.MapCarter();
app.Run();