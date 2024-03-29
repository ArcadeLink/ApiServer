using System.Text.Json.Serialization;

namespace ArcadeLinkOtpAuthProvider.Models;

public class AppwriteResponseBody
{
    public class User
    {
        [JsonPropertyName("$id")]
        public string Id { get; set; }
    }
}