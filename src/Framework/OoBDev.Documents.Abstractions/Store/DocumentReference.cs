namespace OoBDev.Documents.Store;

/// <summary>Names a stored document: the container it lives in and its key.</summary>
/// <param name="Container">The blob container name.</param>
/// <param name="Key">The key returned when the document was stored.</param>
public sealed record DocumentReference(string Container, string Key);
