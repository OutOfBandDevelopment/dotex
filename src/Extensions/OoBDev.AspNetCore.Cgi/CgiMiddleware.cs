#pragma warning disable MA0004 // "await using" declarations cannot take ConfigureAwait
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OoBDev.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace OoBDev.AspNetCore.Cgi;

/// <summary>
/// Runs scripts under <see cref="CgiOptions.ScriptRoot"/> as CGI/1.1 programs (RFC 3875) for requests under <see cref="CgiOptions.Prefix"/>.
/// </summary>
public partial class CgiMiddleware(
    RequestDelegate next,
    IOptions<CgiOptions> options,
    ICgiPathResolver resolver,
    ICgiProcessRunner runner,
    ILogger<CgiMiddleware> logger
    )
{
    private static readonly string[] _skippedHeaders = ["Proxy", "Authorization", "Content-Type", "Content-Length"];

    /// <summary>
    /// Handles the request.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns>A task that completes when the response is written.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var settings = options.Value;
        var prefix = new PathString(settings.Prefix);
        if (!context.Request.Path.StartsWithSegments(prefix, StringComparison.OrdinalIgnoreCase, out var remaining))
        {
            await next(context).ConfigureAwait(false);
            return;
        }

        try
        {
            await RunAsync(context, settings, prefix, remaining).ConfigureAwait(false);
        }
        catch (CgiException ex) when (!context.Response.HasStarted)
        {
            LogFailed(ex, context.Request.Path.Value ?? string.Empty);
            await WriteProblemAsync(context, ex).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is CgiException or IOException or OperationCanceledException && context.Response.HasStarted)
        {
            LogAborted(ex, context.Request.Path.Value ?? string.Empty);
            context.Abort();
        }
    }

    private async Task RunAsync(HttpContext context, CgiOptions settings, PathString prefix, PathString remaining)
    {
        Authorize(context, settings.RequiredRights, settings.AllowAnonymous);
        var script = resolver.Resolve(remaining.Value ?? string.Empty);

        if (settings.ScriptRights.TryGetValue(script.ScriptName, out var scriptRights))
            Authorize(context, scriptRights, settings.AllowAnonymous);

        await using var body = await BufferBodyAsync(context, settings).ConfigureAwait(false);
        var environment = BuildEnvironment(context, settings, prefix, script, body.Length);
        body.Position = 0;

        await using var result = await runner.StartAsync(new CgiProcessRequest(script, environment, body), context.RequestAborted).ConfigureAwait(false);

        context.Response.StatusCode = result.StatusCode;
        foreach (var (name, value) in result.Headers)
        {
            if (name.Equals("Content-Type", StringComparison.OrdinalIgnoreCase))
                context.Response.ContentType = value;
            else
                context.Response.Headers.Append(name, value);
        }

        var buffer = new byte[16 * 1024];
        long total = 0;
        int read;
        while ((read = await result.Body.ReadAsync(buffer, context.RequestAborted).ConfigureAwait(false)) > 0)
        {
            total += read;
            if (settings.MaxResponseBytes is { } limit && total > limit)
                throw new CgiResponseTooLargeException();
            await context.Response.Body.WriteAsync(buffer.AsMemory(0, read), context.RequestAborted).ConfigureAwait(false);
        }
    }

    private static void Authorize(HttpContext context, IList<string> rights, bool allowAnonymous)
    {
        if (allowAnonymous && rights.Count == 0)
            return;

        var checker = context.RequestServices?.GetService<IApplicationRightChecker>() ?? new ApplicationRightChecker();
        switch (checker.Check(context.User, [.. rights]))
        {
            case RightCheckResult.Unauthenticated:
                throw new CgiException(401, "Authentication is required.");
            case RightCheckResult.Forbidden:
                throw new CgiException(403, "A required application right is missing.");
        }
    }

    private static async Task<MemoryStream> BufferBodyAsync(HttpContext context, CgiOptions settings)
    {
        var declared = context.Request.ContentLength;
        if (declared > settings.MaxRequestBodyBytes)
            throw new CgiBodyTooLargeException();

        var memory = new MemoryStream();
        var buffer = new byte[16 * 1024];
        int read;
        while ((read = await context.Request.Body.ReadAsync(buffer, context.RequestAborted).ConfigureAwait(false)) > 0)
        {
            if (memory.Length + read > settings.MaxRequestBodyBytes)
            {
                await memory.DisposeAsync().ConfigureAwait(false);
                throw new CgiBodyTooLargeException();
            }
            memory.Write(buffer, 0, read);
        }
        return memory;
    }

    private static string? TranslatePath(string scriptRoot, string pathInfo)
    {
        if (string.IsNullOrEmpty(pathInfo))
            return null;
        var root = global::System.IO.Path.GetFullPath(scriptRoot);
        var translated = global::System.IO.Path.GetFullPath(global::System.IO.Path.Combine(root, pathInfo.TrimStart('/')));
        var comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        return translated.StartsWith(root + global::System.IO.Path.DirectorySeparatorChar, comparison) ? translated : null;
    }

    private static Dictionary<string, string> BuildEnvironment(HttpContext context, CgiOptions settings, PathString prefix, CgiScript script, long contentLength)
    {
        var request = context.Request;
        var connection = context.Connection;
        var environment = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["GATEWAY_INTERFACE"] = "CGI/1.1",
            ["SERVER_PROTOCOL"] = request.Protocol,
            ["SERVER_SOFTWARE"] = "OoBDev.AspNetCore.Cgi",
            ["SERVER_NAME"] = request.Host.Host,
            ["SERVER_PORT"] = (request.Host.Port ?? connection.LocalPort).ToString(CultureInfo.InvariantCulture),
            ["REQUEST_METHOD"] = request.Method,
            ["SCRIPT_NAME"] = $"{prefix.Value}/{script.ScriptName}",
            ["PATH_INFO"] = script.PathInfo,
            ["QUERY_STRING"] = request.QueryString.HasValue ? request.QueryString.Value![1..] : string.Empty,
            ["REMOTE_ADDR"] = connection.RemoteIpAddress?.ToString() ?? string.Empty,
            ["REQUEST_SCHEME"] = request.Scheme,
        };
        if (TranslatePath(settings.ScriptRoot, script.PathInfo) is { } translated)
            environment["PATH_TRANSLATED"] = translated;
        if (request.Scheme == Uri.UriSchemeHttps)
            environment["HTTPS"] = "on";
        if (contentLength > 0)
            environment["CONTENT_LENGTH"] = contentLength.ToString(CultureInfo.InvariantCulture);
        if (!string.IsNullOrEmpty(request.ContentType))
            environment["CONTENT_TYPE"] = request.ContentType;
        if (context.User.Identity is { IsAuthenticated: true, Name: { } user })
            environment["REMOTE_USER"] = user;

        foreach (var header in request.Headers)
        {
            if (_skippedHeaders.Contains(header.Key, StringComparer.OrdinalIgnoreCase))
                continue;
            environment["HTTP_" + header.Key.ToUpperInvariant().Replace('-', '_')] = string.Join(", ", header.Value.ToArray());
        }
        return environment;
    }

    private static async Task WriteProblemAsync(HttpContext context, CgiException exception)
    {
        var response = context.Response;
        response.Clear();
        response.StatusCode = exception.StatusCode;
        if (exception is CgiBusyException)
            response.Headers.RetryAfter = "1";
        response.ContentType = "application/problem+json";
        var problem = new ProblemDetails
        {
            Status = exception.StatusCode,
            Title = ReasonFor(exception.StatusCode),
            Detail = exception.Message,
            Instance = context.Request.Path,
        };
        await response.WriteAsync(JsonSerializer.Serialize(problem), Encoding.UTF8).ConfigureAwait(false);
    }

    private static string ReasonFor(int status) => status switch
    {
        401 => "Unauthorized",
        403 => "Forbidden",
        404 => "Not Found",
        413 => "Content Too Large",
        500 => "Internal Server Error",
        502 => "Bad Gateway",
        503 => "Service Unavailable",
        504 => "Gateway Timeout",
        _ => "Error",
    };

    [LoggerMessage(Level = LogLevel.Warning, Message = "CGI request {Path} failed before the response started.")]
    private partial void LogFailed(Exception exception, string path);

    [LoggerMessage(Level = LogLevel.Warning, Message = "CGI request {Path} was aborted after the response started.")]
    private partial void LogAborted(Exception exception, string path);
}
