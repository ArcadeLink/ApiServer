using System.Text;
using Appwrite;
using Appwrite.Services;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using HttpResponse = ArcadeLinkOtpAuthProvider.Models.HttpResponse;

namespace ArcadeLinkOtpAuthProvider.Modules;

public class VideoModule (Client client) : ICarterModule
{
    private Client Client { get; } = client;

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/video/insert", InsertVideo);
    }

    private async Task<HttpResponse> InsertVideo(HttpContext context)
    {
        var locationId = context.Request.Query["locationId"];
        var locationKey = context.Request.Query["locationKey"];
        
        
        var db = new Databases(Client);
        var location = await db.GetDocument("alls", "locations", locationId!);

        if (location.Data["manage_key"] != locationKey)
        {
            return new HttpResponse()
            {
                StatusCode = 1,
                Message = "Invalid location key"
            };
        }
        
        var urlUnsafe = context.Request.Query["url"];
        var urlBytes = Convert.FromBase64String(urlUnsafe.ToString().Replace('-', '+').Replace('_', '/'));
        var url = Encoding.UTF8.GetString(urlBytes);
        var userId = context.Request.Query["userId"];
        var doc = await db.CreateDocument("alls", "videos", ID.Unique(),
            new Dictionary<string,object>()
            {
                { "video_length", 0 },
                { "url", url },
                { "locations", location.Id },
                { "user_id", userId.ToString() },
            });

        return new HttpResponse()
        {
            StatusCode = 0,
            Message = "Success",
            Data = doc.Id
        };

    }
}