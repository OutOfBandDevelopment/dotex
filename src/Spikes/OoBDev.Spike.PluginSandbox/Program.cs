using System;
using System.Collections.Generic;
using System.IO;

namespace OoBDev.Spike.PluginSandbox;

/// <summary>The plugin side: <c>echo</c> answers requests, <c>oversize</c> sends a bad length, <c>hog</c> allocates until the job kills it.</summary>
internal static class Program
{
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
                output.Write([0x00, 0x00, 0x20, 0x00]);
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
        Frame.Write(output, """{"v":1,"type":"Event","topic":"lifecycle.ready","payload":{"id":"spike","version":"1.0.0"}}""");
        try
        {
            while (true)
            {
                using var message = Frame.Read(input);
                var root = message.RootElement;
                if (root.GetProperty("type").GetString() == "Shutdown")
                {
                    return 0;
                }

                var id = root.GetProperty("requestId").GetString();
                var payload = root.GetProperty("payload").GetRawText();
                Frame.Write(output, "{\"v\":1,\"type\":\"Response\",\"correlationId\":\"" + id + "\",\"payload\":" + payload + "}");
            }
        }
        catch (EndOfStreamException)
        {
            return 0;
        }
    }
}
