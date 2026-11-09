using System;
using System.IO;
using OoBDev.ImageMask.Cli;
using OoBDev.System.IO;

namespace OoBDev.Examples.TestUtilities;

/// <summary>Where the control examples' user-guide screenshots live, and how a fresh capture replaces a committed one: through <c>image-mask sync</c>, so the file changes only when the capture differs once the zones in the <c>zones.json</c> beside it are masked.</summary>
public static class ExampleGuide
{
    /// <summary>The committed image path for <paramref name="imageName"/>.</summary>
    public static string ImagePath(string imageName) =>
        Path.Combine(RepoRoot(), "docs", "applications", "example", "controls-guide", "images", imageName);

    /// <summary>Renders into a temporary file with <paramref name="render"/>, then syncs it over the committed image; returns true when the committed image was replaced.</summary>
    public static bool Write(string imageName, Action<string> render)
    {
        ArgumentNullException.ThrowIfNull(render);
        using var temp = new TempFileHandle();
        render(temp.FilePath);
        return Sync(temp.FilePath, imageName);
    }

    /// <summary>Syncs an existing capture over the committed image.</summary>
    public static bool Sync(string capturePath, string imageName)
    {
        var committed = ImagePath(imageName);
        Directory.CreateDirectory(Path.GetDirectoryName(committed)!);
        return ImageSync.RunBesideZonesFile(capturePath, committed);
    }

    private static string RepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "GitVersion.yml")))
        {
            dir = dir.Parent;
        }

        return dir?.FullName ?? throw new InvalidOperationException("repo root not found");
    }
}
