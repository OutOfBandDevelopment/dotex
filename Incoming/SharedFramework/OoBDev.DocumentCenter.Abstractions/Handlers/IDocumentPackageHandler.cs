using OoBDev.DocumentCenter.Contracts.Storage;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OoBDev.DocumentCenter.Abstractions.Handlers
{
    public interface IDocumentPackageHandler
    {
        Task<byte[]> PackageAsync(IEnumerable<IDocumentRequestReference> documents);
    }
}
