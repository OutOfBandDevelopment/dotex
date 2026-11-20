namespace OoBDev.System.Sandbox;

/// <summary>The result of a brokered fetch: the decision, and for an allowed call the status and body.</summary>
public sealed record BrokerFetch(BrokerDecision Decision, int Status, byte[] Body);
