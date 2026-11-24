using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Yarp.ReverseProxy.Model;

namespace OoBDev.AspNetCore.AuthGateway;

public static class ProxyPipelineExtensions
{
    /// <summary>The YARP route metadata key that names the gateway target.</summary>
    public const string TargetMetadataKey = "AuthGateway:Target";

    /// <summary>
    /// Adds the gateway step to the proxy pipeline: <c>app.MapReverseProxy(p => p.UseAuthGateway())</c>. On success the request carries
    /// <c>Authorization: Bearer</c> for the target and the API key header is removed; otherwise a problem response ends the request.
    /// </summary>
    public static IReverseProxyApplicationBuilder UseAuthGateway(this IReverseProxyApplicationBuilder pipeline)
    {
        pipeline.Use(async (context, next) =>
        {
            string? target = null;
            context.GetReverseProxyFeature().Route.Config.Metadata?.TryGetValue(TargetMetadataKey, out target);
            var authorizer = context.RequestServices.GetRequiredService<AuthGatewayAuthorizer>();
            var decision = await authorizer.AuthorizeAsync(context, target).ConfigureAwait(false);
            if (!decision.Allowed)
            {
                await Results.Problem(title: decision.Title, statusCode: decision.StatusCode).ExecuteAsync(context).ConfigureAwait(false);
                return;
            }

            var settings = context.RequestServices.GetRequiredService<IOptions<AuthGatewayOptions>>().Value;
            context.Request.Headers.Remove(settings.ApiKeyHeader);
            context.Request.Headers.Authorization = "Bearer " + decision.BearerToken;
            await next().ConfigureAwait(false);
        });
        return pipeline;
    }
}
