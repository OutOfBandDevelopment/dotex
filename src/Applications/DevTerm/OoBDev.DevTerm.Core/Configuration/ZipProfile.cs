namespace OoBDev.DevTerm.Configuration;

/// <summary>One profile read out of a zip by <see cref="ConnectionProfileStore.ReadZip"/>: its name and its raw JSON text.</summary>
public sealed record ZipProfile(string Name, string Json);
