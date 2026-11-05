using System.IO;

namespace OoBDev.ImageMask.Cli;

/// <summary>Keeps the committed guide images unmasked and free of churn: a new capture replaces the committed image only when the two differ once the zones are masked.</summary>
public static class ImageSync
{
    /// <summary>Returns true when <paramref name="committedPath"/> was replaced by the new capture; a missing committed file always counts as different.</summary>
    public static bool Run(string newPath, string committedPath, MaskDefinition definition, int tolerance, long maxDifferentPixels)
    {
        if (File.Exists(committedPath))
        {
            using var committed = ImageMasker.Load(committedPath);
            using var capture = ImageMasker.Load(newPath);
            ImageMasker.Apply(committed, definition, Path.GetFileName(committedPath));
            ImageMasker.Apply(capture, definition, Path.GetFileName(committedPath));

            var result = ImageComparer.Compare(committed, capture, tolerance, out var diff);
            diff?.Dispose();
            if (result.IsMatch(maxDifferentPixels))
                return false;
        }

        File.Copy(newPath, committedPath, overwrite: true);
        return true;
    }
}
