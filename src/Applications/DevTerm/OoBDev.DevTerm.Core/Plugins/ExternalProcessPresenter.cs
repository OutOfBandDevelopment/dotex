using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Buffers;
using System.Text.Json;

namespace OoBDev.DevTerm;

/// <summary>
/// A presenter implemented by a separate program in any language, run as a child process and spoken to in JSON lines on
/// its stdin/stdout. The child cannot crash or stall the host:
/// every request waits at most the reply timeout, and a dead, silent or malformed child is marked <see cref="Faulted"/>
/// and renders nothing from then on (<see cref="FaultReason"/> says why). Requests are one line each:
/// a hello request answered by a hello reply carrying the plugin's name and protocol number, and a render request
/// carrying the received bytes as hex, answered by an output reply carrying a list of lines.
/// </summary>
public sealed class ExternalProcessPresenter : IPresenter, IAsyncDisposable
{
    /// <summary>The wire protocol version a plugin must answer its hello with.</summary>
    public const int ProtocolVersion = 1;

    private readonly PluginChild _process;
    private readonly TimeSpan _replyTimeout;
    private readonly Lock _gate = new();

    private ExternalProcessPresenter(PluginChild process, string name, TimeSpan replyTimeout)
    {
        _process = process;
        Name = name;
        _replyTimeout = replyTimeout;
    }

    public string Name { get; }

    public bool Faulted { get; private set; }

    public string? FaultReason { get; private set; }

    /// <summary>Starts the program and checks its hello; throws if it won't start, stays silent or speaks another protocol.</summary>
    /// <exception cref="InvalidOperationException">The plugin did not complete the handshake.</exception>
    public static async Task<ExternalProcessPresenter> StartAsync(string fileName, IEnumerable<string> arguments, TimeSpan replyTimeout, CancellationToken cancellationToken = default, PluginSandbox? sandbox = null)
    {
        var process = PluginChild.Start(fileName, arguments, sandbox);

        try
        {
            await process.Input.WriteLineAsync("{\"type\":\"hello\"}".AsMemory(), cancellationToken).ConfigureAwait(false);
            await process.Input.FlushAsync(cancellationToken).ConfigureAwait(false);
            var line = await process.Output.ReadLineAsync(cancellationToken).AsTask().WaitAsync(replyTimeout, cancellationToken).ConfigureAwait(false);
            using var doc = JsonDocument.Parse(line ?? throw new InvalidOperationException("The plugin closed without answering hello."));
            var root = doc.RootElement;
            if (root.GetProperty("type").GetString() != "hello" || root.GetProperty("protocol").GetInt32() != ProtocolVersion)
            {
                throw new InvalidOperationException($"The plugin does not speak protocol {ProtocolVersion}.");
            }

            return new ExternalProcessPresenter(process, root.GetProperty("name").GetString() ?? "plugin", replyTimeout);
        }
        catch (Exception ex) when (ex is TimeoutException or JsonException or KeyNotFoundException or IOException or InvalidOperationException)
        {
            process.Kill();
            throw new InvalidOperationException($"Plugin '{fileName}' failed its handshake: {ex.Message}", ex);
        }
    }

    public IReadOnlyList<string> Render(ReadOnlySequence<byte> data)
    {
        lock (_gate)
        {
            if (Faulted)
            {
                return [];
            }

            try
            {
                var request = JsonSerializer.Serialize(new { type = "render", hex = Convert.ToHexString(data.ToArray()) });
                _process.Input.WriteLine(request);
                _process.Input.Flush();
                var line = _process.Output.ReadLineAsync().WaitAsync(_replyTimeout).GetAwaiter().GetResult();
                if (line is null)
                {
                    return Fault("the plugin exited");
                }

                using var doc = JsonDocument.Parse(line);
                return [.. doc.RootElement.GetProperty("lines").EnumerateArray().Select(l => l.GetString() ?? string.Empty)];
            }
            catch (TimeoutException)
            {
                return Fault($"no reply within {_replyTimeout.TotalMilliseconds:0} ms");
            }
            catch (Exception ex) when (ex is JsonException or KeyNotFoundException or InvalidOperationException or IOException)
            {
                return Fault(ex.Message);
            }
        }
    }

    public ValueTask DisposeAsync()
    {
        _process.Dispose();
        return ValueTask.CompletedTask;
    }

    private IReadOnlyList<string> Fault(string reason)
    {
        Faulted = true;
        FaultReason = reason;
        _process.Kill();
        return [];
    }
}
