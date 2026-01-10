using System.Net.Http;

namespace OoBDev.Common.Net.Http;

public interface IHttpPrepareRequest
{
    void PrepareRequest(HttpClient client, HttpRequestMessage request, string url);
}
