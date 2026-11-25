using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.WebSockets;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using OoBDev.System.IO.Pipelines;

namespace OoBDev.DevTerm.Remote;

/// <summary>
/// A device that stays attached to another machine: exchanges the host's shared key for a session token (<c>POST /api/token</c>),
/// then opens the host's <c>/ws/device/{connection}</c> tunnel and moves the device's bytes both ways.
/// </summary>
public sealed class RemoteDeviceAdapter(IOptions<RemoteDeviceOptions> options, HttpMessageHandler? handler = null) : StreamDeviceAdapter
{
    private readonly RemoteDeviceOptions _options = options.Value;

    /// <inheritdoc />
    public override string Type => "remote";

    /// <inheritdoc />
    public override string Path => $"{_options.BaseUrl.TrimEnd('/')}/{_options.Connection}";

    /// <inheritdoc />
    protected override async Task<Stream?> ConnectAsync(CancellationToken cancellationToken)
    {
        if (!Uri.TryCreate(_options.BaseUrl, UriKind.Absolute, out var baseUri) || string.IsNullOrWhiteSpace(_options.Connection))
        {
            throw new InvalidOperationException("A remote DevTerm connection needs the host address and a connection name.");
        }

        var token = await ExchangeAsync(baseUri, cancellationToken).ConfigureAwait(false);
        var socketUri = new UriBuilder(baseUri)
        {
            Scheme = baseUri.Scheme == Uri.UriSchemeHttps ? "wss" : "ws",
            Path = $"/ws/device/{Uri.EscapeDataString(_options.Connection)}",
        }.Uri;

        var socket = new ClientWebSocket();
        socket.Options.SetRequestHeader("Authorization", "Bearer " + token);
        try
        {
            await socket.ConnectAsync(socketUri, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception)
        {
            socket.Dispose();
            throw;
        }

        return new WebSocketStream(socket);
    }

    private async Task<string> ExchangeAsync(Uri baseUri, CancellationToken cancellationToken)
    {
        using var http = handler is null ? new HttpClient() : new HttpClient(handler, disposeHandler: false);
        using var request = new HttpRequestMessage(HttpMethod.Post, new Uri(baseUri, "/api/token"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.Key);
        using var response = await http.SendAsync(request, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            throw new IOException($"The DevTerm host refused the key ({(int)response.StatusCode}).");
        }

        var body = await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken).ConfigureAwait(false);
        return body?.AccessToken ?? throw new IOException("The DevTerm host returned no token.");
    }

    private sealed record TokenResponse([property: JsonPropertyName("access_token")] string AccessToken);
}
