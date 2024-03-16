namespace ArcadeLinkOtpAuthProvider.Models;

public class HttpResponse
{
    public int StatusCode { get; set; }
    public string Message { get; set; }
    public object Data { get; set; }
}