using Appwrite;
using Appwrite.Services;
using Carter;
using Carter.Request;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using HttpResponse = ArcadeLinkOtpAuthProvider.Models.HttpResponse;

namespace ArcadeLinkOtpAuthProvider.Modules;

public class QueueModule(Client client) : ICarterModule
{
    private Client Client { get; } = client;
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/queue/current", GetCurrentQueue);
        app.MapGet("/queue/insert", InsertToQueue);
        app.MapGet("/queue/pass", PassInQueue);
    }
    
    
    private async Task<HttpResponse> PassInQueue(HttpRequest request)
    {
        try
        {
            // 获取参数
            var qth = request.Query.AsMultiple<string>("qth").First();
            var userId = request.Query.AsMultiple<string>("userId").First();
            var queueId = request.Query.AsMultiple<string>("queueId").First();
            
            // 获取队列
            var databases = new Databases(Client);
            var queue = await databases.ListDocuments("alls", qth);
            var queueDocument = queue.Documents.FirstOrDefault(a =>
                (string)a.Data["userId"] == userId  // 用户ID
                && a.Data["queueId"].ToString()!.Equals(queueId)); // 队列ID
            
            // 检测是否在队列中
            if (queueDocument == null)
            {
                return new HttpResponse()
                {
                    StatusCode = -1,
                    Message = "Not in queue"
                };
            }

            // 检测是否已经通过
            if ((bool)queueDocument.Data["passed"])
            {
                return new HttpResponse()
                {
                    StatusCode = -1,
                    Message = "Already passed"
                };
            }
            
            // 更新队列
            await databases.UpdateDocument("alls", qth, queueDocument.Id, new Dictionary<string, object>
            {
                { "passed", true }
            });
            
            return new HttpResponse()
            {
                StatusCode = 0,
                Message = "Success"
            };
        }
        catch (Exception e)
        {
            Console.Out.WriteLine(e);
            return new HttpResponse()
            {
                StatusCode = -1,
                Message = e.Message
            };
        }
    }
    
    private async Task<HttpResponse> InsertToQueue(HttpRequest request)
    {
        try
        {
            var qth = request.Query.AsMultiple<string>("qth").First();
            var userId = request.Query.AsMultiple<string>("userId").First();
            var isRight = request.Query.AsMultiple<int>("isRight").FirstOrDefault();
            
            var users = new Users(Client);
            var user = await users.Get(userId);
            
            // 获取队列
            var databases = new Databases(Client);
            var queue = await databases.ListDocuments("alls", qth);
            
            // 检测重复
            if (queue.Documents.Any(a => 
                    (string)a.Data["userId"] == userId // 用户ID是否有重复
                    && !(bool)a.Data["passed"]))    // 是否已经通过
            {
                return new HttpResponse()
                {
                    StatusCode = -1,
                    Message = "Already in queue"
                };
            }
            
            // 获取队列长度
            var queueLength = queue.Documents.Count;
            
            // 插入队列
            await databases.CreateDocument("alls", qth, ID.Unique(),new Dictionary<string, object>
            {
                { "queueId", queueLength + 1 },
                { "name", user.Name },
                { "userId", userId },
                { "isRight", isRight},
                { "passed", false }
            });

            // 返回队列长度
            return new HttpResponse()
            {
                StatusCode = 0,
                Message = "Success",
                Data = new
                {
                    queueId = queueLength + 1
                }
            };
        }
        catch (Exception e)
        {
            return new HttpResponse()
            {
                StatusCode = -1,
                Message = e.Message
            };
        }
    }
    
    private async Task<HttpResponse> GetCurrentQueue(HttpRequest request)
    {
        try
        {
            // 是否显示已经经过的 1 显示 0 不显示
            var showPassed = request.Query.AsMultiple<int>("showPassed").FirstOrDefault() == 1;
            
            var qthId = request.Query.AsMultiple<string>("qth").First();
            var databases = new Databases(Client);

            var queue = showPassed ?
                await databases.ListDocuments("alls", qthId) : // 显示所有
                await databases.ListDocuments("alls", qthId, [
                        Query.NotEqual("passed", true)
                    ]); // 不显示已经通过的
            
            return new HttpResponse()
            {
                StatusCode = 0,
                Message = "Success",
                Data = queue.Documents
            };
        }
        catch (Exception e)
        {
            return new HttpResponse()
            {
                StatusCode = -1,
                Message = e.Message
            };
        }
    }
}