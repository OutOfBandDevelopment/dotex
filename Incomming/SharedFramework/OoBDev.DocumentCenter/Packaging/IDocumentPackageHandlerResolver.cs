using OoBDev.DocumentCenter.Contracts;
using OoBDev.DocumentCenter.Contracts.Handlers;

namespace OoBDev.DocumentCenter.Packaging
{
    public interface IDocumentPackageHandlerResolver
    {
        IDocumentPackageHandler GetHandler(PackageTypes packageType);
    }
}
