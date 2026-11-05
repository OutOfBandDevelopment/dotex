using System.Net;
using Microsoft.Extensions.Logging;

namespace OoBDev.System.IO.Ports.Bridge;

internal static partial class BridgeLog
{
    [LoggerMessage(Level = LogLevel.Information, Message = "{Bridge}: listening on {Endpoint} ({Protocol})")]
    public static partial void Listening(this ILogger logger, string bridge, EndPoint endpoint, BridgeProtocol protocol);

    [LoggerMessage(Level = LogLevel.Warning, Message = "{Bridge}: mDNS advertisement failed; bridge keeps running without it")]
    public static partial void AdvertisementFailed(this ILogger logger, Exception exception, string bridge);

    [LoggerMessage(Level = LogLevel.Debug, Message = "{Bridge}: dropped a connection while accepting it")]
    public static partial void AcceptDropped(this ILogger logger, Exception exception, string bridge);

    [LoggerMessage(Level = LogLevel.Information, Message = "{Bridge}: replacing client {Old} with {New}")]
    public static partial void ReplacingClient(this ILogger logger, string bridge, EndPoint? old, EndPoint? @new);

    [LoggerMessage(Level = LogLevel.Warning, Message = "{Bridge}: rejected extra client {Remote}; port is held by {Holder}")]
    public static partial void RejectedClient(this ILogger logger, string bridge, EndPoint? remote, string holder);

    [LoggerMessage(Level = LogLevel.Information, Message = "{Bridge}: client connected {Remote}")]
    public static partial void ClientConnected(this ILogger logger, string bridge, EndPoint? remote);

    [LoggerMessage(Level = LogLevel.Debug, Message = "{Bridge}: client connection error")]
    public static partial void ClientError(this ILogger logger, Exception exception, string bridge);

    [LoggerMessage(Level = LogLevel.Information, Message = "{Bridge}: client disconnected {Remote}")]
    public static partial void ClientDisconnected(this ILogger logger, string bridge, EndPoint? remote);

    [LoggerMessage(Level = LogLevel.Debug, Message = "{Bridge}: serial port closed, dropped {Count} bytes from client")]
    public static partial void SerialClosedDropped(this ILogger logger, string bridge, int count);

    [LoggerMessage(Level = LogLevel.Warning, Message = "{Bridge}: serial write failed, dropped {Count} bytes")]
    public static partial void SerialWriteFailed(this ILogger logger, Exception exception, string bridge, int count);

    [LoggerMessage(Level = LogLevel.Information, Message = "{Bridge}: serial port open ({Settings})")]
    public static partial void SerialOpen(this ILogger logger, string bridge, BridgeOptions settings);

    [LoggerMessage(Level = LogLevel.Warning, Message = "{Bridge}: serial port unavailable ({Message}); retrying in {Seconds:0.#}s")]
    public static partial void SerialUnavailable(this ILogger logger, string bridge, string message, double seconds);

    [LoggerMessage(Level = LogLevel.Debug, Message = "{Bridge}: dropping client after write failure")]
    public static partial void ClientDropped(this ILogger logger, Exception exception, string bridge);

    [LoggerMessage(Level = LogLevel.Warning, Message = "No bridges configured. Run 'ser2net-win config' to add one.")]
    public static partial void NoBridges(this ILogger logger);

    [LoggerMessage(Level = LogLevel.Error, Message = "{Bridge}: bridge failed and was stopped (other bridges keep running)")]
    public static partial void BridgeFailed(this ILogger logger, Exception exception, string bridge);
}
