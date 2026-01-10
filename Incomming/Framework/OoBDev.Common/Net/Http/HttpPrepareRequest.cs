using System.Collections.Generic;
using System.Net.Http;

namespace OoBDev.Common.Net.Http;

public class HttpPrepareRequest : IHttpPrepareRequest
{
    private readonly IEnumerable<IHttpPrepareRequestFeature> _features;
    public HttpPrepareRequest(
        IEnumerable<IHttpPrepareRequestFeature> features
        )
    {
        _features = features;
    }

    public void PrepareRequest(HttpClient client, HttpRequestMessage request, string url)
    {
        foreach (var feature in _features)
        {
            feature.PrepareRequest(client, request, url);
        }
    }
}
