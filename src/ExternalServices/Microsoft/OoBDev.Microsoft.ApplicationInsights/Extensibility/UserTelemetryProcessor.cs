using OoBDev.System.Security.Claims;
using Microsoft.AspNetCore.Http;
using OpenTelemetry;
using OpenTelemetry.Logs;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace OoBDev.Microsoft.ApplicationInsights.Extensibility;

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

/// <summary>
/// OpenTelemetry processor that adds user information from the HTTP context to log records.
/// </summary>
public class UserLogProcessor : BaseProcessor<LogRecord>
{
    private readonly IHttpContextAccessor _accessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserLogProcessor"/> class.
    /// </summary>
    /// <param name="accessor">The accessor for accessing the current HTTP context.</param>
    public UserLogProcessor(IHttpContextAccessor accessor) => _accessor = accessor;

    /// <summary>
    /// Adds the user claims to the log record attributes.
    /// </summary>
    /// <param name="data">The log record being emitted.</param>
    public override void OnEnd(LogRecord data)
    {
        var extra = UserClaims.Read(_accessor).ToList();
        if (extra.Count == 0) return;

        var attributes = new List<KeyValuePair<string, object?>>(data.Attributes ?? []);
        attributes.AddRange(extra.Select(c => new KeyValuePair<string, object?>(c.Key, c.Value)));
        data.Attributes = attributes;
    }
}

internal static class UserClaims
{
    public static IEnumerable<(string Key, string Value)> Read(IHttpContextAccessor accessor)
    {
        var user = accessor.HttpContext?.User;
        if (user is null) yield break;

        var objectId = user.Claims.FirstOrDefault(c => c.Type == CommonClaims.ObjectId)?.Value;
        if (!string.IsNullOrWhiteSpace(objectId))
            yield return ($"Claim-{CommonClaims.ObjectId}", objectId);

        var userId = user.Claims.FirstOrDefault(c => c.Type == CommonClaims.UserId)?.Value;
        if (!string.IsNullOrWhiteSpace(userId))
            yield return ($"Claim-{CommonClaims.UserId}", userId);
    }
}
