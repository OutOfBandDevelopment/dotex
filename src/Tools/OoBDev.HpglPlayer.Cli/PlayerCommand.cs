using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using OoBDev.Documents.Hpgl;

namespace OoBDev.HpglPlayer.Cli;

/// <summary>Turns an HP-GL file into the stereo X-Y signal and hands it to a player, or writes it as a WAV file. Exit code 0 done, 2 error.</summary>
public static class PlayerCommand
{
    /// <summary>The help text printed when the arguments are missing.</summary>
    public const string Usage = """
        hpgl-player <file.plt> [--rate 48000] [--fps 25] [--jump 4] [--seconds 1] [--z true] [--device name-or-number] [--wav out.wav]
        Plays the plot as a looping left=X, right=Y signal on the default sound device until Ctrl+C; --wav writes the file instead.
        --z true adds a third blanking track and --device picks the output (both play through WASAPI); hpgl-player --list-devices lists the outputs.
        Windows only for playback. Start with the volume low and the scope in X-Y mode.
        """;

    /// <summary>Runs the command; <paramref name="play"/> receives the WAV bytes and the chosen device (null: the default) and returns when playback ends.</summary>
    public static int Run(string[] args, Action<byte[], string?> play, TextWriter error)
    {
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(play);
        ArgumentNullException.ThrowIfNull(error);
        if (args.Length == 0 || args[0].StartsWith("--", StringComparison.Ordinal))
        {
            error.WriteLine(Usage);
            return 2;
        }

        try
        {
            var config = new ConfigurationBuilder().AddCommandLine(args[1..]).Build();
            var options = new VectorSceneAudioOptions
            {
                SampleRate = config.GetValue("rate", 48000),
                FramesPerSecond = config.GetValue("fps", 25),
                JumpSamples = config.GetValue("jump", 4),
                Duration = TimeSpan.FromSeconds(config.GetValue("seconds", 1.0)),
                ZChannel = config.GetValue("z", false),
            };
            var wav = Render(File.ReadAllBytes(args[0]), options);
            if (wav.Length == 0)
            {
                error.WriteLine("The file holds nothing to draw.");
                return 2;
            }

            var target = config["wav"];
            if (target is null)
            {
                play(wav, config["device"]);
            }
            else
            {
                File.WriteAllBytes(target, wav);
            }

            return 0;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or FormatException or InvalidOperationException)
        {
            error.WriteLine(ex.Message);
            return 2;
        }
    }

    /// <summary>Parses the plotter bytes and renders the WAV; empty when the scene has no lines.</summary>
    public static byte[] Render(byte[] plot, VectorSceneAudioOptions options)
    {
        var scene = HpglToSvgConverter.ParseScene(plot);
        using var wav = new MemoryStream();
        new VectorSceneAudioWriter(options).Write(scene, wav);
        return wav.ToArray();
    }
}
