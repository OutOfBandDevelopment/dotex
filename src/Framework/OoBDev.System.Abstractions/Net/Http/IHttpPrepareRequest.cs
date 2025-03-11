using System.Net.Http;

namespace OoBDev.System.Net.Http;

public interface IHttpPrepareRequest
{
    void PrepareRequest(HttpClient client, HttpRequestMessage request, string url);
}
