using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.IO.Pipelines;
using System.Text;
using System.Text.RegularExpressions;
using OoBDev.DevTerm;
using OoBDev.System.IO;
using OoBDev.System.IO.Pipelines;
using System.IO.Pipelines;
namespace OoBDev.DevTerm.Tui.Tests;

/// <summary>
/// An in-process <see cref="ITransport"/> that answers scripted commands deterministically, instead
/// of a real device or even a real socket — the same <see cref="Pipe"/>-backed shape as
/// <see cref="FakeTransport"/>, but with request/response behavior baked in so a test doesn't have
/// to manually call <c>PushIncomingAsync</c> after every write. Exists to exercise
/// Session/TuiMode/MainWindow logic against realistic multi-line replies and "event stream" style
/// output without any real transport I/O — see docs/applications/devterm/design/testing.md.
/// </summary>
internal sealed class LoopbackTransport : IDeviceAdapter
{
    private readonly Pipe _pipe = new();
    private PipeWriter? _output;
    private readonly IReadOnlyList<LoopbackRule> _rules;
    private ConnectionState _state = ConnectionState.Closed;

    /// <param name="rules">The script to answer with — defaults to <see cref="LoopbackScript.Default"/> when omitted.</param>
    public LoopbackTransport(IReadOnlyList<LoopbackRule>? rules = null) => _rules = rules ?? LoopbackScript.Default();

    public List<byte[]> WrittenPayloads { get; } = [];

    public ConnectionState State
    {
        get => _state;
        private set
        {
            if (_state == value)
            {
                return;
            }

            var previous = _state;
            _state = value;
            StateChanged?.Invoke(this, new ConnectionStateChangedEventArgs(previous, value));
        }
    }

    public event EventHandler<ConnectionStateChangedEventArgs>? StateChanged;

    public string Type => "Loopback";

    public string Path => "loopback";

    public PipeReader Input => _pipe.Reader;

    public PipeWriter Output => _output ??= DeviceAdapterOutput.Create(WriteAsync);

    public Task OpenAsync(CancellationToken cancellationToken = default)
    {
        State = ConnectionState.Open;
        return Task.CompletedTask;
    }

    public async Task CloseAsync(CancellationToken cancellationToken = default)
    {
        await _pipe.Writer.CompleteAsync();
        State = ConnectionState.Closed;
    }

    public async Task WriteAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
    {
        WrittenPayloads.Add(data.ToArray());

        var line = Encoding.ASCII.GetString(data.Span).TrimEnd('\r', '\n');
        if (line.Length == 0)
        {
            return;
        }

        var rule = _rules.FirstOrDefault(r => r.Pattern.IsMatch(line));
        if (rule is null)
        {
            // A visible marker rather than silence: a test whose rules don't cover a sent command
            // should fail loudly (wrong/missing output), not hang waiting on a reply that never comes.
            await PushLineAsync($"? Unrecognized: {line}");
            return;
        }

        foreach (var responseLine in rule.Respond(rule.Pattern.Match(line)))
        {
            await PushLineAsync(responseLine);
        }
    }

    public async ValueTask DisposeAsync() => await CloseAsync();

    private async Task PushLineAsync(string text) =>
        await _pipe.Writer.WriteAsync(Encoding.ASCII.GetBytes(text + "\r\n"));
}
