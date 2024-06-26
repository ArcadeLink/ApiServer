using System.Globalization;
using Appwrite;
using Appwrite.Services;
using ArcadeLinkOtpAuthProvider.Services;
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
        app.MapGet("/safeSetName", SafeSetName);
        app.MapGet("/getRandomName", GetRandomName);
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
    
    private async Task<bool> VerifySession(string userId, string sessionId)
    {
        // 创建一个新的 Appwrite 客户端
        var account = new Users(Client);
        
        // 获取当前会话
        try
        {
            var list = await account.ListSessions(userId);
            if (list.Sessions.All(a => a.Id != sessionId))
            {
                return false;
            }

            return true;
        }
        catch (Exception e)
        {
            return false;
        }
    }

    private async Task<HttpResponse> SafeSetName(HttpRequest request)
    {
        var userId = request.Query.AsMultiple<string>("userId").First();
        var sessionId = request.Query.AsMultiple<string>("sessionId").First();
        var name = request.Query.AsMultiple<string>("name").First();
        
        var isVerified = await VerifySession(userId, sessionId);
        
        // 验证会话
        if (!isVerified)
        {
            return new HttpResponse()
            {
                StatusCode = -1,
                Message = "Invalid session",
            };
        }
        
        // 合规验证
        var result = BaiduTextVerifyService.Verify(name);
        if (result.conclusionType != 1)
        {
            return new HttpResponse()
            {
                StatusCode = -1,
                Message = result.conclusion,
            };
        }
        
        // 更新用户的名字
        var account = new Users(Client);
        try
        {
            await account.UpdateName(userId, name);
        }
        catch (Exception e)
        {
            return new HttpResponse()
            {
                StatusCode = -1,
                Message = e.Message
            };
        }
        
        return new HttpResponse()
        {
            StatusCode = 0,
            Message = "Success",
            Data = new
            {
                name
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
        var isVerified = await VerifySession(userId, sessionId);

        if (!isVerified)
        {
            return new HttpResponse()
            {
                StatusCode = -1,
                Message = "Invalid session",
            };
        }
        
        // 更新用户的密钥
        try
        {
            await account.UpdatePrefs(
                userId,
                prefs: new Dictionary<string, string>
                {
                    { "otpSecret", base32String },
                    { "lastUpdateTime" , DateTime.Now.ToString(CultureInfo.InvariantCulture)}
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