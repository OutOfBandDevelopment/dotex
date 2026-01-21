using System.Threading.Tasks;

namespace OoBDev.DocumentCenter.Abstractions.Providers
{
    public interface IDocumentConversionProvider
    {
        Task<byte[]?> ConvertAsync(DocumentTypes inputType, byte[] input, DocumentTypes outputType);
    }
}
