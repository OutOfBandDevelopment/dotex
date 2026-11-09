namespace OoBDev.DevTerm;

/// <summary>One plugin folder's outcome: loaded, or skipped with the reason.</summary>
public sealed record PluginLoadResult(string Folder, string Name, bool Loaded, string Message);
