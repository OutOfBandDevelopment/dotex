namespace OoBDev.DevTerm.DeviceManifests;

/// <summary>One manifest found on disk by <see cref="ManifestCatalog.Discover"/>.</summary>
/// <param name="Name">The manifest's own <see cref="DeviceManifest.Name"/> when it loads, else its file/folder name.</param>
/// <param name="Path">What to hand <see cref="DeviceManifestLoader.Load"/>: a folder, a <c>.json</c> file, or a <c>.zip</c>.</param>
/// <param name="Source">Which location it came from (e.g. "user", "installed").</param>
public sealed record ManifestEntry(string Name, string Path, string Source)
{
    /// <summary>What a picker lists: <c>Name (source)</c>.</summary>
    public string DisplayName => $"{Name} ({Source})";
}
