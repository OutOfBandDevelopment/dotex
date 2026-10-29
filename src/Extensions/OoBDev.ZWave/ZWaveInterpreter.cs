using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;

namespace OoBDev.ZWave;

/// <summary>
/// Turns decoded <see cref="ZWaveFrame"/>s into readable text and named values. Pure: no state, no I/O.
/// </summary>
public static class ZWaveInterpreter
{
    private static readonly Dictionary<string, string> _none = new(StringComparer.Ordinal);

    /// <summary>
    /// Describes one frame.
    /// </summary>
    /// <param name="frame">The decoded frame.</param>
    /// <returns>The line of text and the values the frame carries.</returns>
    public static ZWaveReading Interpret(ZWaveFrame frame)
    {
        ArgumentNullException.ThrowIfNull(frame);
        return frame.Kind switch
        {
            ZWaveFrameKind.Ack => new ZWaveReading("ACK", _none),
            ZWaveFrameKind.Nak => new ZWaveReading("NAK (the controller rejected the last frame)", _none),
            ZWaveFrameKind.Can => new ZWaveReading("CAN (the controller dropped the last frame; ACK its previous reply first)", _none),
            ZWaveFrameKind.Invalid => new ZWaveReading($"invalid bytes: {Hex(frame.Raw)}", _none),
            _ => InterpretData(frame),
        };
    }

    /// <summary>
    /// Formats bytes as space-separated upper-case hex pairs.
    /// </summary>
    /// <param name="bytes">The bytes.</param>
    /// <returns>For example <c>01 03 00 15 E9</c>.</returns>
    public static string Hex(ReadOnlySpan<byte> bytes)
    {
        var sb = new StringBuilder();
        foreach (var b in bytes)
        {
            if (sb.Length > 0) sb.Append(' ');
            sb.Append(b.ToString("X2", CultureInfo.InvariantCulture));
        }

        return sb.ToString();
    }

    /// <summary>
    /// The node ids set in a <c>GetInitData</c> node bitmap (bit 0 of byte 0 is node 1).
    /// </summary>
    /// <param name="bitmap">The bitmap bytes.</param>
    /// <returns>The node ids in ascending order.</returns>
    public static IReadOnlyList<int> NodesIn(ReadOnlySpan<byte> bitmap)
    {
        var nodes = new List<int>();
        for (var i = 0; i < bitmap.Length; i++)
            for (var bit = 0; bit < 8; bit++)
                if ((bitmap[i] & (1 << bit)) != 0) nodes.Add((i * 8) + bit + 1);

        return nodes;
    }

    /// <summary>
    /// Node ids as ranges.
    /// </summary>
    /// <param name="nodes">Ascending node ids.</param>
    /// <returns>For example <c>1-12,14-21,23</c>.</returns>
    public static string Ranges(IReadOnlyList<int> nodes)
    {
        ArgumentNullException.ThrowIfNull(nodes);
        var parts = new List<string>();
        for (var i = 0; i < nodes.Count;)
        {
            var j = i;
            while (j + 1 < nodes.Count && nodes[j + 1] == nodes[j] + 1) j++;
            parts.Add(j == i ? nodes[i].ToString(CultureInfo.InvariantCulture) : $"{nodes[i]}-{nodes[j]}");
            i = j + 1;
        }

        return string.Join(',', parts);
    }

    /// <summary>
    /// One <c>GetNodeProtocolInfo</c> reply as a phrase.
    /// </summary>
    /// <param name="payload">The reply payload.</param>
    /// <returns>The phrase, or <see langword="null"/> for a node the controller does not know.</returns>
    public static string? DescribeProtocolInfo(ReadOnlySpan<byte> payload)
    {
        if (payload.Length < 6 || payload[4] == 0) return null;

        var listening = (payload[0] & 0x80) != 0 ? "listening" : "sleeping";
        var routing = (payload[0] & 0x40) != 0 ? ", routing" : string.Empty;
        return $"{listening}{routing}, {ZWaveNames.BasicDeviceClass(payload[3])}, {ZWaveNames.GenericDeviceClass(payload[4])} (specific 0x{payload[5]:X2})";
    }

    /// <summary>
    /// A command class payload (<c>class command params...</c>) as a phrase.
    /// </summary>
    /// <param name="command">The command class payload.</param>
    /// <returns>For example <c>Basic Report 99</c>.</returns>
    public static string DescribeCommand(ReadOnlySpan<byte> command)
    {
        if (command.Length < 2)
            return command.Length == 0 ? "(empty command)" : $"{ZWaveNames.CommandClass(command[0])} (no command)";

        var name = ZWaveNames.CommandClass(command[0]);
        var parameters = command[2..];
        var hasValue = parameters.Length > 0;
        var value = hasValue ? parameters[0] : (byte)0;
        return (command[0], command[1]) switch
        {
            (0x20, 0x01) when hasValue => $"{name} Set {value}",
            (0x20, 0x02) => $"{name} Get",
            (0x20, 0x03) when hasValue => $"{name} Report {value}",
            (0x25, 0x01) when hasValue => $"{name} Set {OnOff(value)}",
            (0x25, 0x02) => $"{name} Get",
            (0x25, 0x03) when hasValue => $"{name} Report {OnOff(value)}",
            (0x26, 0x01) when hasValue => $"{name} Set {value}",
            (0x26, 0x02) => $"{name} Get",
            (0x26, 0x03) when hasValue => $"{name} Report {value}",
            (0x30, 0x03) when hasValue => $"{name} Report {(value == 0 ? "idle" : "triggered")}",
            (0x5B, 0x03) when parameters.Length >= 3 => $"{name} Notification scene {parameters[2]}, key attribute 0x{parameters[1]:X2}",
            (0x71, 0x05) when parameters.Length >= 5 => $"{name} Report type 0x{parameters[4]:X2}, event 0x{(parameters.Length > 5 ? parameters[5] : (byte)0):X2}",
            (0x80, 0x03) when hasValue => value == 0xFF ? $"{name} Report low battery warning" : $"{name} Report {value}%",
            (0x72, 0x05) when parameters.Length >= 6 => ManufacturerReport(name, parameters),
            (0x70, 0x06) when parameters.Length >= 3 && parameters[1] is 1 or 2 or 4 && parameters.Length >= 2 + parameters[1] => $"{name} Report parameter {parameters[0]} = {ConfigValue(parameters[2..(2 + parameters[1])])}",
            (0x86, 0x12) when parameters.Length >= 5 => $"{name} Report library {parameters[0]}, protocol {parameters[1]}.{parameters[2]:D2}, application {parameters[3]}.{parameters[4]:D2}",
            (0x86, 0x14) when parameters.Length >= 2 => $"{name} Command Class Report {ZWaveNames.CommandClass(parameters[0])} version {parameters[1]}",
            (0x5E, 0x02) when parameters.Length >= 4 => $"{name} Report version {parameters[0]}, role type 0x{parameters[1]:X2}, node type 0x{parameters[2]:X2}",
            (0x85, 0x06) when hasValue => $"{name} Groupings Report {value} association group(s)",
            (0x60, 0x08) when parameters.Length >= 2 => $"{name} End Point Report {parameters[1] & 0x7F} end point(s)",
            (0x98, 0x03) when hasValue => $"{name} Commands Supported Report {ClassList(parameters[1..])}",
            (0x84, 0x07) => $"{name} Notification (the node is awake)",
            _ => parameters.Length == 0 ? $"{name} command 0x{command[1]:X2}" : $"{name} command 0x{command[1]:X2} {Hex(parameters)}",
        };
    }

    private static string OnOff(byte value) => value == 0 ? "off" : "on";

    private static ZWaveReading InterpretData(ZWaveFrame frame)
    {
        var p = frame.Payload;
        var label = $"{(frame.IsResponse ? "RES" : "REQ")} {ZWaveNames.Function(frame.Function)}";
        var values = new Dictionary<string, string>(StringComparer.Ordinal);

        string? detail;
        if (frame.IsResponse)
        {
            detail = frame.Function switch
            {
                ZWaveFunctions.GetVersion => Version(p, values),
                ZWaveFunctions.MemoryGetId => MemoryId(p, values),
                ZWaveFunctions.GetCapabilities => Capabilities(p, values),
                ZWaveFunctions.GetControllerCapabilities => ControllerCapabilities(p, values),
                ZWaveFunctions.GetInitData => InitData(p, values),
                ZWaveFunctions.GetSucNodeId => SucNode(p, values),
                ZWaveFunctions.GetNodeProtocolInfo => ProtocolInfo(p, values),
                ZWaveFunctions.IsFailedNode when p.Length > 0 => IsFailed(p[0], values),
                ZWaveFunctions.RequestNodeInfo when p.Length > 0 => p[0] != 0 ? "request queued (the node answers with an ApplicationUpdate)" : "request rejected",
                ZWaveFunctions.RemoveFailedNode when p.Length > 0 => RemoveFailedStart(p[0]),
                ZWaveFunctions.SendData when p.Length > 0 => p[0] != 0 ? "accepted for transmission" : "rejected (the transmit queue is full)",
                _ => null,
            };
        }
        else
        {
            detail = frame.Function switch
            {
                ZWaveFunctions.ApplicationCommandHandler when p.Length >= 3 => ApplicationCommand(p, values),
                ZWaveFunctions.SendData when p.Length >= 2 => SendCallback(p, values),
                ZWaveFunctions.ApplicationUpdate when p.Length >= 2 => AppUpdate(p, values),
                ZWaveFunctions.AddNodeToNetwork when p.Length >= 2 => NodeManagement(p, "include", values),
                ZWaveFunctions.RemoveNodeFromNetwork when p.Length >= 2 => NodeManagement(p, "exclude", values),
                ZWaveFunctions.SetLearnMode when p.Length >= 2 => NodeManagement(p, "learn", values),
                ZWaveFunctions.SetDefault when p.Length >= 1 => Event(values, "network reset to defaults (new home ID; read it with MemoryGetId)"),
                ZWaveFunctions.RequestNodeNeighborUpdate when p.Length >= 2 => Event(values, NeighborStatus(p[1])),
                ZWaveFunctions.RemoveFailedNode when p.Length >= 2 => Event(values, RemoveFailedStatus(p[1])),
                _ => null,
            };
        }

        detail ??= p.Length == 0 ? null : Hex(p);
        return new ZWaveReading(detail is null ? label : $"{label}: {detail}", values);
    }

    private static string Version(ReadOnlySpan<byte> p, Dictionary<string, string> values)
    {
        if (p.Length < 2) return Hex(p);

        var result = $"{Encoding.ASCII.GetString(p[..^1]).TrimEnd('\0')}, {ZWaveNames.LibraryType(p[^1])}";
        values["library"] = result;
        return result;
    }

    private static string MemoryId(ReadOnlySpan<byte> p, Dictionary<string, string> values)
    {
        if (p.Length < 5) return Hex(p);

        var home = Convert.ToHexString(p[..4]);
        values["homeId"] = home;
        values["controllerNode"] = p[4].ToString(CultureInfo.InvariantCulture);
        return $"home ID {home}, this controller is node {p[4]}";
    }

    private static string Capabilities(ReadOnlySpan<byte> p, Dictionary<string, string> values)
    {
        if (p.Length < 8) return Hex(p);

        var manufacturer = ZWaveNames.Manufacturer((ushort)((p[2] << 8) | p[3]));
        var productType = (p[4] << 8) | p[5];
        var productId = (p[6] << 8) | p[7];
        var functions = 0;
        foreach (var b in p[8..]) functions += BitOperations.PopCount(b);

        values["serialApi"] = $"{p[0]}.{p[1]}";
        values["manufacturer"] = manufacturer;
        values["product"] = $"0x{productType:X4} / 0x{productId:X4}";
        values["functions"] = functions.ToString(CultureInfo.InvariantCulture);
        return $"Serial API {p[0]}.{p[1]}, manufacturer {manufacturer}, product 0x{productType:X4}/0x{productId:X4}, {functions} functions";
    }

    private static string ControllerCapabilities(ReadOnlySpan<byte> p, Dictionary<string, string> values)
    {
        if (p.Length < 1) return string.Empty;

        var caps = p[0];
        var flags = new List<string>();
        (int Mask, string Text)[] known =
        [
            (0x01, "secondary"), (0x02, "on another network"), (0x04, "SIS present"),
            (0x08, "was primary"), (0x10, "SUC"), (0x20, "no nodes included"),
        ];
        foreach (var (mask, text) in known)
            if ((caps & mask) != 0) flags.Add(text);

        var role = flags.Count == 0 ? "primary" : string.Join(", ", flags);
        values["role"] = role;
        return role;
    }

    private static string InitData(ReadOnlySpan<byte> p, Dictionary<string, string> values)
    {
        if (p.Length < 3 || p.Length < 3 + p[2]) return Hex(p);

        var nodes = NodesIn(p.Slice(3, p[2]));
        var ranges = Ranges(nodes);
        values["nodeCount"] = nodes.Count.ToString(CultureInfo.InvariantCulture);
        values["nodes"] = ranges;
        var tail = p[(3 + p[2])..];
        var chip = string.Empty;
        if (tail.Length >= 2)
        {
            values["chip"] = $"{tail[0]} v{tail[1]}";
            chip = $", chip {tail[0]} v{tail[1]}";
        }

        return $"{nodes.Count} nodes ({ranges}), API version {p[0]}{chip}";
    }

    private static string SucNode(ReadOnlySpan<byte> p, Dictionary<string, string> values)
    {
        if (p.Length < 1) return string.Empty;

        values["sucNode"] = p[0] == 0 ? "none" : p[0].ToString(CultureInfo.InvariantCulture);
        return p[0] == 0 ? "no SUC on this network" : $"SUC is node {p[0]}";
    }

    private static string ApplicationCommand(ReadOnlySpan<byte> p, Dictionary<string, string> values)
    {
        var length = Math.Min(p[2], p.Length - 3);
        var description = DescribeCommand(p.Slice(3, length));
        values["lastReport"] = $"node {p[1]} sent {description}";
        values["reportNode"] = p[1].ToString(CultureInfo.InvariantCulture);
        values["reportText"] = description;
        if (length >= 8 && p[3] == 0x72 && p[4] == 0x05)
        {
            var manufacturer = (ushort)((p[5] << 8) | p[6]);
            var type = (ushort)((p[7] << 8) | p[8]);
            var id = (ushort)((p[9] << 8) | p[10]);
            values["reportIds"] = $"{manufacturer:X4}:{type:X4}:{id:X4}";
        }

        return values["lastReport"];
    }

    // The reply does not name its node; ZWaveNodeTable attaches "protocol" / "failed" to the node it asked about.
    private static string ProtocolInfo(ReadOnlySpan<byte> p, Dictionary<string, string> values)
    {
        var text = DescribeProtocolInfo(p) ?? "no such node";
        values["protocol"] = text;
        return text;
    }

    private static string IsFailed(byte failed, Dictionary<string, string> values)
    {
        values["failed"] = failed != 0 ? "FAILED" : "ok";
        return failed != 0 ? "node is marked failed" : "node is not marked failed";
    }

    private static string Event(Dictionary<string, string> values, string text)
    {
        values["lastEvent"] = text;
        return text;
    }

    // Include / exclude / learn-mode callbacks: callback id, status, source node, node-info length, node-info bytes.
    private static string NodeManagement(ReadOnlySpan<byte> p, string kind, Dictionary<string, string> values)
    {
        var node = p.Length > 2 ? p[2] : (byte)0;
        var who = node == 0 ? string.Empty : $" node {node}";
        var include = kind == "include";
        var text = (kind, p[1]) switch
        {
            ("learn", 0x01) => "learn mode started: start the other controller's include",
            ("learn", 0x06) => "learn mode done: this controller joined a network (read the home ID with MemoryGetId)",
            (_, 0x01) => include ? "ready: put the device in include mode now (3 quick presses of its button)" : "ready: put the device in exclude mode now",
            (_, 0x02) => "a device was found",
            (_, 0x03) => include ? $"adding slave{who}" : $"removing slave{who}",
            (_, 0x04) => include ? $"adding controller{who}" : $"removing controller{who}",
            (_, 0x05) => "protocol part done",
            (_, 0x06) => include ? $"include done{who}" : $"exclude done{who}",
            (_, 0x07) => $"{kind} failed",
            _ => $"{kind} status 0x{p[1]:X2}{who}",
        };
        return Event(values, text);
    }

    private static string NeighborStatus(byte status) => status switch
    {
        0x21 => "neighbor update started",
        0x22 => "neighbor update done",
        0x23 => "neighbor update failed",
        _ => $"neighbor update status 0x{status:X2}",
    };

    private static string RemoveFailedStatus(byte status) => status switch
    {
        0x00 => "failed node removal started",
        0x01 => "the node responded, so it is not failed",
        0x02 => "failed node removed",
        0x03 => "failed node removal failed",
        _ => $"failed node removal status 0x{status:X2}",
    };

    private static string RemoveFailedStart(byte code) =>
        code == 0 ? "removal started" : $"not started (code 0x{code:X2}: the node is not failed, or the controller is busy)";

    // ApplicationUpdate: status, node, then for 0x84 (node info received) a length, basic/generic/specific class and the supported command classes.
    private static string AppUpdate(ReadOnlySpan<byte> p, Dictionary<string, string> values)
    {
        var text = p[0] switch
        {
            0x84 when p.Length >= 6 => $"node {p[1]} info: {ZWaveNames.BasicDeviceClass(p[3])}, {ZWaveNames.GenericDeviceClass(p[4])}, supports {ClassList(p[6..])}",
            0x84 => $"node {p[1]} info received",
            0x81 => $"node {p[1]} did not answer the node-info request",
            0x82 => $"node {p[1]} info request done",
            0x40 => $"node id {p[1]} assigned",
            0x20 => $"node {p[1]} deleted",
            0x10 => $"SUC id is node {p[1]}",
            0x80 => $"node {p[1]} routing pending",
            _ => $"status 0x{p[0]:X2}, node {p[1]}",
        };
        values["lastReport"] = text;
        if (p[0] == 0x84 && p.Length >= 6)
        {
            values["updateNode"] = p[1].ToString(CultureInfo.InvariantCulture);
            values["updateClasses"] = ClassList(p[6..]);
        }

        return text;
    }

    private static long ConfigValue(ReadOnlySpan<byte> bytes)
    {
        long value = (sbyte)bytes[0];
        for (var i = 1; i < bytes.Length; i++) value = (value << 8) | bytes[i];
        return value;
    }

    private static string ManufacturerReport(string name, ReadOnlySpan<byte> p)
    {
        var manufacturer = (ushort)((p[0] << 8) | p[1]);
        var type = (ushort)((p[2] << 8) | p[3]);
        var id = (ushort)((p[4] << 8) | p[5]);
        return $"{name} Report {ZWaveNames.Manufacturer(manufacturer)}, product type 0x{type:X4}, id 0x{id:X4}";
    }

    private static string ClassList(ReadOnlySpan<byte> classes)
    {
        var names = new List<string>();
        foreach (var c in classes)
            names.Add(c == 0xEF ? "|" : ZWaveNames.CommandClass(c)); // 0xEF marks the controlled classes that follow

        return names.Count == 0 ? "no command classes" : string.Join(", ", names);
    }

    private static string SendCallback(ReadOnlySpan<byte> p, Dictionary<string, string> values)
    {
        var status = p[1] switch
        {
            0x00 => "delivered",
            0x01 => "no acknowledgement from the node",
            0x02 => "transmission failed",
            0x03 => "the controller was busy routing",
            0x04 => "no route to the node",
            _ => $"status 0x{p[1]:X2}",
        };
        values["lastSend"] = $"#{p[0]}: {status}";
        return $"transmit #{p[0]} {status}";
    }
}
