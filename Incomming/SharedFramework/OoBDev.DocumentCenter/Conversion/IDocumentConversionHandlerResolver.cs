using OoBDev.DocumentCenter.Contracts;
using OoBDev.DocumentCenter.Contracts.Handlers;

namespace OoBDev.DocumentCenter.Conversion
{
    public interface IDocumentConversionHandlerResolver
    {
        IDocumentConversionHandler GetHandler(DocumentTypes inputType, DocumentTypes outputType);
    }
}
