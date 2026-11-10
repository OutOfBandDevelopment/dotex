using System;
using System.Collections.Generic;
using System.IO;

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
