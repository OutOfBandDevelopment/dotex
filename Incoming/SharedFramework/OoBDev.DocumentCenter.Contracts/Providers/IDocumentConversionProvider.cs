using System.Threading.Tasks;

namespace OoBDev.DocumentCenter.Contracts.Providers
{
    public interface IDocumentConversionProvider
    {
        Task<byte[]?> ConvertAsync(DocumentTypes inputType, byte[] input, DocumentTypes outputType);
    }
}
