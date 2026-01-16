using OoBDev.DocumentCenter.Contracts;
using OoBDev.DocumentCenter.Contracts.Providers;
using OoBDev.DocumentCenter.Contracts.Storage;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OoBDev.DocumentCenter.Packaging
{

    public class DocumentPackageProvider : IDocumentPackageProvider
    {
        private readonly IDocumentPackageHandlerResolver _resolver;

        public DocumentPackageProvider(
            IDocumentPackageHandlerResolver resolver
            )
        {
            _resolver = resolver;
        }

        public Task<byte[]> PackageAsync(PackageTypes packageType, IEnumerable<IDocumentRequestReference> documents) =>
            _resolver.GetHandler(packageType).PackageAsync(documents);
    }
}
