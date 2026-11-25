namespace OoBDev.DevTerm;

/// <summary>What the user is asked to approve: a program that will run with their rights.</summary>
public sealed record PluginApprovalRequest(string Name, string Version, string Folder, string CommandLine, string Hash);
