using System.Collections.Generic;
using System.Text.Json;

namespace OoBDev.System.Sandbox;

/// <summary>The manifest of a plugin package (<c>protocol-and-manifest.md</c>, Table 9), reduced to the fields the signing and grant spike needs.</summary>
/// <param name="Id">Plain name: letters, digits, dash, underscore and dot.</param>
/// <param name="Version">Semantic version of the plugin.</param>
/// <param name="Protocol">The <c>major.minor</c> the plugin speaks.</param>
/// <param name="Entry">Map from platform to the relative executable path.</param>
/// <param name="Permissions">What the plugin requests.</param>
/// <param name="Files">Map from relative path to <c>sha256:</c> digest, covering every file of the package.</param>
/// <param name="Limits">Optional limits the plugin asks for.</param>
public sealed record PluginManifest(string Id, string Version, string Protocol, IReadOnlyDictionary<string, string> Entry, PluginPermissions Permissions, IReadOnlyDictionary<string, string> Files, PluginLimits? Limits = null)
{
    /// <summary>JSON options shared by the reader and the writer.</summary>
    public static JsonSerializerOptions Json { get; } = new(JsonSerializerDefaults.Web) { WriteIndented = true };
}
