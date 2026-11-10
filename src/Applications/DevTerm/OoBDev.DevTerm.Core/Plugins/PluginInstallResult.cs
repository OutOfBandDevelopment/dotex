namespace OoBDev.DevTerm;

/// <summary>The outcome of <see cref="PluginInstaller.Install"/>: installed (with the plugin folder), or refused with the reason.</summary>
public sealed record PluginInstallResult(bool Installed, string Message, string? Folder = null);
