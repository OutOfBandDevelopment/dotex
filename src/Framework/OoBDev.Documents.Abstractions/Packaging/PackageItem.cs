using System;
namespace OoBDev.Documents.Packaging;

/// <summary>One document to put in a package.</summary>
/// <param name="EntryName">The path of the entry inside the package, with forward slashes.</param>
/// <param name="Content">The document bytes.</param>
public sealed record PackageItem(string EntryName, ReadOnlyMemory<byte> Content);
