using System;
using System.IO;
using System.Media;
using System.Threading;

namespace OoBDev.HpglPlayer.Cli;

internal static class Program
{
    private static int Main(string[] args)
    {
        if (Array.Exists(args, a => a == "--list-devices"))
        {
            return ListDevices();
        }

        try
        {
            return PlayerCommand.Run(args, PlayLooping, Console.Error);
        }
        catch (InvalidOperationException ex)
        {
            Console.Error.WriteLine(ex.Message);
            return 2;
        }
    }

    private static int ListDevices()
    {
        var names = WasapiLoopPlayer.DeviceNames();
        for (var i = 0; i < names.Count; i++)
        {
            Console.WriteLine($"{i}: {names[i]}");
        }

        return 0;
    }

    private static void PlayLooping(byte[] wav, string? device)
    {
        if (!OperatingSystem.IsWindows())
        {
            throw new InvalidOperationException("Playback needs Windows; write a file with --wav and play it with any player set to loop.");
        }

        using var stop = new ManualResetEventSlim();
        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            stop.Set();
        };
        Console.Error.WriteLine("Playing in a loop; Ctrl+C stops.");
        var channels = BitConverter.ToInt16(wav, 22);
        if (device is null && channels == 2)
        {
            using var stream = new MemoryStream(wav);
            using var player = new SoundPlayer(stream);
            player.PlayLooping();
            stop.Wait();
            player.Stop();
            return;
        }

        WasapiLoopPlayer.PlayLooping(wav, device, stop);
    }
}
