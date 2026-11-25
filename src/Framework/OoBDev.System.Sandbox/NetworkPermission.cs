namespace OoBDev.System.Sandbox;

/// <summary>An exact host and port a plugin asks for (or is granted); there are no wildcards in the first version (REQ-007).</summary>
public sealed record NetworkPermission(string Host, int Port);
