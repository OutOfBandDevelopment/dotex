using OoBDev.AspNetCore.Extensions.Middleware;
using Microsoft.AspNetCore.Builder;

namespace OoBDev.AspNetCore.Extensions;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder AddOoBDevAspNetCoreMiddleware(this IApplicationBuilder app)
    {
        app.UseMiddleware<CorrelationInfoMiddleware>();
        app.UseMiddleware<AuditLoggingMiddleware>();
        app.UseMiddleware<OoBDevInternalMiddleware>();

        return app;
    }
}
