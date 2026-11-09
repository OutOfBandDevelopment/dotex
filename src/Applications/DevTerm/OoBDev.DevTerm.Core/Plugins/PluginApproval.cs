using System;

namespace OoBDev.DevTerm;

/// <summary>One remembered approval (<see cref="PluginTrust.Approvals"/>).</summary>
public sealed record PluginApproval(string Name, string Hash, DateTimeOffset ApprovedAt);
