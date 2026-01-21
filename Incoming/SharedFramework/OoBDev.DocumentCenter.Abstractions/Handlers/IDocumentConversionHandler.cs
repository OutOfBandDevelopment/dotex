using System.Threading.Tasks;

namespace OoBDev.DocumentCenter.Abstractions.Handlers
{
    public interface IDocumentConversionHandler
    {
        Task<byte[]?> ConvertAsync(DocumentTypes inputType, byte[] input, DocumentTypes outputType);
    }
}
