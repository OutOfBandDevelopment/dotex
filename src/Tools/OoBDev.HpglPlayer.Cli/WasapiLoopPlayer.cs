using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace OoBDev.HpglPlayer.Cli;

/// <summary>Plays a WAV in a loop through WASAPI (shared mode) on a chosen output device, with any channel count the device accepts.</summary>
public static class WasapiLoopPlayer
{
    /// <summary>The names of the active output devices, in the order <see cref="DeviceChoice.Pick"/> numbers them.</summary>
    public static IReadOnlyList<string> DeviceNames()
    {
        using var enumerator = new MMDeviceEnumerator();
        return [.. enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active).Select(d => d.FriendlyName)];
    }

    /// <summary>Plays until <paramref name="stop"/> is set.</summary>
    /// <exception cref="InvalidOperationException">The device does not accept the signal's format.</exception>
    public static void PlayLooping(byte[] wav, string? device, ManualResetEventSlim stop)
    {
        using var enumerator = new MMDeviceEnumerator();
        var devices = enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active).ToList();
        var chosen = device is null
            ? enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia)
            : devices[DeviceChoice.Pick([.. devices.Select(d => d.FriendlyName)], device)];

        using var reader = new WaveFileReader(new MemoryStream(wav));
        try
        {
            using var output = new WasapiOut(chosen, AudioClientShareMode.Shared, true, 100);
            output.Init(new LoopingWaveProvider(reader));
            output.Play();
            stop.Wait();
            output.Stop();
        }
        catch (Exception ex) when (ex is System.Runtime.InteropServices.COMException or ArgumentException)
        {
            throw new InvalidOperationException(
                $"'{chosen.FriendlyName}' does not accept {reader.WaveFormat.Channels} channels at {reader.WaveFormat.SampleRate} Hz in shared mode ({ex.Message}). Set the device to 5.1 or more in the Windows sound settings, or write a file with --wav.", ex);
        }
    }
}
