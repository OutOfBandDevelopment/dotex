using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OoBDev.UiDefinitions;

namespace OoBDev.ZWave.Panels;

/// <summary>Turns panel commands into Serial API request frames and sends them. Each request that expects a callback gets a fresh callback id.</summary>
public sealed class ZWaveControlSurface : IControlSurface, ICommandPreview, IDisposable
{
    private static readonly TimeSpan _defaultRefreshGap = TimeSpan.FromMilliseconds(150);

    private const string _refreshId = "refresh";

    private readonly DeviceSend _send;
    private readonly TimeProvider _time;
    private readonly TimeSpan _refreshGap;
    private static readonly TimeSpan _defaultReplyTimeout = TimeSpan.FromSeconds(3);

    private readonly ZWaveDecoder? _decoder;
    private readonly TimeSpan _replyTimeout;
    private int _callbackId;

    /// <summary>Creates the surface over the connection's send function.</summary>
    /// <param name="send">Sends bytes to the stick.</param>
    /// <param name="time">The clock for the pause between the Refresh requests, or <see langword="null"/> for the system clock.</param>
    /// <param name="refreshGap">The pause between the Refresh requests, or <see langword="null"/> for 150 ms, which the stick needs to answer each one.</param>
    /// <param name="decoder">
    /// The decoder of the same session. While the surface lives it acknowledges every data frame the decoder sees (the stick
    /// retransmits and then answers CAN without it), and Enumerate and Describe node read the replies from its node table.
    /// </param>
    /// <param name="replyTimeout">How long Enumerate waits for the node list, or <see langword="null"/> for 3 seconds.</param>
    public ZWaveControlSurface(DeviceSend send, TimeProvider? time = null, TimeSpan? refreshGap = null, ZWaveDecoder? decoder = null, TimeSpan? replyTimeout = null)
    {
        ArgumentNullException.ThrowIfNull(send);
        _send = send;
        _time = time ?? TimeProvider.System;
        _refreshGap = refreshGap ?? _defaultRefreshGap;
        _replyTimeout = replyTimeout ?? _defaultReplyTimeout;
        _decoder = decoder;
        if (_decoder is not null)
        {
            _decoder.FrameDecoded += OnFrameDecoded;
        }
    }

    /// <summary>Stops acknowledging the stick's frames; called when the panel closes.</summary>
    public void Dispose()
    {
        if (_decoder is not null)
        {
            _decoder.FrameDecoded -= OnFrameDecoded;
        }
    }

    // Runs on the read loop, so the answer is sent from the pool: awaiting a send there would stall the reads.
    private void OnFrameDecoded(object? sender, ZWaveFrame frame)
    {
        if (ZWaveAckResponder.Respond(frame) is { } answer)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    await _send(answer.Raw, CancellationToken.None).ConfigureAwait(false);
                }
                catch (Exception ex) when (ex is not OutOfMemoryException)
                {
                    // The stick retransmits an unanswered frame; a failed ACK needs no other handling.
                }
            });
        }
    }

    /// <inheritdoc/>
    public async Task InvokeAsync(string commandId, string? value, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(commandId);
        if (commandId == _refreshId)
        {
            var frames = RefreshFrames();
            for (var i = 0; i < frames.Length; i++)
            {
                if (i > 0)
                {
                    await Task.Delay(_refreshGap, _time, cancellationToken).ConfigureAwait(false);
                }

                await _send(frames[i].Raw, cancellationToken).ConfigureAwait(false);
            }

            return;
        }

        if (commandId is "enumerate" or "describe")
        {
            await QueryNodesAsync(commandId, value, cancellationToken).ConfigureAwait(false);
            return;
        }

        var frame = Build(commandId, value, NextCallback());
        await _send(frame.Raw, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public string? PreviewCommand(string commandId, string? value)
    {
        ArgumentNullException.ThrowIfNull(commandId);
        if (commandId == _refreshId)
        {
            return string.Join(" | ", RefreshFrames().Select(f => CommandPreviewFormat.ToHex(f.Raw)));
        }

        // Enumerate and Describe node are sequences that depend on replies, so they preview as a description, not a frame.
        if (commandId is "enumerate" or "describe")
        {
            return commandId == "enumerate" ? "node list, then protocol info, failed state, manufacturer and command classes per node" : "protocol info, failed state, manufacturer and command classes of the node";
        }

        return ZWaveUiDefinition.CommandIds.Contains(commandId)
            ? CommandPreviewFormat.ToHex(Build(commandId, value, 2).Raw)
            : null;
    }

    private async Task QueryNodesAsync(string commandId, string? value, CancellationToken cancellationToken)
    {
        var table = (_decoder ?? throw new InvalidOperationException("Enumerate and Describe node need the zwave presenter on the session.")).Table;
        int[] nodes;
        if (commandId == "describe")
        {
            nodes = [Node((value ?? string.Empty).Split(',', StringSplitOptions.TrimEntries))];
        }
        else
        {
            var listed = WaitForAsync(f => f.IsResponse && f.Function == ZWaveFunctions.GetInitData, cancellationToken);
            await _send(ZWaveRequests.MemoryGetId().Raw, cancellationToken).ConfigureAwait(false);
            await Task.Delay(_refreshGap, _time, cancellationToken).ConfigureAwait(false);
            await _send(ZWaveRequests.GetInitData().Raw, cancellationToken).ConfigureAwait(false);
            if (!await listed.ConfigureAwait(false))
            {
                throw new TimeoutException("The controller did not send its node list.");
            }

            nodes = [.. table.Nodes.Select(n => n.Node)];
        }

        foreach (var node in nodes)
        {
            var id = (byte)node;
            table.Expect(ZWaveFunctions.GetNodeProtocolInfo, node);
            table.Expect(ZWaveFunctions.IsFailedNode, node);
            ZWaveFrame[] requests =
            [
                ZWaveRequests.GetNodeProtocolInfo(id),
                ZWaveRequests.IsFailedNode(id),
                ZWaveRequests.RequestNodeInfo(id),
                ZWaveRequests.SendData(id, [0x72, 0x04], NextCallback()),
                ZWaveRequests.SendData(id, [0x86, 0x11], NextCallback()),
            ];
            foreach (var request in requests)
            {
                await _send(request.Raw, cancellationToken).ConfigureAwait(false);
                await Task.Delay(_refreshGap, _time, cancellationToken).ConfigureAwait(false);
            }
        }
    }

    // True when a frame matching the predicate arrives before the timeout.
    private async Task<bool> WaitForAsync(Func<ZWaveFrame, bool> matches, CancellationToken cancellationToken)
    {
        var arrived = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        void OnFrame(object? sender, ZWaveFrame frame)
        {
            if (matches(frame))
            {
                arrived.TrySetResult(true);
            }
        }

        _decoder!.FrameDecoded += OnFrame;
        try
        {
            var timeout = Task.Delay(_replyTimeout, _time, cancellationToken);
            return await Task.WhenAny(arrived.Task, timeout).ConfigureAwait(false) == arrived.Task;
        }
        finally
        {
            _decoder.FrameDecoded -= OnFrame;
        }
    }

    private static ZWaveFrame[] RefreshFrames() =>
    [
        ZWaveRequests.GetVersion(),
        ZWaveRequests.MemoryGetId(),
        ZWaveRequests.GetCapabilities(),
        ZWaveRequests.GetControllerCapabilities(),
        ZWaveRequests.GetInitData(),
        ZWaveRequests.GetSucNodeId(),
    ];

    private byte NextCallback()
    {
        // Callback ids 0 and 1 are reserved by the Serial API, so cycle through 2..255.
        var next = Interlocked.Increment(ref _callbackId);
        return (byte)(2 + ((next - 1) % 254));
    }

    private static ZWaveFrame Build(string commandId, string? value, byte callback)
    {
        ArgumentNullException.ThrowIfNull(commandId);
        var parts = (value ?? string.Empty).Split(',', StringSplitOptions.TrimEntries);
        return commandId switch
        {
            "version" => ZWaveRequests.GetVersion(),
            "memoryId" => ZWaveRequests.MemoryGetId(),
            "initData" => ZWaveRequests.GetInitData(),
            "softReset" => ZWaveRequests.SoftReset(),
            "nodeInfo" => ZWaveRequests.RequestNodeInfo(Node(parts)),
            "switchOn" => ZWaveRequests.SwitchSet(Node(parts), true, callback),
            "switchOff" => ZWaveRequests.SwitchSet(Node(parts), false, callback),
            "switchGet" => ZWaveRequests.SwitchGet(Node(parts), callback),
            "setLevel" => ZWaveRequests.LevelSet(Node(parts), Level(parts), callback),
            "addNode" => ZWaveRequests.AddNode(callback),
            "stopAddNode" => ZWaveRequests.StopAddNode(callback),
            "removeNode" => ZWaveRequests.RemoveNode(callback),
            "stopRemoveNode" => ZWaveRequests.StopRemoveNode(callback),
            "resetNetwork" => ZWaveRequests.SetDefault(callback),
            "learnOn" => ZWaveRequests.SetLearnMode(true, callback),
            "learnOff" => ZWaveRequests.SetLearnMode(false, callback),
            "heal" => ZWaveRequests.NeighborUpdate(Node(parts), callback),
            "isFailed" => ZWaveRequests.IsFailedNode(Node(parts)),
            "removeFailed" => ZWaveRequests.RemoveFailedNode(Node(parts), callback),
            "configGet" => ZWaveRequests.ConfigurationGet(Node(parts), Parameter(parts), callback),
            "configSet" => ConfigSet(Node(parts), Parameter(parts), callback, parts),
            "sendRaw" => ZWaveRequests.SendData(Node(parts), RawPayload(parts), callback),
            _ => throw new ArgumentException($"Unknown Z-Wave command '{commandId}'.", nameof(commandId)),
        };
    }

    private static byte Node(string[] parts) =>
        parts.Length > 0 && byte.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var node) && node is >= 1 and <= 232
            ? node
            : throw new ArgumentException("A node id from 1 to 232 is required.", nameof(parts));

    private static byte Parameter(string[] parts)
    {
        var tokens = parts.Length > 1 ? parts[1].Split(' ', StringSplitOptions.RemoveEmptyEntries) : [];
        return tokens.Length > 0 && byte.TryParse(tokens[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var parameter)
            ? parameter
            : throw new ArgumentException("A configuration parameter number from 0 to 255 is required.", nameof(parts));
    }

    private static ZWaveFrame ConfigSet(byte node, byte parameter, byte callback, string[] parts)
    {
        var tokens = parts[1].Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (tokens.Length < 2 || !int.TryParse(tokens[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var value))
        {
            throw new ArgumentException("Setting a parameter needs a number value: 'parameter value [size]'.", nameof(parts));
        }

        var size = value is >= -128 and <= 255 ? 1 : value is >= -32768 and <= 65535 ? 2 : 4;
        if (tokens.Length > 2 && (!int.TryParse(tokens[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out size) || size is not (1 or 2 or 4)))
        {
            throw new ArgumentException("The value size is 1, 2 or 4 bytes.", nameof(parts));
        }

        return ZWaveRequests.ConfigurationSet(node, parameter, value, size, callback);
    }

    private static byte[] RawPayload(string[] parts)
    {
        try
        {
            var payload = parts.Length > 1 ? Convert.FromHexString(string.Concat(parts[1].Split(' ', StringSplitOptions.RemoveEmptyEntries))) : [];
            return payload.Length >= 2 ? payload : throw new ArgumentException("A command class payload of at least two hex bytes is required.", nameof(parts));
        }
        catch (FormatException)
        {
            throw new ArgumentException("The payload must be hex bytes, for example '26 01 32'.", nameof(parts));
        }
    }

    private static byte Level(string[] parts) =>
        parts.Length > 1 && byte.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var level) && level <= 99
            ? level
            : throw new ArgumentException("A level from 0 to 99 is required.", nameof(parts));
}
