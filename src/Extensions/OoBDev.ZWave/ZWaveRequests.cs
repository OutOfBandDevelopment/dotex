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
}
