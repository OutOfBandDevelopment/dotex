using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
namespace OoBDev.Documents.Packaging;

/// <summary>Writes documents into one package format.</summary>
public interface IDocumentPackageHandler
{
    /// <summary>The package type this handler writes, for example <c>zip</c>.</summary>
    string PackageType { get; }

    /// <summary>Writes the items to the stream; the original content is not changed.</summary>
    /// <param name="items">The documents.</param>
    /// <param name="destination">The stream that receives the package.</param>
    /// <param name="cancellationToken">Cancels the write.</param>
    Task PackAsync(IReadOnlyCollection<PackageItem> items, Stream destination, CancellationToken cancellationToken = default);
}
