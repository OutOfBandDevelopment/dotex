namespace OoBDev.DevTerm.Configuration.Discovery;

/// <summary>One resource record from a DNS answer, decoded for the types DNS-SD needs.</summary>
/// <param name="Name">The owner name.</param>
/// <param name="Type">1 A, 12 PTR, 16 TXT, 33 SRV.</param>
/// <param name="Target">PTR target, SRV target host, or the dotted address for an A record.</param>
/// <param name="Port">The SRV port, otherwise 0.</param>
public sealed record DnsRecord(string Name, int Type, string Target, int Port);
