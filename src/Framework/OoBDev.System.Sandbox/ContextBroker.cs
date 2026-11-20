using System;
using System.Collections.Concurrent;
using System.Linq;

namespace OoBDev.Spike.PluginSandbox;

/// <summary>
/// The host's copy of the contexts it issued. A plugin presents only an id; the broker decides from its own stored copy, so a plugin
/// that edits what it was given gains nothing.
/// </summary>
public sealed class ContextBroker(TimeProvider time)
{
    private readonly ConcurrentDictionary<string, RequestContext> _issued = new(StringComparer.Ordinal);

    /// <summary>Stores a context for the duration of its request and returns it.</summary>
    public RequestContext Issue(RequestContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        _issued[context.Id] = context;
        return context;
    }

    /// <summary>Forgets a context once its request has finished.</summary>
    public void Release(string id) => _issued.TryRemove(id, out _);

    /// <summary>Checks that <paramref name="pluginId"/> may act with <paramref name="id"/>, optionally for an action that needs <paramref name="requiredRight"/>.</summary>
    public ContextRefusal Check(string pluginId, string id, string? requiredRight = null)
    {
        if (!_issued.TryGetValue(id, out var context))
        {
            return ContextRefusal.Unknown;
        }

        if (!string.Equals(context.Audience, pluginId, StringComparison.Ordinal))
        {
            return ContextRefusal.WrongPlugin;
        }

        if (!context.IsValidAt(time.GetUtcNow()))
        {
            return ContextRefusal.Expired;
        }

        return requiredRight is not null && !context.Rights.Any(r => string.Equals(r, requiredRight, StringComparison.Ordinal))
            ? ContextRefusal.MissingRight
            : ContextRefusal.None;
    }
}
