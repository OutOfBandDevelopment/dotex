using OoBDev.System.Security.Claims;
using Microsoft.AspNetCore.Http;
using OpenTelemetry;
using OpenTelemetry.Logs;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace OoBDev.OpenTelemetry;

/// <summary>
/// OpenTelemetry processor that extracts user information from the HTTP context and adds it to spans.
/// </summary>
public class UserTelemetryProcessor : BaseProcessor<Activity>
{
    private readonly IHttpContextAccessor _accessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserTelemetryProcessor"/> class.
    /// </summary>
    /// <param name="accessor">The accessor for accessing the current HTTP context.</param>
    public UserTelemetryProcessor(IHttpContextAccessor accessor) => _accessor = accessor;

    /// <summary>
    /// Adds the user claims to the span tags when the span starts.
    /// </summary>
    /// <param name="data">The span being started.</param>
    public override void OnStart(Activity data)
    {
        foreach (var (key, value) in UserClaims.Read(_accessor))
            data.SetTag(key, value);
    }
}

