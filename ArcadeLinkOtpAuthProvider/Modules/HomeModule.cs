using System.Security.Cryptography;
using System.Text;
using Appwrite;
using Appwrite.Models;
using Appwrite.Services;
using Carter;
using Carter.Request;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using OtpNet;
using HttpResponse = ArcadeLinkOtpAuthProvider.Models.HttpResponse;

namespace ArcadeLinkOtpAuthProvider.Modules;

public class HomeModule(Client client, NameProvider provider) : ICarterModule
{
    private Client Client { get; } = client;
    private NameProvider NameProvider { get; } = provider;

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/", () => "You're 59+20, 73!");
        
        app.MapGet("/refreshSecret", RefreshSecret);
        app.MapGet("/getRandomName", GetRandomName);
    }

    private HttpResponse GetRandomName(HttpRequest request)
    {
        var firstNames = NameProvider.FirstNames.Split(",");
        var lastNames = NameProvider.LastName.Split(",");
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
        var account = new Users(client);
        
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
            var updatePrefs = await account.UpdatePrefs(
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