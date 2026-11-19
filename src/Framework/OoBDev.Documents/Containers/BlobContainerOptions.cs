using System;
using System.Collections.Generic;

namespace OoBDev.Documents.Containers;

/// <summary>
/// Chooses the provider of each blob container by configuration (section <c>BlobContainerOptions</c>). A container that is not listed
/// keeps the default resolution: a keyed provider, the first provider factory that offers it, then the default provider.
/// </summary>
public class BlobContainerOptions
{
    /// <summary>The provider key per container name, for example <c>{ "invoices": "aws-s3", "exports": "file-system" }</c>.</summary>
    public Dictionary<string, string> Providers { get; set; } = new(StringComparer.Ordinal);
}
