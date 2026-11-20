namespace OoBDev.Spike.PluginSandbox;

/// <summary>The outcome of <see cref="PluginHost.Start"/>: a lease on the plugin, or the reason there is none.</summary>
/// <param name="Refusal">None when started.</param>
/// <param name="Detail">The package refusal or a short explanation.</param>
/// <param name="Lease">The lease, present only when started; dispose it to release the plugin.</param>
public sealed record HostStart(HostRefusal Refusal, string Detail, PluginLease<BrokeredPlugin>? Lease);
