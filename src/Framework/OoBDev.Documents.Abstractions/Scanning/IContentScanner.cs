using OoBDev.Documents.Models;
using System.Threading.Tasks;

namespace OoBDev.Documents.Scanning;

/// <summary>Checks content for malware before it is stored. An adapter (for example ClamAV) implements it.</summary>
public interface IContentScanner
{
    /// <summary>Scans the content. The scanner may read the stream; the caller rewinds it afterwards.</summary>
    /// <param name="reference">The content to scan.</param>
    /// <returns>The verdict.</returns>
    Task<ContentScanResult> ScanAsync(ContentReference reference);
}
