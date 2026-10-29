using System;

namespace OoBDev.ZWave;

/// <summary>
/// Builders for the Serial API requests, each a data frame ready to transmit. Pure: no state, no I/O. The controller expects an
/// ACK for each reply, which the consumer sends as <see cref="ZWaveFrame.Ack"/>.
/// </summary>
public static class ZWaveRequests
{
    /// <summary>Transmit options for SendData: ACK plus auto route.</summary>
    public const byte TransmitOptions = 0x25;

    /// <summary>
    /// Builds a request for <paramref name="function"/>.
    /// </summary>
    /// <param name="function">The Serial API function id.</param>
    /// <param name="payload">The function's parameters.</param>
    /// <returns>The request frame.</returns>
    public static ZWaveFrame For(byte function, params byte[] payload) =>
        ZWaveFrame.Create(ZWaveFrame.Request, function, payload);

    /// <summary>The GetVersion request.</summary>
    /// <returns>The request frame.</returns>
    public static ZWaveFrame GetVersion() => For(ZWaveFunctions.GetVersion);

    /// <summary>The MemoryGetId request (home id and own node id).</summary>
    /// <returns>The request frame.</returns>
    public static ZWaveFrame MemoryGetId() => For(ZWaveFunctions.MemoryGetId);

    /// <summary>The GetInitData request (node bitmap).</summary>
    /// <returns>The request frame.</returns>
    public static ZWaveFrame GetInitData() => For(ZWaveFunctions.GetInitData);

    /// <summary>The GetNodeProtocolInfo request for <paramref name="node"/>.</summary>
    /// <param name="node">The node to ask about.</param>
    /// <returns>The request frame.</returns>
    public static ZWaveFrame GetNodeProtocolInfo(byte node) => For(ZWaveFunctions.GetNodeProtocolInfo, node);

    /// <summary>
    /// Wraps a command class payload (<c>class command params...</c>) for <paramref name="node"/> in a SendData request.
    /// </summary>
    /// <param name="node">The destination node.</param>
    /// <param name="command">The command class payload.</param>
    /// <param name="callbackId">The id the controller repeats in its transmit callback.</param>
    /// <returns>The request frame.</returns>
    public static ZWaveFrame SendData(byte node, ReadOnlySpan<byte> command, byte callbackId) =>
        For(ZWaveFunctions.SendData, [node, (byte)command.Length, .. command, TransmitOptions, callbackId]);

    /// <summary>Basic Get for <paramref name="node"/>.</summary>
    /// <param name="node">The destination node.</param>
    /// <param name="callbackId">The id the controller repeats in its transmit callback.</param>
    /// <returns>The request frame.</returns>
    public static ZWaveFrame BasicGet(byte node, byte callbackId) => SendData(node, [0x20, 0x02], callbackId);

    /// <summary>Binary Switch Set for <paramref name="node"/>.</summary>
    /// <param name="node">The destination node.</param>
    /// <param name="on">Whether to switch on.</param>
    /// <param name="callbackId">The id the controller repeats in its transmit callback.</param>
    /// <returns>The request frame.</returns>
    public static ZWaveFrame SwitchSet(byte node, bool on, byte callbackId) =>
        SendData(node, [0x25, 0x01, on ? (byte)0xFF : (byte)0x00], callbackId);

    /// <summary>Binary Switch Get for <paramref name="node"/>.</summary>
    /// <param name="node">The destination node.</param>
    /// <param name="callbackId">The id the controller repeats in its transmit callback.</param>
    /// <returns>The request frame.</returns>
    public static ZWaveFrame SwitchGet(byte node, byte callbackId) => SendData(node, [0x25, 0x02], callbackId);

    /// <summary>Multilevel Switch Set for <paramref name="node"/>.</summary>
    /// <param name="node">The destination node.</param>
    /// <param name="level">The level, 0 to 99 (0xFF restores the last level).</param>
    /// <param name="callbackId">The id the controller repeats in its transmit callback.</param>
    /// <returns>The request frame.</returns>
    public static ZWaveFrame LevelSet(byte node, byte level, byte callbackId) => SendData(node, [0x26, 0x01, level], callbackId);

    /// <summary>Include mode: any node, with network-wide inclusion.</summary>
    public const byte ModeAny = 0x01;

    /// <summary>Stop include, exclude or learn mode.</summary>
    public const byte ModeStop = 0x05;

    /// <summary>Starts including a node; the callbacks report progress.</summary>
    /// <param name="callbackId">The id the controller repeats in its callbacks.</param>
    /// <returns>The request frame.</returns>
    public static ZWaveFrame AddNode(byte callbackId) => For(ZWaveFunctions.AddNodeToNetwork, ModeAny, callbackId);

    /// <summary>Starts excluding a node; the callbacks report progress.</summary>
    /// <param name="callbackId">The id the controller repeats in its callbacks.</param>
    /// <returns>The request frame.</returns>
    public static ZWaveFrame RemoveNode(byte callbackId) => For(ZWaveFunctions.RemoveNodeFromNetwork, ModeAny, callbackId);

    /// <summary>Stops include mode.</summary>
    /// <param name="callbackId">The id the controller repeats in its callbacks.</param>
    /// <returns>The request frame.</returns>
    public static ZWaveFrame StopAddNode(byte callbackId) => For(ZWaveFunctions.AddNodeToNetwork, ModeStop, callbackId);

    /// <summary>Stops exclude mode.</summary>
    /// <param name="callbackId">The id the controller repeats in its callbacks.</param>
    /// <returns>The request frame.</returns>
    public static ZWaveFrame StopRemoveNode(byte callbackId) => For(ZWaveFunctions.RemoveNodeFromNetwork, ModeStop, callbackId);

    /// <summary>Resets the controller to defaults. Destroys the network: every node is forgotten.</summary>
    /// <param name="callbackId">The id the controller repeats in its callback.</param>
    /// <returns>The request frame.</returns>
    public static ZWaveFrame SetDefault(byte callbackId) => For(ZWaveFunctions.SetDefault, callbackId);

    /// <summary>Starts or stops learn mode.</summary>
    /// <param name="enable">Whether to enter learn mode.</param>
    /// <param name="callbackId">The id the controller repeats in its callbacks.</param>
    /// <returns>The request frame.</returns>
    public static ZWaveFrame SetLearnMode(bool enable, byte callbackId) => For(ZWaveFunctions.SetLearnMode, enable ? (byte)0x01 : (byte)0x00, callbackId);

    /// <summary>Asks <paramref name="node"/> to refresh its neighbour list.</summary>
    /// <param name="node">The node.</param>
    /// <param name="callbackId">The id the controller repeats in its callbacks.</param>
    /// <returns>The request frame.</returns>
    public static ZWaveFrame NeighborUpdate(byte node, byte callbackId) => For(ZWaveFunctions.RequestNodeNeighborUpdate, node, callbackId);

    /// <summary>Asks whether the controller considers <paramref name="node"/> failed.</summary>
    /// <param name="node">The node.</param>
    /// <returns>The request frame.</returns>
    public static ZWaveFrame IsFailedNode(byte node) => For(ZWaveFunctions.IsFailedNode, node);

    /// <summary>Removes a node the controller considers failed.</summary>
    /// <param name="node">The node.</param>
    /// <param name="callbackId">The id the controller repeats in its callbacks.</param>
    /// <returns>The request frame.</returns>
    public static ZWaveFrame RemoveFailedNode(byte node, byte callbackId) => For(ZWaveFunctions.RemoveFailedNode, node, callbackId);

    /// <summary>Asks <paramref name="node"/> for its node information frame.</summary>
    /// <param name="node">The node.</param>
    /// <returns>The request frame.</returns>
    public static ZWaveFrame RequestNodeInfo(byte node) => For(ZWaveFunctions.RequestNodeInfo, node);

    /// <summary>Restarts the Serial API application.</summary>
    /// <returns>The request frame.</returns>
    public static ZWaveFrame SoftReset() => For(ZWaveFunctions.SoftReset);
}
