using Microsoft.Extensions.Logging;

namespace OoBDev.System.IO.Usb.Bridge;

internal static partial class UsbBridgeLog
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Listening on {Address}:{Port} for {Device}")]
    public static partial void Listening(this ILogger logger, string address, int port, string device);

    [LoggerMessage(Level = LogLevel.Information, Message = "Client {Client} connected")]
    public static partial void ClientConnected(this ILogger logger, string client);

    [LoggerMessage(Level = LogLevel.Information, Message = "Client {Client} disconnected")]
    public static partial void ClientDisconnected(this ILogger logger, string client);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Refused {Client}: another client has the device")]
    public static partial void ClientRefused(this ILogger logger, string client);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Could not open the device: {Reason}")]
    public static partial void OpenFailed(this ILogger logger, string reason);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Client connection ended: {Reason}")]
    public static partial void SessionFailed(this ILogger logger, string reason);
}
