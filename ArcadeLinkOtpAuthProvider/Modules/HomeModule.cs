using Appwrite;
using Appwrite.Services;
using Carter;
using Carter.Request;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using OtpNet;
using HttpResponse = ArcadeLinkOtpAuthProvider.Models.HttpResponse;

namespace ArcadeLinkOtpAuthProvider.Modules;

// ReSharper disable once UnusedType.Global
public class HomeModule(Client client, NameDictionary dictionary) : ICarterModule
{
    private Client Client { get; } = client;
    private NameDictionary NameDictionary { get; } = dictionary;

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/", () => "You're 59+20, 73!");
        
        app.MapGet("/refreshSecret", RefreshSecret);
        app.MapGet("/getRandomName", GetRandomName);
        
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
                (string)a.Data["userId"] == userId && a.Data["queueId"].ToString()!.Equals(queueId));
            
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
            
            var users = new Users(Client);
            var user = await users.Get(userId);
            
            // 获取队列
            var databases = new Databases(Client);
            var queue = await databases.ListDocuments("alls", qth);
            
            // 检测重复
            if (queue.Documents.Any(a => (string)a.Data["userId"] == userId && !(bool)a.Data["passed"]))
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

    private HttpResponse GetRandomName(HttpRequest request)
    {
        var firstNames = NameDictionary.FirstNames.Split(",");
        var lastNames = NameDictionary.LastName.Split(",");
        var random = new Random();
        var firstName = firstNames[random.Next(firstNames.Length)].Replace(" ", string.Empty);
        var lastName = lastNames[random.Next(lastNames.Length)].Replace(" ", string.Empty);
        return new HttpResponse()
        {
            StatusCode = 0,
            Message = "Success",
            Data = new
            {
                name = firstName + lastName
            }
        };
    }
    
    private async Task<HttpResponse> RefreshSecret(HttpRequest request)
    {
        // 生成一个随机的密钥
        var key = KeyGeneration.GenerateRandomKey(20);
        var base32String = Base32Encoding.ToString(key);
        
        // 创建一个新的 Appwrite 客户端
        var account = new Users(Client);
        
        // 获取参数
        var userId = request.Query.AsMultiple<string>("userId").First();
        var sessionId = request.Query.AsMultiple<string>("sessionId").First();
        
        // 获取当前会话
        try
        {
            var list = await account.ListSessions(userId);
            if (list.Sessions.All(a => a.Id != sessionId))
            {
                return new HttpResponse()
                {
                    StatusCode = -1,
                    Message = "Invalid session"
                };
            }
        }
        catch (Exception e)
        {
            return new HttpResponse()
            {
                StatusCode = -1,
                Message = e.Message
            };
        }
        
        // 更新用户的密钥
        try
        {
            await account.UpdatePrefs(
                userId,
                prefs: new Dictionary<string, string>
                {
                    { "otpSecret", base32String }
                });
        }
        catch (Exception e)
        {
            return new HttpResponse()
            {
                StatusCode = -1,
                Message = e.Message
            };
        }

        // 返回新的密钥
        return new HttpResponse()
        {
            StatusCode = 0,
            Message = "Success",
            Data = new
            {
                secret = base32String
            }
        };
    }
    
}