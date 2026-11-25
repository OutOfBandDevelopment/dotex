using OoBDev.System.Security;
using OpenTelemetry;
using OpenTelemetry.Logs;
using System.Collections.Generic;
using System.Linq;

namespace OoBDev.Telemetry;

/// <summary>
/// OpenTelemetry processor that adds user information from the current user accessor to log records.
/// </summary>
public class UserLogProcessor : BaseProcessor<LogRecord>
{
    private readonly ICurrentUserAccessor _accessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserLogProcessor"/> class.
    /// </summary>
    /// <param name="accessor">The accessor for the current user.</param>
    public UserLogProcessor(ICurrentUserAccessor accessor) => _accessor = accessor;

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
