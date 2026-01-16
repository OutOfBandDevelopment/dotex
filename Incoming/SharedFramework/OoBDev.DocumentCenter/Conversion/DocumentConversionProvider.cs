using OoBDev.DocumentCenter.Contracts;
using OoBDev.DocumentCenter.Contracts.Providers;
using System.Threading.Tasks;

namespace OoBDev.DocumentCenter.Conversion
{
    public class DocumentConversionProvider : IDocumentConversionProvider
    {
        private readonly IDocumentConversionHandlerResolver _resolver;

        public DocumentConversionProvider(
            IDocumentConversionHandlerResolver resolver
            )
        {
            _resolver = resolver;
        }

        public async Task<byte[]?> ConvertAsync(DocumentTypes inputType, byte[] input, DocumentTypes outputType) =>
            inputType == outputType ? 
                input :
                await _resolver.GetHandler(inputType, outputType)
                               .ConvertAsync(inputType, input, outputType)
                               .ConfigureAwait(false);
    }
}
