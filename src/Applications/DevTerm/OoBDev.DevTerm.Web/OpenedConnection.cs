namespace OoBDev.DevTerm.Web;

/// <summary>
/// The extra connections the host opens from its <c>--project</c> file, each its own <see cref="SessionHub"/>.
/// Shared by the REST endpoints and the Blazor <c>/connections</c> page so both see the same set.
/// </summary>
/// <summary>An opened project connection; <see cref="ControlPort"/> and <see cref="ControlToken"/> are set only when its profile asked for <c>ControlHttp</c>.</summary>
public sealed record OpenedConnection(string Id, string Name, int? ControlPort = null, string? ControlToken = null);
