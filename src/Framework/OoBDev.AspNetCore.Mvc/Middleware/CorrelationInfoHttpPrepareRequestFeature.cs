using OoBDev.System.Accessors;
using OoBDev.System.Net.Http;
using System;
using System.Net.Http;

namespace OoBDev.AspNetCore.Mvc.Middleware;
public class CorrelationInfoHttpPrepareRequestFeature : IHttpPrepareRequestFeature
{
    private readonly IAccessor<CorrelationInfo> _accessor;
    public CorrelationInfoHttpPrepareRequestFeature(
        IAccessor<CorrelationInfo> accessor
        )
    {
        _accessor = accessor;
    }

    public void PrepareRequest(HttpClient client, HttpRequestMessage request, string url)
    {
        var correlationId = _accessor.Value?.CorrelationId;
        var requestId = Guid.NewGuid().ToString();

        if (!string.IsNullOrWhiteSpace(correlationId))
        {
            request.Headers.TryAddWithoutValidation(DefinedHttpHeaders.CorrelationIdHeader, correlationId);
        }

        request.Headers.TryAddWithoutValidation(DefinedHttpHeaders.RequestIdHeader, requestId);
    }
}
