using System;

namespace OoBDev.Spike.PluginSandbox;

/// <summary>One line of the grant audit trail: an approval, or a broker call that was allowed or refused (REQ-002, REQ-003).</summary>
public sealed record AuditEntry(DateTimeOffset When, string PluginId, string Action, string Target, bool Allowed, string Reason);
