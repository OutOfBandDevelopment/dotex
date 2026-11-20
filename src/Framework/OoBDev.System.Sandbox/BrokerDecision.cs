namespace OoBDev.Spike.PluginSandbox;

/// <summary>The broker's answer to a plugin's resource request; a refusal is logged (REQ-003).</summary>
public sealed record BrokerDecision(bool Allowed, string Reason);
