using System;
using System.IO;
using System.Text;
using System.Text.Json;

namespace OoBDev.System.Sandbox;

/// <summary>
/// A running plugin whose file and network requests are answered by the host. The sandbox gives the plugin no direct access at all; every
/// <c>broker.*</c> request it sends is checked twice, against the plugin's grant (<see cref="GrantBroker"/>) and against the rights of the
/// caller's <see cref="RequestContext"/> (<see cref="ContextBroker"/>), and every decision is audited.
/// </summary>
public sealed class BrokeredPlugin : IDisposable
{
    private readonly ISandboxedPlugin _plugin;
    private readonly GrantBroker _grants;
    private readonly ContextBroker _contexts;
    private readonly IDisposable? _hold;
    private RequestContext? _current;

    internal BrokeredPlugin(string pluginId, ISandboxedPlugin plugin, GrantBroker grants, ContextBroker contexts, IDisposable? hold)
    {
        PluginId = pluginId;
        _plugin = plugin;
        _grants = grants;
        _contexts = contexts;
        _hold = hold;
    }

    /// <summary>The manifest id.</summary>
    public string PluginId { get; }

    /// <summary>Waits for the ready event the plugin sends when it starts.</summary>
    public ReceivedFrame WaitReady() => _plugin.Receive();

    /// <summary>
    /// Runs one request for <paramref name="context"/>: the plugin first receives the context, then the request, and may ask the host for
    /// files and network on the way. Returns the plugin's response payload. One call at a time per instance.
    /// </summary>
    public string Call(string requestJson, RequestContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        _contexts.Issue(context);
        _current = context;
        try
        {
            _plugin.Send(FrameType.Event, "context.set", context.ToJson());
            var sequence = _plugin.Send(FrameType.Request, "run", requestJson);
            while (true)
            {
                var frame = _plugin.Receive();
                if (frame.Type == FrameType.Response && frame.Ack == sequence)
                {
                    return frame.Text;
                }

                if (frame.Type == FrameType.Request && frame.Topic.StartsWith("broker.", StringComparison.Ordinal))
                {
                    _plugin.Reply(frame, FrameType.Response, Serve(frame));
                }
            }
        }
        finally
        {
            _current = null;
            _contexts.Release(context.Id);
        }
    }

    /// <summary>Asks the plugin to end, then stops it if it does not.</summary>
    public void Dispose()
    {
        try
        {
            _plugin.Send(FrameType.Shutdown, "lifecycle.shutdown", "{}");
            _plugin.WaitForExit(2000);
        }
        catch (Exception ex) when (ex is IOException or ObjectDisposedException or InvalidOperationException)
        {
        }

        _plugin.Dispose();
        _hold?.Dispose();
    }

    private static string Denied(string reason) => Json(new { allowed = false, reason });

    private static string Json<T>(T value) => JsonSerializer.Serialize(value);

    private string Serve(ReceivedFrame request)
    {
        using var document = JsonDocument.Parse(request.Payload);
        var root = document.RootElement;
        var context = _current;
        switch (request.Topic)
        {
            case "broker.file.read":
            {
                var path = root.GetProperty("path").GetString() ?? string.Empty;
                if (Refuse(context, BrokerRights.FileRead) is { } refusal)
                {
                    return Denied(refusal);
                }

                var decision = _grants.CheckFile(PluginId, path, write: false);
                return decision.Allowed ? Json(new { allowed = true, text = File.ReadAllText(path) }) : Denied(decision.Reason);
            }

            case "broker.file.write":
            {
                var path = root.GetProperty("path").GetString() ?? string.Empty;
                if (Refuse(context, BrokerRights.FileWrite) is { } refusal)
                {
                    return Denied(refusal);
                }

                var decision = _grants.CheckFile(PluginId, path, write: true);
                if (!decision.Allowed)
                {
                    return Denied(decision.Reason);
                }

                File.WriteAllText(path, root.GetProperty("text").GetString() ?? string.Empty);
                return Json(new { allowed = true });
            }

            case "broker.fetch":
            {
                if (Refuse(context, BrokerRights.Fetch) is { } refusal)
                {
                    return Denied(refusal);
                }

                var uri = new Uri(root.GetProperty("url").GetString() ?? string.Empty);
                var fetch = _grants.FetchAsync(PluginId, uri).GetAwaiter().GetResult();
                return fetch.Decision.Allowed
                    ? Json(new { allowed = true, status = fetch.Status, text = Encoding.UTF8.GetString(fetch.Body) })
                    : Denied(fetch.Decision.Reason);
            }

            default:
                return Denied("unknown broker topic");
        }
    }

    private string? Refuse(RequestContext? context, string right)
    {
        if (context is null)
        {
            return "no request context";
        }

        var check = _contexts.Check(PluginId, context.Id, right);
        return check == ContextRefusal.None ? null : "context: " + check;
    }
}
