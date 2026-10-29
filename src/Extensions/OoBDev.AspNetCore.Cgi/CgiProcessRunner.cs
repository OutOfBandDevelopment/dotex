#pragma warning disable MA0048 // related small types share a file
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.AspNetCore.Cgi;

/// <summary>
/// What to run.
/// </summary>
/// <param name="Script">The resolved script.</param>
/// <param name="Environment">The complete child environment (meta-variables).</param>
/// <param name="Body">The request body, or null for none.</param>
public record CgiProcessRequest(CgiScript Script, IDictionary<string, string> Environment, Stream? Body);

/// <summary>
/// A running CGI program whose header block was read. Disposing it kills the process tree when it is still running.
/// </summary>
/// <param name="StatusCode">The HTTP status code.</param>
/// <param name="Headers">The response headers except <c>Status</c>.</param>
/// <param name="Body">The rest of the program output.</param>
/// <param name="onDispose">Cleanup run on dispose.</param>
public sealed class CgiResult(int StatusCode, IList<KeyValuePair<string, string>> Headers, Stream Body, Func<ValueTask> onDispose) : IAsyncDisposable
{
    /// <summary>
    /// The HTTP status code.
    /// </summary>
    public int StatusCode { get; } = StatusCode;

    /// <summary>
    /// The response headers except <c>Status</c>.
    /// </summary>
    public IList<KeyValuePair<string, string>> Headers { get; } = Headers;

    /// <summary>
    /// The rest of the program output.
    /// </summary>
    public Stream Body { get; } = Body;

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await Body.DisposeAsync().ConfigureAwait(false);
        await onDispose().ConfigureAwait(false);
    }
}

/// <summary>
/// Starts CGI programs.
/// </summary>
public interface ICgiProcessRunner
{
    /// <summary>
    /// Starts the program and reads its header block.
    /// </summary>
    /// <param name="request">What to run.</param>
    /// <param name="cancellationToken">Cancels the run and kills the process.</param>
    /// <returns>The parsed header block and the body stream.</returns>
    /// <exception cref="CgiException">When the program cannot run or answers badly.</exception>
    Task<CgiResult> StartAsync(CgiProcessRequest request, CancellationToken cancellationToken);
}

/// <summary>
/// Runs scripts as child processes with a cleared environment and no shell.
/// </summary>
public sealed partial class CgiProcessRunner : ICgiProcessRunner, IDisposable
{
    private const int _maxHeaderBytes = 16 * 1024;

    private readonly CgiOptions _options;
    private readonly ILogger<CgiProcessRunner> _logger;
    private readonly SemaphoreSlim _slots;

    /// <summary>
    /// Creates the runner.
    /// </summary>
    /// <param name="options">The CGI options.</param>
    /// <param name="logger">The logger.</param>
    public CgiProcessRunner(IOptions<CgiOptions> options, ILogger<CgiProcessRunner> logger)
    {
        _options = options.Value;
        _logger = logger;
        _slots = new SemaphoreSlim(_options.MaxConcurrentProcesses, _options.MaxConcurrentProcesses);
    }

    /// <inheritdoc />
    public void Dispose() => _slots.Dispose();

    /// <inheritdoc />
    public async Task<CgiResult> StartAsync(CgiProcessRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!await _slots.WaitAsync(0, CancellationToken.None).ConfigureAwait(false))
            throw new CgiBusyException();

        var timeout = new CancellationTokenSource(_options.Timeout);
        var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeout.Token);
        Process? process = null;
        var handedOver = false;
        try
        {
            process = Start(request);
            var started = process;
            using var killer = linked.Token.Register(() => Kill(started));

            var stderr = DrainStderrAsync(process);
            var stdin = FeedStdinAsync(process, request.Body);

            Stream stdout = process.StandardOutput.BaseStream;
            var (head, leftover) = await ReadHeadAsync(stdout, linked.Token).ConfigureAwait(false);
            if (head is null)
                await ThrowNoHeaderAsync(process).ConfigureAwait(false);

            var (status, headers) = ParseHead(head, request.Script.ScriptName);
            var body = new PrefixedStream(leftover, stdout);

            handedOver = true;
            return new CgiResult(status, headers, body, () => new ValueTask(CompleteAsync(started, stderr, stdin, linked, timeout)));
        }
        catch (OperationCanceledException) when (timeout.IsCancellationRequested)
        {
            throw new CgiTimeoutException();
        }
        finally
        {
            if (!handedOver)
            {
                if (process != null)
                {
                    Kill(process);
                    process.Dispose();
                }
                linked.Dispose();
                timeout.Dispose();
                _slots.Release();
            }
        }
    }

    private static async Task ThrowNoHeaderAsync(Process process)
    {
        await process.WaitForExitAsync(CancellationToken.None).ConfigureAwait(false);
        if (process.ExitCode != 0)
            throw new CgiProcessFailedException($"The program exited with code {process.ExitCode.ToString(CultureInfo.InvariantCulture)} and wrote nothing.");
        throw new CgiProtocolException("The program wrote no header block.");
    }

    private async Task CompleteAsync(Process started, Task stderr, Task stdin, CancellationTokenSource linked, CancellationTokenSource timeout)
    {
        try
        {
            Kill(started);
            await Task.WhenAll(stderr, stdin).ConfigureAwait(false);
        }
        catch (IOException)
        {
            // the pipe closed because the process was killed
        }
        finally
        {
            started.Dispose();
            linked.Dispose();
            timeout.Dispose();
            _slots.Release();
        }
    }

    private Process Start(CgiProcessRequest request)
    {
        var script = request.Script;
        var info = new ProcessStartInfo
        {
            FileName = script.Executable.Length > 0 ? script.Executable : script.ScriptPath,
            WorkingDirectory = Path.GetDirectoryName(script.ScriptPath)!,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };
        if (script.Executable.Length > 0)
            info.ArgumentList.Add(script.ScriptPath);

        info.Environment.Clear();
        foreach (var name in _options.InheritedEnvironmentVariables)
        {
            var value = Environment.GetEnvironmentVariable(name);
            if (value != null)
                info.Environment[name] = value;
        }
        foreach (var (key, value) in request.Environment)
            info.Environment[key] = value;

        try
        {
            return Process.Start(info) ?? throw new CgiProcessFailedException("The program did not start.");
        }
        catch (Win32Exception ex)
        {
            LogStartFailed(ex, script.ScriptName);
            throw new CgiProcessFailedException("The program could not start.");
        }
    }

    private static async Task FeedStdinAsync(Process process, Stream? body)
    {
        try
        {
            if (body != null)
                await body.CopyToAsync(process.StandardInput.BaseStream).ConfigureAwait(false);
        }
        catch (IOException)
        {
            // the program closed its input early; that is allowed
        }
        finally
        {
            try
            {
                process.StandardInput.Close();
            }
            catch (IOException)
            {
                // see above
            }
        }
    }

    private async Task DrainStderrAsync(Process process)
    {
        var kept = new MemoryStream();
        var buffer = new byte[4096];
        var stream = process.StandardError.BaseStream;
        try
        {
            int read;
            while ((read = await stream.ReadAsync(buffer).ConfigureAwait(false)) > 0)
            {
                var room = _options.MaxStderrBytes - (int)kept.Length;
                if (room > 0)
                    kept.Write(buffer, 0, Math.Min(room, read));
            }
        }
        catch (IOException)
        {
            // killed while reading
        }
        if (kept.Length > 0)
            LogStderr(Encoding.UTF8.GetString(kept.ToArray()));
    }

    private static async Task<(string? Head, byte[] Leftover)> ReadHeadAsync(Stream stdout, CancellationToken cancellationToken)
    {
        var collected = new List<byte>();
        var buffer = new byte[1024];
        while (collected.Count <= _maxHeaderBytes)
        {
            var read = await stdout.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
            if (read == 0)
            {
                if (collected.Count == 0)
                    return (null, []);
                throw new CgiProtocolException("The header block was not ended by a blank line.");
            }
            collected.AddRange(buffer.AsSpan(0, read).ToArray());

            var span = CollectionsMarshal.AsSpan(collected);
            var (end, length) = FindBlankLine(span);
            if (end >= 0)
            {
                var head = Encoding.Latin1.GetString(span[..end]);
                return (head, span[(end + length)..].ToArray());
            }
        }
        throw new CgiProtocolException("The header block is too large.");
    }

    private static (int End, int Length) FindBlankLine(ReadOnlySpan<byte> data)
    {
        for (var i = 0; i < data.Length; i++)
        {
            if (data[i] != (byte)'\n')
                continue;
            if (i + 1 < data.Length && data[i + 1] == (byte)'\n')
                return (i, 2);
            if (i + 2 < data.Length && data[i + 1] == (byte)'\r' && data[i + 2] == (byte)'\n')
                return (i, 3);
        }
        return (-1, 0);
    }

    private static (int Status, IList<KeyValuePair<string, string>> Headers) ParseHead(string head, string scriptName)
    {
        var headers = new List<KeyValuePair<string, string>>();
        int? status = null;
        var hasContentType = false;
        var hasLocation = false;

        foreach (var raw in head.Split('\n'))
        {
            var line = raw.EndsWith('\r') ? raw[..^1] : raw;
            if (line.Contains('\r', StringComparison.Ordinal) || line.Contains('\0', StringComparison.Ordinal))
                throw new CgiProtocolException($"{scriptName} wrote a control character in a header.");

            var colon = line.IndexOf(':', StringComparison.Ordinal);
            if (colon <= 0)
                throw new CgiProtocolException($"{scriptName} wrote a malformed header line.");

            var name = line[..colon];
            var value = line[(colon + 1)..].Trim();
            if (name.Contains(' ', StringComparison.Ordinal) || name.Contains('\t', StringComparison.Ordinal))
                throw new CgiProtocolException($"{scriptName} wrote an invalid header name.");

            if (name.Equals("Status", StringComparison.OrdinalIgnoreCase))
            {
                var digits = value.Split(' ', 2)[0];
                if (!int.TryParse(digits, NumberStyles.None, CultureInfo.InvariantCulture, out var code) || code is < 100 or > 599)
                    throw new CgiProtocolException($"{scriptName} wrote an invalid Status header.");
                status = code;
                continue;
            }

            hasContentType |= name.Equals("Content-Type", StringComparison.OrdinalIgnoreCase);
            hasLocation |= name.Equals("Location", StringComparison.OrdinalIgnoreCase);
            headers.Add(new KeyValuePair<string, string>(name, value));
        }

        if (!hasContentType && !hasLocation)
            throw new CgiProtocolException($"{scriptName} wrote neither Content-Type nor Location.");
        if (hasLocation && !hasContentType && status is null)
            status = 302;

        return (status ?? 200, headers);
    }

    private static void Kill(Process process)
    {
        try
        {
            if (!process.HasExited)
                process.Kill(entireProcessTree: true);
        }
        catch (InvalidOperationException)
        {
            // already gone
        }
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "CGI script {ScriptName} could not start.")]
    private partial void LogStartFailed(Exception exception, string scriptName);

    [LoggerMessage(Level = LogLevel.Warning, Message = "CGI stderr: {Text}")]
    private partial void LogStderr(string text);

    private sealed class PrefixedStream(byte[] prefix, Stream inner) : Stream
    {
        private int _offset;

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public override void Flush() { }
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        public override int Read(byte[] buffer, int offset, int count) => Read(buffer.AsSpan(offset, count));

        public override int Read(Span<byte> buffer)
        {
            if (_offset < prefix.Length)
                return TakePrefix(buffer);
            return inner.Read(buffer);
        }

        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            if (_offset < prefix.Length)
                return new ValueTask<int>(TakePrefix(buffer.Span));
            return inner.ReadAsync(buffer, cancellationToken);
        }

        private int TakePrefix(Span<byte> buffer)
        {
            var count = Math.Min(buffer.Length, prefix.Length - _offset);
            prefix.AsSpan(_offset, count).CopyTo(buffer);
            _offset += count;
            return count;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                inner.Dispose();
            base.Dispose(disposing);
        }
    }
}
