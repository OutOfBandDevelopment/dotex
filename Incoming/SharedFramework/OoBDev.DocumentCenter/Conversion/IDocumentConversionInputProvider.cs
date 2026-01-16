using OoBDev.DocumentCenter.Contracts;
using OoBDev.DocumentCenter.Contracts.Handlers;
using System.Collections.Generic;

namespace OoBDev.DocumentCenter.Conversion
{
    public interface IDocumentConversionInputProvider
    {
        IEnumerable<(IDocumentConversionHandler handler, DocumentTypes output)> GetInputHandlers(DocumentTypes inputType);
    }
}
