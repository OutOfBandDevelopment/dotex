using OoBDev.DocumentCenter.Contracts.Storage;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OoBDev.DocumentCenter.Contracts.Providers
{
    public interface IDocumentPackageProvider
    {
        Task<byte[]> PackageAsync(PackageTypes packageType, IEnumerable<IDocumentRequestReference> documents);
    }
}
