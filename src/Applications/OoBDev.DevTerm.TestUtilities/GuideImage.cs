using System;
using System.IO;
using OoBDev.ImageMask.Cli;

namespace OoBDev.DevTerm.TestUtilities;

/// <summary>Writes a guide screenshot through <c>image-mask sync</c>: the committed image is replaced only when the new capture differs once the zones in the <c>zones.json</c> beside it are masked.</summary>
public static class GuideImage
{
    private const string _zonesFileName = "zones.json";

    /// <summary>Renders into a temporary file with <paramref name="render"/>, then syncs it over <paramref name="committedPath"/>; returns true when the committed image was replaced.</summary>
    public static bool Write(string committedPath, Action<string> render)
    {
        ArgumentNullException.ThrowIfNull(render);

        var directory = Path.GetDirectoryName(committedPath)!;
        Directory.CreateDirectory(directory);
        var temp = Path.Combine(Path.GetTempPath(), $"guide_{Guid.NewGuid():N}.png");
        try
        {
            render(temp);
            return Sync(temp, committedPath);
        }
        finally
        {
            File.Delete(temp);
        }
    }

    /// <summary>Syncs an existing capture over the committed image using the zones file next to it, when there is one.</summary>
    public static bool Sync(string capturePath, string committedPath)
    {
        var zonesPath = Path.Combine(Path.GetDirectoryName(committedPath)!, _zonesFileName);
        var definition = File.Exists(zonesPath) ? MaskDefinition.Load(zonesPath) : new MaskDefinition();
        return ImageSync.Run(capturePath, committedPath, definition, tolerance: 0, maxDifferentPixels: 0);
    }
}
