using Appwrite;
using Appwrite.Services;
using ArcadeLinkOtpAuthProvider.Models;
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

        app.MapPost("/initializeUserInfomation", InitializeUserInfomation);
    }
    
    private string GetRandomName()
    {
        var firstNames = NameDictionary.FirstNames.Split(",");
        var lastNames = NameDictionary.LastName.Split(",");
        var random = new Random();
        var firstName = firstNames[random.Next(firstNames.Length)].Replace(" ", string.Empty);
        var lastName = lastNames[random.Next(lastNames.Length)].Replace(" ", string.Empty);
        return firstName + lastName;
    }

    private async Task<HttpResponse> InitializeUserInfomation(HttpRequest request)
    {
        var signature =
            "5c73493ad4a9be173995cac2f4580acf48487496cda16fbaa650a494717dced5c025a1f0fc281d2f70b8899587ea3b485630fc6a271febb766892078c42f6a82";
        if (!request.Headers["X-Appwrite-Webhook-Signature"].Equals(signature))
        {
            return new HttpResponse()
            {
                StatusCode = -1,
                Message = "Invalid signature",
                Data = "乱几把发请求还要你妈 Data"
            };
        }
        
        var users = new Users(Client);
        
        var requestBody = await request.ReadFromJsonAsync<AppwriteResponseBody.User>();
        var userId = requestBody!.Id;

        // 更新用户名
        var name = GetRandomName();
        await users.UpdateName(userId, name);
        
        // 更新密钥
        
        // 生成一个随机的密钥
        var key = KeyGeneration.GenerateRandomKey(20);
        var base32String = Base32Encoding.ToString(key);
        await users.UpdatePrefs(
            userId,
            prefs: new Dictionary<string, string>
            {
                { "otpSecret", base32String }
            });
        
        // 返回新的密钥
        var response = new HttpResponse()
        {
            StatusCode = 0,
            Message = "Success",
            Data = new
            {
                secret = Base32Encoding.ToString(KeyGeneration.GenerateRandomKey(20))
            }
        };
        return response;
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