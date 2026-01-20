using OoBDev.DocumentCenter.Contracts.Storage;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OoBDev.DocumentCenter.Abstractions
{
    public interface IDocumentPackager
    {
        Task<IDocumentContentResult> PackToAsync(PackageTypes packageType, params string[] keys);
        Task<IDocumentContentResult> PackToAsync(PackageTypes packageType, string? container, params string[] keys);
        Task<IDocumentContentResult> PackToAsync(PackageTypes packageType, params (string key, string? container)[] documents);
        Task<IDocumentContentResult> PackToAsync(PackageTypes packageType, IEnumerable<string> keys);
        Task<IDocumentContentResult> PackToAsync(PackageTypes packageType, string container, IEnumerable<string> keys);
        Task<IDocumentContentResult> PackToAsync(PackageTypes packageType, IEnumerable<(string key, string? container)> documents);
        Task<IDocumentContentResult> PackToAsync(PackageTypes packageType, IEnumerable<(string key, string? container, string? filename)> documents);
        Task<IDocumentContentResult> PackToAsync(PackageTypes packageType, IEnumerable<IDocumentRequestReference> documents);
    }
}
