namespace OoBDev.System.Sandbox;

/// <summary>A folder a plugin asks for (or is granted). <paramref name="Access"/> is <c>ro</c> or <c>rw</c>.</summary>
public sealed record FilePermission(string Path, string Access);
