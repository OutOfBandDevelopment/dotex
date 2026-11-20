using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Text;

namespace OoBDev.Spike.PluginSandbox;

/// <summary>
/// Runs this assembly as a plugin under the macOS Seatbelt sandbox (<c>/usr/bin/sandbox-exec</c>) with a generated profile: no network, no file
/// writes except the granted folders and the plugin's scratch folder, no reads of user data except the granted folders, and an explicit environment.
/// Written from the documented profile language and NOT yet run on a Mac (none was available): the profile text is unit-tested on every platform,
/// the escape tests in <c>CrossPlatformEscapeTests</c> run it when a Mac is present. <c>sandbox-exec</c> is deprecated by Apple but still ships;
/// the supported route (App Sandbox entitlements) needs a signed bundle and is the production follow-up.
/// There is no PID namespace or resource limit on macOS; those rules are met by launcher-side checks (not built).
/// </summary>
[SupportedOSPlatform("macos")]
public sealed class SeatbeltPlugin : ISandboxedPlugin
{
    private readonly Process _process;
    private readonly FrameChannel _channel;

    /// <summary>Starts the plugin under <c>sandbox-exec</c>.</summary>
    public SeatbeltPlugin(string commandArguments, IReadOnlyCollection<string>? readDirectories = null, IReadOnlyCollection<string>? writeDirectories = null, FrameFlags flags = FrameFlags.Crc32 | FrameFlags.Hmac | FrameFlags.CounterSign)
    {
        var key = RandomNumberGenerator.GetBytes(32);
        var dll = typeof(SeatbeltPlugin).Assembly.Location;
        var dotnetRoot = Path.GetFullPath(Path.Combine(RuntimeEnvironment.GetRuntimeDirectory(), "..", "..", ".."));
        var scratch = Directory.CreateTempSubdirectory("plugin-").FullName;
        var write = new List<string>(writeDirectories ?? []) { scratch };
        var info = new ProcessStartInfo("/usr/bin/sandbox-exec")
        {
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        info.ArgumentList.Add("-p");
        info.ArgumentList.Add(Profile(Real(dotnetRoot), Real(Path.GetDirectoryName(dll)!), Real(readDirectories ?? []), Real(write)));
        info.ArgumentList.Add(Path.Combine(dotnetRoot, "dotnet"));
        info.ArgumentList.Add(dll);
        foreach (var part in CommandLine.Split(commandArguments))
        {
            info.ArgumentList.Add(part);
        }

        info.Environment.Clear();
        info.Environment["HOME"] = scratch;
        info.Environment["TMPDIR"] = scratch;
        info.Environment["DOTNET_ROOT"] = dotnetRoot;
        info.Environment["DOTNET_NOLOGO"] = "1";
        info.Environment["DOTNET_CLI_HOME"] = scratch;
        info.Environment[Program.KeyVariable] = Convert.ToHexString(key);
        info.Environment[Program.FlagsVariable] = ((byte)flags).ToString(global::System.Globalization.CultureInfo.InvariantCulture);
        _process = Process.Start(info) ?? throw new InvalidOperationException("could not start the plugin");
        _channel = new FrameChannel(_process.StandardOutput.BaseStream, _process.StandardInput.BaseStream, flags, key);
    }

    /// <inheritdoc />
    /// <inheritdoc />
    public LimitHit LimitHit => LimitHit.None;

    public int ExitCode => _process.ExitCode;

    /// <summary>
    /// Builds the profile. Later rules win in Seatbelt, so the order is: allow everything, deny the network and all writes, deny reads of
    /// user data areas, then allow back what the plugin needs and what was granted. Paths must already be real paths (macOS temp is under /private).
    /// </summary>
    public static string Profile(string dotnetRoot, string assemblyDirectory, IReadOnlyCollection<string> read, IReadOnlyCollection<string> write)
    {
        var profile = new StringBuilder();
        profile.AppendLine("(version 1)");
        profile.AppendLine("(allow default)");
        profile.AppendLine("(deny network*)");
        profile.AppendLine("(deny file-write*)");
        profile.AppendLine("(deny file-read* (subpath \"/Users\") (subpath \"/Volumes\") (subpath \"/private/var\") (subpath \"/private/tmp\") (subpath \"/Library/Keychains\"))");
        profile.AppendLine("(allow file-write* (literal \"/dev/null\") (literal \"/dev/dtracehelper\"))");
        foreach (var path in new[] { dotnetRoot, assemblyDirectory })
        {
            profile.AppendLine(Allow("file-read*", path));
        }

        foreach (var path in read)
        {
            profile.AppendLine(Allow("file-read*", path));
        }

        foreach (var path in write)
        {
            profile.AppendLine(Allow("file-read* file-write*", path));
        }

        return profile.ToString();
    }

    /// <inheritdoc />
    public ulong Send(FrameType type, string topic, string json) => _channel.Send(type, topic, json);

    /// <summary>Answers <paramref name="request"/> with a JSON payload.</summary>
    public ulong Reply(ReceivedFrame request, FrameType type, string json) => _channel.Reply(request, type, global::System.Text.Encoding.UTF8.GetBytes(json));

    /// <inheritdoc />
    public ReceivedFrame Receive() => _channel.Read();

    /// <inheritdoc />
    public bool WaitForExit(int milliseconds) => _process.WaitForExit(milliseconds);

    /// <inheritdoc />
    public void Dispose()
    {
        if (!_process.HasExited)
        {
            _process.Kill(entireProcessTree: true);
        }

        _process.WaitForExit();
        _process.Dispose();
    }

    private static string Allow(string operations, string path) => $"(allow {operations} (subpath \"{path.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("\"", "\\\"", StringComparison.Ordinal)}\"))";

    private static string Real(string path) => Directory.ResolveLinkTarget(path, returnFinalTarget: true)?.FullName ?? ResolvePrivate(Path.GetFullPath(path));

    private static List<string> Real(IEnumerable<string> paths)
    {
        var result = new List<string>();
        foreach (var path in paths)
        {
            result.Add(Real(path));
        }

        return result;
    }

    // /var, /tmp and /etc are links into /private on macOS; Seatbelt matches the real path.
    private static string ResolvePrivate(string path) =>
        path.StartsWith("/var/", StringComparison.Ordinal) || path.StartsWith("/tmp/", StringComparison.Ordinal) || path.StartsWith("/etc/", StringComparison.Ordinal)
            ? "/private" + path
            : path;
}
