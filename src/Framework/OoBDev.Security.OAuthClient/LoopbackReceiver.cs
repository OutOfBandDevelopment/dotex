using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.Security.OAuthClient;

/// <summary>Listens on <c>127.0.0.1</c> and an ephemeral port for the browser's redirect (RFC 8252, section 7.3).</summary>
public sealed class LoopbackReceiver : IDisposable
{
    private const int _maxRequestBytes = 8192;
    private readonly TcpListener _listener;

    /// <summary>Starts listening on a free port.</summary>
    public LoopbackReceiver()
    {
        _listener = new TcpListener(IPAddress.Loopback, 0);
        _listener.Start();
        RedirectUri = new Uri($"http://127.0.0.1:{((IPEndPoint)_listener.LocalEndpoint).Port}/callback");
    }

    /// <summary>The redirect address to register in the authorization request.</summary>
    public Uri RedirectUri { get; }

    /// <summary>Waits for the redirect and returns the authorization code. A request with another path or a wrong <c>state</c> is answered and ignored, so a stray request cannot end or hijack the sign-in.</summary>
    /// <param name="state">The <c>state</c> sent in the authorization request.</param>
    /// <param name="timeout">How long to wait.</param>
    /// <param name="cancellationToken">Cancels the wait.</param>
    /// <returns>The code.</returns>
    /// <exception cref="OAuthException">The provider returned an error, or the wait timed out.</exception>
    public async Task<string> WaitForCodeAsync(string state, TimeSpan timeout, CancellationToken cancellationToken = default)
    {
        using var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutSource.CancelAfter(timeout);
        try
        {
            while (true)
            {
                using var client = await _listener.AcceptTcpClientAsync(timeoutSource.Token).ConfigureAwait(false);
                var stream = client.GetStream();
                var target = await ReadTargetAsync(stream, timeoutSource.Token).ConfigureAwait(false);
                var query = ParseQuery(target, out var path);
                if (!string.Equals(path, "/callback", StringComparison.Ordinal))
                {
                    await RespondAsync(stream, 404, "Not found.", timeoutSource.Token).ConfigureAwait(false);
                    continue;
                }

                if (!query.TryGetValue("state", out var received) || !string.Equals(received, state, StringComparison.Ordinal))
                {
                    await RespondAsync(stream, 400, "The sign-in answer did not match this request.", timeoutSource.Token).ConfigureAwait(false);
                    continue;
                }

                if (query.TryGetValue("error", out var error))
                {
                    await RespondAsync(stream, 400, "Sign-in was not completed. You can close this tab.", timeoutSource.Token).ConfigureAwait(false);
                    throw new OAuthException(error, query.GetValueOrDefault("error_description"));
                }

                if (!query.TryGetValue("code", out var code) || code.Length == 0)
                {
                    await RespondAsync(stream, 400, "The sign-in answer had no code.", timeoutSource.Token).ConfigureAwait(false);
                    continue;
                }

                await RespondAsync(stream, 200, "Signed in. You can close this tab.", timeoutSource.Token).ConfigureAwait(false);
                return code;
            }
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new OAuthException("timeout", "The browser did not return to the application in time.");
        }
    }

    /// <summary>Stops listening.</summary>
    public void Dispose() => _listener.Stop();

    private static async Task<string> ReadTargetAsync(NetworkStream stream, CancellationToken cancellationToken)
    {
        var buffer = new byte[_maxRequestBytes];
        var length = 0;
        while (length < buffer.Length)
        {
            var read = await stream.ReadAsync(buffer.AsMemory(length), cancellationToken).ConfigureAwait(false);
            if (read == 0)
                break;
            length += read;
            if (Encoding.ASCII.GetString(buffer, 0, length).Contains("\r\n", StringComparison.Ordinal))
                break;
        }

        var line = Encoding.ASCII.GetString(buffer, 0, length).Split("\r\n")[0];
        var parts = line.Split(' ');
        return parts.Length >= 2 && string.Equals(parts[0], "GET", StringComparison.Ordinal) ? parts[1] : string.Empty;
    }

    private static Dictionary<string, string> ParseQuery(string target, out string path)
    {
        var result = new Dictionary<string, string>(StringComparer.Ordinal);
        var split = target.IndexOf('?', StringComparison.Ordinal);
        path = split < 0 ? target : target[..split];
        if (split < 0)
            return result;
        foreach (var pair in target[(split + 1)..].Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var equals = pair.IndexOf('=', StringComparison.Ordinal);
            var key = Uri.UnescapeDataString((equals < 0 ? pair : pair[..equals]).Replace('+', ' '));
            var value = equals < 0 ? string.Empty : Uri.UnescapeDataString(pair[(equals + 1)..].Replace('+', ' '));
            result[key] = value;
        }

        return result;
    }

    private static async Task RespondAsync(NetworkStream stream, int status, string message, CancellationToken cancellationToken)
    {
        var reason = status == 200 ? "OK" : status == 404 ? "Not Found" : "Bad Request";
        var body = Encoding.UTF8.GetBytes($"<!DOCTYPE html><html><body><p>{WebUtility.HtmlEncode(message)}</p></body></html>");
        var head = Encoding.ASCII.GetBytes($"HTTP/1.1 {status} {reason}\r\nContent-Type: text/html; charset=utf-8\r\nContent-Length: {body.Length}\r\nConnection: close\r\n\r\n");
        await stream.WriteAsync(head, cancellationToken).ConfigureAwait(false);
        await stream.WriteAsync(body, cancellationToken).ConfigureAwait(false);
        await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
    }
}
