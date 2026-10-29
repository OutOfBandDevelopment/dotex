using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace OoBDev.ZWave;

/// <summary>
/// What is known about the nodes on the network, built from the frames the controller sends. Replies to
/// <c>GetNodeProtocolInfo</c> and <c>IsFailedNode</c> do not name their node, so the consumer says which node it asked about
/// with <see cref="Expect"/> before sending; replies are matched in the order asked.
/// </summary>
public sealed class ZWaveNodeTable
{
    private readonly Dictionary<int, ZWaveNodeRecord> _nodes = [];
    private readonly Queue<(byte Function, int Node)> _expected = new();

    /// <summary>Gets the nodes seen so far, in node id order.</summary>
    public IReadOnlyList<ZWaveNodeRecord> Nodes => [.. _nodes.Values.OrderBy(n => n.Node)];

    /// <summary>Gets the home id, once a <c>MemoryGetId</c> reply has been applied.</summary>
    public string? HomeId { get; private set; }

    /// <summary>
    /// Records that the next reply to <paramref name="function"/> concerns <paramref name="node"/>.
    /// </summary>
    /// <param name="function">The request's function id.</param>
    /// <param name="node">The node the request names.</param>
    public void Expect(byte function, int node) => _expected.Enqueue((function, node));

    /// <summary>
    /// Folds one decoded frame into the table.
    /// </summary>
    /// <param name="frame">The frame.</param>
    /// <returns>The node the frame was attributed to, or <see langword="null"/> when it names none.</returns>
    public ZWaveNodeRecord? Apply(ZWaveFrame frame)
    {
        ArgumentNullException.ThrowIfNull(frame);
        if (frame.Kind != ZWaveFrameKind.Data) return null;

        var reading = ZWaveInterpreter.Interpret(frame);
        var v = reading.Values;
        if (frame.IsResponse)
        {
            if (frame.Function == ZWaveFunctions.MemoryGetId && v.TryGetValue("homeId", out var home))
            {
                HomeId = home;
                return null;
            }

            if (frame.Function == ZWaveFunctions.GetInitData && frame.Payload.Length >= 3 && frame.Payload.Length >= 3 + frame.Payload[2])
            {
                foreach (var n in ZWaveInterpreter.NodesIn(frame.Payload.Slice(3, frame.Payload[2]))) Record(n);
                return null;
            }

            if (frame.Function is ZWaveFunctions.GetNodeProtocolInfo or ZWaveFunctions.IsFailedNode && TakeExpected(frame.Function) is { } asked)
            {
                var record = Record(asked);
                if (v.TryGetValue("protocol", out var protocol)) record.Protocol = protocol;
                if (v.TryGetValue("failed", out var failed)) record.Failed = failed;
                return record;
            }

            return null;
        }

        if (frame.Function == ZWaveFunctions.ApplicationUpdate && v.TryGetValue("updateNode", out var un) && v.TryGetValue("updateClasses", out var classes))
        {
            var record = Record(int.Parse(un, CultureInfo.InvariantCulture));
            record.Classes = classes;
            return record;
        }

        if (frame.Function == ZWaveFunctions.ApplicationCommandHandler && v.TryGetValue("reportNode", out var rn))
        {
            var record = Record(int.Parse(rn, CultureInfo.InvariantCulture));
            record.Last = v["reportText"];
            if (v.TryGetValue("reportIds", out var ids)) record.Ids = ids;
            return record;
        }

        return null;
    }

    private int? TakeExpected(byte function)
    {
        var kept = new Queue<(byte Function, int Node)>();
        int? found = null;
        while (_expected.Count > 0)
        {
            var item = _expected.Dequeue();
            if (found is null && item.Function == function) found = item.Node;
            else kept.Enqueue(item);
        }

        foreach (var item in kept) _expected.Enqueue(item);
        return found;
    }

    private ZWaveNodeRecord Record(int node)
    {
        if (!_nodes.TryGetValue(node, out var record)) _nodes[node] = record = new ZWaveNodeRecord { Node = node };
        return record;
    }
}
