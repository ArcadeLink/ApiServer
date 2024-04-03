using ArcadeLinkOtpAuthProvider.Models;

namespace ArcadeLinkOtpAuthProvider.Services;

using System;
using System.IO;
using RestSharp;//依赖版本106.15.0 https://www.nuget.org/packages/RestSharp/106.15.0
using Newtonsoft.Json; //https://www.nuget.org/packages/Newtonsoft.Json

public class BaiduTextVerifyService
{
    const string API_KEY = "0DGHNKYpzrFhzpJLnviVLKSd";
    const string SECRET_KEY = "ZJvO0N6qHx8GW5LbIuCenbYHEE4VqcA7";

    public static BaiduServiceResponse Verify(string text) {
        var client = new RestClient($"https://aip.baidubce.com/rest/2.0/solution/v1/text_censor/v2/user_defined?access_token={GetAccessToken()}")
            {
                Timeout = -1
            };
        
        var request = new RestRequest(Method.POST);
        request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
        request.AddHeader("Accept", "application/json");
        request.AddParameter("text", text);
        
        var response = client.Execute(request);
        return JsonConvert.DeserializeObject<BaiduServiceResponse>(response.Content)!;
    }
    
    /**
    * 使用 AK，SK 生成鉴权签名（Access Token）
    * @return 鉴权签名信息（Access Token）
    */
    private static string GetAccessToken() {
        var client = new RestClient($"https://aip.baidubce.com/oauth/2.0/token")
        {
            Timeout = -1
        };
        
        var request = new RestRequest(Method.POST);
        request.AddParameter("grant_type", "client_credentials");
        request.AddParameter("client_id", API_KEY);
        request.AddParameter("client_secret", SECRET_KEY);
        var response = client.Execute(request);
        Console.WriteLine(response.Content);
        var result = JsonConvert.DeserializeObject<dynamic>(response.Content);
        return result!.access_token.ToString();
    }

}