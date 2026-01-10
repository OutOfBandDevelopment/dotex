using System.Threading.Tasks;

namespace OoBDev.DocumentCenter.Contracts.Handlers
{
    public interface IDocumentConversionHandler
    {
        Task<byte[]?> ConvertAsync(DocumentTypes inputType, byte[] input, DocumentTypes outputType);
    }
}
