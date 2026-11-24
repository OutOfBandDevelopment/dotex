using System.Collections.Generic;

namespace OoBDev.System.Sandbox;

/// <summary>What a manifest requests. Requests are not grants; a package that requests nothing gets no files and no network.</summary>
public sealed record PluginPermissions(IReadOnlyList<FilePermission>? Files = null, IReadOnlyList<NetworkPermission>? Network = null);
