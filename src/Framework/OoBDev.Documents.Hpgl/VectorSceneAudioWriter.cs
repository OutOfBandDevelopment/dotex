using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace OoBDev.Documents.Hpgl;

/// <summary>
/// Writes a <see cref="VectorScene"/> as a stereo 16-bit PCM WAV for an oscilloscope in X-Y mode:
/// the left channel is X and the right channel is Y, both scaled to -1..1 keeping the aspect ratio.
/// There is no blanking track, so the jumps between lines are kept short.
/// </summary>
/// <param name="options">The settings; they are validated here.</param>
public sealed class VectorSceneAudioWriter(VectorSceneAudioOptions options)
{
    private readonly VectorSceneAudioOptions _options = Validated(options);

    private static VectorSceneAudioOptions Validated(VectorSceneAudioOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        options.Validate();
        return options;
    }

    /// <summary>Writes the WAV; an empty scene writes nothing.</summary>
    /// <param name="scene">The scene to draw.</param>
    /// <param name="output">Where the WAV bytes go.</param>
    public void Write(VectorScene scene, Stream output)
    {
        ArgumentNullException.ThrowIfNull(scene);
        ArgumentNullException.ThrowIfNull(output);

        var frame = BuildFrame(scene);
        if (frame.Count == 0) return;

        var total = checked((int)(_options.SampleRate * _options.Duration.TotalSeconds));
        if (total == 0) return;

        var dataLength = total * 4;
        using var writer = new BinaryWriter(output, Encoding.ASCII, leaveOpen: true);
        writer.Write("RIFF"u8);
        writer.Write(36 + dataLength);
        writer.Write("WAVEfmt "u8);
        writer.Write(16);
        writer.Write((short)1);
        writer.Write((short)2);
        writer.Write(_options.SampleRate);
        writer.Write(_options.SampleRate * 4);
        writer.Write((short)4);
        writer.Write((short)16);
        writer.Write("data"u8);
        writer.Write(dataLength);
        for (var i = 0; i < total; i++)
        {
            var (x, y) = frame[i % frame.Count];
            writer.Write(ToPcm(x));
            writer.Write(ToPcm(y));
        }
    }

    private static short ToPcm(double value) => (short)Math.Round(Math.Clamp(value, -1, 1) * short.MaxValue);

    private List<(double X, double Y)> BuildFrame(VectorScene scene)
    {
        var lines = scene.Lines.Where(l => l.Points.Count > 0).ToList();
        var frame = new List<(double, double)>();
        if (lines.Count == 0) return frame;

        var points = lines.SelectMany(l => l.Points).ToList();
        var minX = Math.Min(0, points.Min(p => p.X));
        var maxX = Math.Max(scene.Width, points.Max(p => p.X));
        var minY = Math.Min(0, points.Min(p => p.Y));
        var maxY = Math.Max(scene.Height, points.Max(p => p.Y));
        var span = Math.Max(Math.Max(maxX - minX, maxY - minY), double.Epsilon);
        var centreX = (minX + maxX) / 2;
        var centreY = (minY + maxY) / 2;
        (double, double) Map(VectorPoint p) => ((p.X - centreX) * 2 / span, -(p.Y - centreY) * 2 / span);

        var frameSamples = Math.Max(1, _options.SampleRate / _options.FramesPerSecond);
        var jumps = Math.Max(0, lines.Count - 1) * _options.JumpSamples;
        var drawBudget = Math.Max(lines.Count, frameSamples - jumps);
        var length = lines.Sum(l => PathLength(l.Points));
        var step = length > 0 ? length / drawBudget : 0;

        for (var li = 0; li < lines.Count; li++)
        {
            var line = lines[li].Points;
            var start = Map(line[0]);
            if (frame.Count > 0)
            {
                var last = frame[^1];
                for (var j = 1; j <= _options.JumpSamples; j++)
                {
                    var t = (double)j / (_options.JumpSamples + 1);
                    frame.Add((last.Item1 + (start.Item1 - last.Item1) * t, last.Item2 + (start.Item2 - last.Item2) * t));
                }
            }

            frame.Add(start);
            for (var i = 1; i < line.Count; i++)
            {
                var a = line[i - 1];
                var b = line[i];
                var segment = Math.Sqrt(((b.X - a.X) * (b.X - a.X)) + ((b.Y - a.Y) * (b.Y - a.Y)));
                var n = step > 0 ? Math.Max(1, (int)Math.Round(segment / step)) : 1;
                for (var k = 1; k <= n; k++)
                {
                    var t = (double)k / n;
                    frame.Add(Map(new VectorPoint(a.X + (b.X - a.X) * t, a.Y + (b.Y - a.Y) * t)));
                }
            }
        }

        return frame;
    }

    private static double PathLength(IReadOnlyList<VectorPoint> points)
    {
        var sum = 0.0;
        for (var i = 1; i < points.Count; i++)
        {
            var dx = points[i].X - points[i - 1].X;
            var dy = points[i].Y - points[i - 1].Y;
            sum += Math.Sqrt((dx * dx) + (dy * dy));
        }

        return sum;
    }
}
