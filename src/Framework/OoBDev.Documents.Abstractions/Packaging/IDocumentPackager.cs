using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
namespace OoBDev.Documents.Packaging;

/// <summary>Packs documents by package type.</summary>
public interface IDocumentPackager
{
    /// <summary>Packs the items with the handler for the type.</summary>
    /// <param name="packageType">The package type, for example <c>zip</c> (case is ignored).</param>
    /// <param name="items">The documents.</param>
    /// <param name="destination">The stream that receives the package.</param>
    /// <param name="cancellationToken">Cancels the write.</param>
    /// <exception cref="PackagingException">No handler exists for the type, or an entry name is empty, rooted, escapes the package or repeats.</exception>
    Task PackAsync(string packageType, IReadOnlyCollection<PackageItem> items, Stream destination, CancellationToken cancellationToken = default);
}
