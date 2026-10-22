using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OoBDev.AspNetCore.Cgi;

/// <summary>
/// Options for <see cref="CgiMiddleware"/>.
/// </summary>
public class CgiOptions
{
    /// <summary>
    /// The URL prefix that maps to <see cref="ScriptRoot"/>.
    /// </summary>
    [Required]
    public string Prefix { get; set; } = "/cgi-bin";

    /// <summary>
    /// The directory that holds the scripts. It must exist and must not be under the static file root.
    /// </summary>
    [Required]
    public string ScriptRoot { get; set; } = default!;

    /// <summary>
    /// Maps a script extension (for example <c>.py</c>) to the executable that runs it (for example <c>python3</c>).
    /// An empty value runs the script file directly. Extensions not listed are refused with 403.
    /// </summary>
    public IDictionary<string, string> Handlers { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Maximum run time before the process tree is killed.
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Maximum request body size in bytes. Larger requests get 413.
    /// </summary>
    [Range(0, long.MaxValue)]
    public long MaxRequestBodyBytes { get; set; } = 10 * 1024 * 1024;

    /// <summary>
    /// Maximum response body size in bytes, or null for no limit. Past the limit the response is aborted.
    /// </summary>
    public long? MaxResponseBytes { get; set; }

    /// <summary>
    /// Maximum number of processes running at once. Requests over the limit get 503 with <c>Retry-After</c>.
    /// </summary>
    [Range(1, int.MaxValue)]
    public int MaxConcurrentProcesses { get; set; } = Environment.ProcessorCount * 2;

    /// <summary>
    /// Maximum number of stderr bytes kept and logged per request.
    /// </summary>
    [Range(0, int.MaxValue)]
    public int MaxStderrBytes { get; set; } = 64 * 1024;

    /// <summary>
    /// Lets unauthenticated users run scripts. Off by default: callers must be authenticated.
    /// </summary>
    public bool AllowAnonymous { get; set; }

    /// <summary>
    /// Application rights (<c>CommonClaims.ApplicationRight</c>); the user needs at least one when the list is not empty.
    /// </summary>
    public IList<string> RequiredRights { get; set; } = [];

    /// <summary>
    /// Extra rights per script file name, checked after <see cref="RequiredRights"/>; the user needs at least one of the listed rights.
    /// </summary>
    public IDictionary<string, IList<string>> ScriptRights { get; set; } = new Dictionary<string, IList<string>>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Allows <c>.cmd</c> and <c>.bat</c> handlers (Windows). Off by default.
    /// </summary>
    public bool AllowBatchFiles { get; set; }

    /// <summary>
    /// Names of host environment variables copied into the (otherwise cleared) child environment when they exist.
    /// </summary>
    public IList<string> InheritedEnvironmentVariables { get; set; } = ["PATH", "SystemRoot"];
}
