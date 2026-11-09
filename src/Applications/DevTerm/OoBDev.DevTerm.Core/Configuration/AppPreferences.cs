using System.Collections.Generic;

namespace OoBDev.DevTerm.Configuration;

/// <summary>
/// App-wide preferences - settings about dev-term itself rather than about a device connection, so
/// they live in <see cref="DevTermUserDataPaths.PreferencesFile"/>, never in a connection profile or
/// <c>appsettings.Local.json</c>. The theme and the log/export retention rules.
/// </summary>
public sealed class AppPreferences
{
    /// <summary>The selected theme: <c>light</c>, <c>dark</c>, <c>system</c>, or a user theme's name. Null means never chosen (<c>system</c>).</summary>
    public string? Theme { get; set; }

    /// <summary>How long session logs are kept. Null (the default) keeps everything.</summary>
    public RetentionRule? LogRetention { get; set; }

    /// <summary>How long exports are kept. Null (the default) keeps everything.</summary>
    public RetentionRule? ExportRetention { get; set; }

    /// <summary>Manifest names whose control panel the user has already opened from Device > Device Manifest, so <see cref="ManifestPanelHint"/> stops offering it.</summary>
    public List<string> UsedPanelHints { get; set; } = [];
}
