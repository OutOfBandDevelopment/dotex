using OoBDev.DocumentCenter.Contracts;
using OoBDev.DocumentCenter.Contracts.Providers;
using OoBDev.DocumentCenter.Contracts.Storage;
using OoBDev.DocumentCenter.Storage;
using OoBDev.Toolkit.Contracts.Common;
using OoBDev.Toolkit.Contracts.IO;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OoBDev.DocumentCenter.Packaging
{
    public class DocumentPackager : IDocumentPackager
    {
        private readonly IDocumentTypeResolver _resolver;
        private readonly IDocumentPackageProvider _provider;
        private readonly IDateTools _date;

        public DocumentPackager(
            IDocumentTypeResolver resolver,
            IDocumentPackageProvider provider,
            IDateTools date,
#pragma warning disable IDE0060 // Remove unused parameter
            ITempFileFactory temp
#pragma warning restore IDE0060 // Remove unused parameter
            )
        {
            _resolver = resolver;
            _provider = provider;
            _date = date;
        }

        public Task<IDocumentContentResult> PackToAsync(PackageTypes packageType, params string[] keys) => PackToAsync(packageType, keys.AsEnumerable());
        public Task<IDocumentContentResult> PackToAsync(PackageTypes packageType, string? container, params string[] keys) => PackToAsync(packageType, container, keys.AsEnumerable());
        public Task<IDocumentContentResult> PackToAsync(PackageTypes packageType, params (string key, string? container)[] documents) => PackToAsync(packageType, documents.AsEnumerable());
        public Task<IDocumentContentResult> PackToAsync(PackageTypes packageType, IEnumerable<string> keys) => PackToAsync(packageType, null, keys.AsEnumerable());
        public Task<IDocumentContentResult> PackToAsync(PackageTypes packageType, string? container, IEnumerable<string> keys) => PackToAsync(packageType, keys.Select(k => (k, container)));

        public Task<IDocumentContentResult> PackToAsync(PackageTypes packageType, IEnumerable<(string key, string? container)> documents) =>
            PackToAsync(packageType, documents.Select(doc => (doc.key, doc.container, (string?)null)));

        public Task<IDocumentContentResult> PackToAsync(PackageTypes packageType, IEnumerable<(string key, string? container, string? filename)> documents) =>
            PackToAsync(packageType, documents.Select(doc => new DocumentRequestReference { Key = doc.key, Container = doc.container, FileName = doc.filename }));

        public async Task<IDocumentContentResult> PackToAsync(PackageTypes packageType, IEnumerable<IDocumentRequestReference> documents)
        {
            var packageDocumentType = _resolver.GetByPackageType(packageTypes: packageType);
            var packagedData = await _provider.PackageAsync(packageType, documents).ConfigureAwait(false);
            return new DocumentContentResult
            {
                Content = packagedData,
                ContentType = _resolver.GetMimeType(packageDocumentType),
                DocumentType = packageDocumentType,
                FileName = Path.ChangeExtension($"{_date.Now():yyyyMMddHHmmss}.bin", _resolver.GetExtension(packageDocumentType)),
            };
        }
    }
}
