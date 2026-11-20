using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading;

namespace OoBDev.Spike.PluginSandbox;

/// <summary>The plugin side: <c>echo</c> answers requests, <c>oversize</c> sends a bad length, <c>hog</c> allocates until the job kills it.</summary>
internal static class Program
{
    internal const string KeyVariable = "PLUGIN_FRAME_KEY";
    internal const string FlagsVariable = "PLUGIN_FRAME_FLAGS";

    private static int Main(string[] args)
    {
        var mode = args.Length > 0 ? args[0] : "echo";
        var input = Console.OpenStandardInput();
        var output = Console.OpenStandardOutput();
        switch (mode)
        {
            case "echo":
                return Echo(input, output);

            case "oversize":
                var header = new byte[FrameChannel.HeaderSize];
                header[0] = FrameChannel.Version;
                header[1] = byte.Parse(Environment.GetEnvironmentVariable(FlagsVariable) ?? "0");
                header[4] = 0x00;
                header[5] = 0x00;
                header[6] = 0x20;
                output.Write(header);
                output.Flush();
                input.CopyTo(Stream.Null);
                return 0;

            case "brokered":
                return Brokered(input, output);

            case "caller":
                return Caller(args);

            case "holdmutex":
                return HoldMutex(args);

            case "probe":
                return Probe(args, input, output);

            case "fork":
                for (var i = 0; i < 10; i++)
                {
                    Process.Start(new ProcessStartInfo("/usr/bin/sleep", "30") { UseShellExecute = false });
                }

                Thread.Sleep(Timeout.Infinite);
                return 0;

            case "spin":
                while (true)
                {
                }

            case "sleep":
                Thread.Sleep(Timeout.Infinite);
                return 0;

            case "hog":
                var blocks = new List<byte[]>();
                while (true)
                {
                    var block = new byte[16 * 1024 * 1024];
                    Array.Fill(block, (byte)1);
                    blocks.Add(block);
                }

            default:
                return 2;
        }
    }

    // A stand-in host: starts a sandboxed plugin (echo, or sleep when it should survive because echo ends when its stdin closes) (killOnClose per args[1]), writes its process id to args[2], then waits to be killed.
    private static int Caller(string[] args)
    {
        if (!OperatingSystem.IsWindows())
        {
            return 3;
        }

        var kill = args[1] == "kill";
        var plugin = new SandboxedPlugin(kill ? "echo" : "sleep", new JobLimits(MemoryMb: 256), killOnClose: kill);
        if (kill)
        {
            plugin.Receive();
        }

        File.WriteAllText(args[2], plugin.ProcessId.ToString(global::System.Globalization.CultureInfo.InvariantCulture));
        Thread.Sleep(Timeout.Infinite);
        return 0;
    }

    // Holds a named mutex (args[1]) until the process is killed, then the next owner sees it abandoned; args[2] is written once held.
    private static int HoldMutex(string[] args)
    {
        using var mutex = new Mutex(false, args[1]);
        mutex.WaitOne();
        File.WriteAllText(args[2], "held");
        Thread.Sleep(Timeout.Infinite);
        return 0;
    }

    // Tries one thing a sandboxed plugin must not be able to do and reports whether it worked, so a test can assert the refusal.
    private static int Probe(string[] args, Stream input, Stream output)
    {
        var kind = args[1];
        var target = args[2];
        bool allowed;
        string detail;
        try
        {
            switch (kind)
            {
                case "net":
                    var parts = target.Split(':');
                    using (var client = new TcpClient())
                    {
                        allowed = client.ConnectAsync(parts[0], int.Parse(parts[1], global::System.Globalization.CultureInfo.InvariantCulture)).Wait(3000) && client.Connected;
                    }

                    detail = "connect";
                    break;

                case "file-read":
                    detail = File.ReadAllText(target);
                    allowed = true;
                    break;

                case "file-write":
                    File.WriteAllText(target, "x");
                    detail = "written";
                    allowed = true;
                    break;

                case "pids":
                    var count = Directory.GetDirectories("/proc").Count(path => int.TryParse(Path.GetFileName(path), out _));
                    allowed = count > int.Parse(target, global::System.Globalization.CultureInfo.InvariantCulture);
                    detail = count.ToString(global::System.Globalization.CultureInfo.InvariantCulture);
                    break;

                case "env":
                    var value = Environment.GetEnvironmentVariable(target);
                    allowed = value is not null;
                    detail = value ?? "absent";
                    break;

                case "spawn":
                    using (var child = Process.Start(new ProcessStartInfo(target) { UseShellExecute = false, CreateNoWindow = true }))
                    {
                        child?.WaitForExit(3000);
                        allowed = child is not null;
                    }

                    detail = "spawned";
                    break;

                default:
                    return 2;
            }
        }
        catch (Exception ex)
        {
            allowed = false;
            detail = ex.GetType().Name;
        }

        var flags = (FrameFlags)byte.Parse(Environment.GetEnvironmentVariable(FlagsVariable) ?? "0");
        var key = Environment.GetEnvironmentVariable(KeyVariable) is { } hex ? Convert.FromHexString(hex) : null;
        var channel = new FrameChannel(input, output, flags, key);
        channel.Send(FrameType.Event, "probe.result", JsonSerializer.Serialize(new { kind, allowed, detail }));
        return 0;
    }

    // A plugin that can do nothing itself: it forwards read, write and fetch requests to the host broker and returns the answer, and "whoami" returns the request context the host sent.
    private static int Brokered(Stream input, Stream output)
    {
        var flags = (FrameFlags)byte.Parse(Environment.GetEnvironmentVariable(FlagsVariable) ?? "0");
        var key = Environment.GetEnvironmentVariable(KeyVariable) is { } hex ? Convert.FromHexString(hex) : null;
        var channel = new FrameChannel(input, output, flags, key);
        channel.Send(FrameType.Event, "lifecycle.ready", """{"id":"brokered","version":"1.0.0"}""");
        var context = "{}";
        try
        {
            while (true)
            {
                var message = channel.Read();
                if (message.Type == FrameType.Shutdown)
                {
                    return 0;
                }

                if (message.Topic == "context.set")
                {
                    context = message.Text;
                    continue;
                }

                using var document = JsonDocument.Parse(message.Payload);
                var op = document.RootElement.GetProperty("op").GetString();
                if (op == "whoami")
                {
                    channel.Reply(message, FrameType.Response, global::System.Text.Encoding.UTF8.GetBytes(context));
                    continue;
                }

                var topic = op switch
                {
                    "read" => "broker.file.read",
                    "write" => "broker.file.write",
                    _ => "broker.fetch",
                };
                channel.Send(FrameType.Request, topic, message.Text);
                var answer = channel.Read();
                channel.Reply(message, FrameType.Response, answer.Payload);
            }
        }
        catch (EndOfStreamException)
        {
            return 0;
        }
        catch (FrameViolation)
        {
            return 3;
        }
    }

    private static int Echo(Stream input, Stream output)
    {
        var flags = (FrameFlags)byte.Parse(Environment.GetEnvironmentVariable(FlagsVariable) ?? "0");
        var key = Environment.GetEnvironmentVariable(KeyVariable) is { } hex ? Convert.FromHexString(hex) : null;
        var channel = new FrameChannel(input, output, flags, key);
        channel.Send(FrameType.Event, "lifecycle.ready", """{"id":"spike","version":"1.0.0"}""");
        try
        {
            while (true)
            {
                var message = channel.Read();
                if (message.Type == FrameType.Shutdown)
                {
                    return 0;
                }

                channel.Reply(message, FrameType.Response, message.Payload);
            }
        }
        catch (EndOfStreamException)
        {
            return 0;
        }
        catch (FrameViolation)
        {
            return 3;
        }
    }
}
