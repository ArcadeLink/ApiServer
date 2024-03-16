using Carter;
using Carter.Request;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using HttpResponse = ArcadeLinkOtpAuthProvider.Models.HttpResponse;

namespace ArcadeLinkOtpAuthProvider.Modules;

public class HomeModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/", () => "You're 59+20, 73!");
        
        app.MapGet("/getSecret", GetSecret);
    }
    
    private static HttpResponse GetSecret(HttpRequest request)
    {
        return new HttpResponse();
    }
}