using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OoBDev.UiDefinitions;

namespace OoBDev.ZWave.Panels;

/// <summary>Turns panel commands into Serial API request frames and sends them. Each request that expects a callback gets a fresh callback id.</summary>
public sealed class ZWaveControlSurface : IControlSurface, ICommandPreview
{
    private readonly DeviceSend _send;
    private int _callbackId;

    /// <summary>Creates the surface over the connection's send function.</summary>
    public ZWaveControlSurface(DeviceSend send)
    {
        ArgumentNullException.ThrowIfNull(send);
        _send = send;
    }

    /// <inheritdoc/>
    public Task InvokeAsync(string commandId, string? value, CancellationToken cancellationToken = default)
    {
        var frame = Build(commandId, value, NextCallback());
        return _send(frame.Raw, cancellationToken);
    }

    /// <inheritdoc/>
    public string? PreviewCommand(string commandId, string? value)
    {
        ArgumentNullException.ThrowIfNull(commandId);
        return ZWaveUiDefinition.CommandIds.Contains(commandId)
            ? CommandPreviewFormat.ToHex(Build(commandId, value, 2).Raw)
            : null;
    }

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
