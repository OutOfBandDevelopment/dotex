using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace OoBDev.System.Net.Http;

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
