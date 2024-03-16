using Config.Net;

public interface IApiConfig
{
    string ProjectId { get; }
    string ProjectKey { get; }
    string Endpoint { get; }
}