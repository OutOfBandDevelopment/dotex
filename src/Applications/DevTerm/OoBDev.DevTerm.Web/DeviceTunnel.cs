using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using OoBDev.DevTerm.Configuration;
using OoBDev.System.IO;

namespace OoBDev.DevTerm.Web;

/// <summary>
/// <c>/ws/device/{name}</c>: the raw bytes of a saved connection of this host. The host opens the named connection's device
/// adapter, each binary frame received is written to the device, and each read from the device is sent as one binary frame.
/// A remote DevTerm uses it as a transport (<c>RemoteDeviceAdapter</c>); the device stays attached here. A read-only token is refused.
/// </summary>
internal static class DeviceTunnel
{
    public static async Task HandleAsync(HttpContext context, ConnectionManager connections, string name)
    {
        if (!context.WebSockets.IsWebSocketRequest)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }

        if (context.Items.ContainsKey(AccessTokenMiddleware.ReadOnlyItem))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return;
        }

        var options = connections.ProjectOptions(name);
        if (options is null)
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            return;
        }

        var services = new ServiceCollection();
        services.AddDevTermFrontEnd(options);
        await using var provider = services.BuildServiceProvider();
        var adapter = provider.GetRequiredService<IDeviceAdapter>();
        var abort = context.RequestAborted;

        try
        {
            await adapter.OpenAsync(abort).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            context.Response.StatusCode = StatusCodes.Status502BadGateway;
            await context.Response.WriteAsync($"The device could not be opened: {ex.Message}", abort).ConfigureAwait(false);
            return;
        }

        using var socket = await context.WebSockets.AcceptWebSocketAsync().ConfigureAwait(false);
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(abort);
        var toSocket = PumpToSocketAsync(adapter.Input, socket, linked.Token);
        try
        {
            await PumpToDeviceAsync(socket, adapter.Output, linked.Token).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is OperationCanceledException or WebSocketException)
        {
            // The remote client went away.
        }
        finally
        {
            await linked.CancelAsync().ConfigureAwait(false);
            try
            {
                await toSocket.ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is OperationCanceledException or WebSocketException)
            {
                // Same.
            }

            await adapter.CloseAsync(CancellationToken.None).ConfigureAwait(false);
            await adapter.DisposeAsync().ConfigureAwait(false);
        }
    }

    private static async Task PumpToSocketAsync(PipeReader input, WebSocket socket, CancellationToken cancellationToken)
    {
        while (socket.State == WebSocketState.Open)
        {
            var result = await input.ReadAsync(cancellationToken).ConfigureAwait(false);
            foreach (var segment in result.Buffer)
            {
                await socket.SendAsync(segment, WebSocketMessageType.Binary, true, cancellationToken).ConfigureAwait(false);
            }

            input.AdvanceTo(result.Buffer.End);
            if (result.IsCompleted)
            {
                if (socket.State == WebSocketState.Open)
                {
                    await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "device closed", CancellationToken.None).ConfigureAwait(false);
                }

                return;
            }
        }
    }

    private static async Task PumpToDeviceAsync(WebSocket socket, PipeWriter output, CancellationToken cancellationToken)
    {
        var buffer = new byte[16 * 1024];
        while (socket.State == WebSocketState.Open)
        {
            var result = await socket.ReceiveAsync(buffer.AsMemory(), cancellationToken).ConfigureAwait(false);
            if (result.MessageType == WebSocketMessageType.Close)
            {
                return;
            }

            if (result.MessageType == WebSocketMessageType.Binary && result.Count > 0)
            {
                await output.WriteAsync(buffer.AsMemory(0, result.Count), cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
