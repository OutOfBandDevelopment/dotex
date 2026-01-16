using OoBDev.DocumentCenter.Contracts.Storage;
using System.IO;
using System.Threading.Tasks;

namespace OoBDev.DocumentCenter.Contracts
{
    public interface IDocumentConverter
    {
        Task<IDocumentContentResult?> ConvertToAsync(string key, DocumentTypes outputType);
        Task<IDocumentContentResult?> ConvertToAsync(string key, string container, DocumentTypes outputType);
        Task<IDocumentContentResult?> ConvertToAsync(DocumentTypes inputType, byte[] content, DocumentTypes outputType);
        Task<IDocumentContentResult?> ConvertToAsync(DocumentTypes inputType, Stream content, DocumentTypes outputType);
        Task<IDocumentContentResult?> ConvertToAsync(string fileName, byte[] content, DocumentTypes outputType);
        Task<IDocumentContentResult?> ConvertToAsync(string fileName, Stream content, DocumentTypes outputType);
        Task<IDocumentContentResult?> ConvertToAsync(string fileName, byte[] content, string contentType, DocumentTypes outputType);
        Task<IDocumentContentResult?> ConvertToAsync(string fileName, Stream content, string contentType, DocumentTypes outputType);

        Task<IDocumentStoreResult?> ConvertToAndStoreAsync(string key, DocumentTypes outputType);
        Task<IDocumentStoreResult?> ConvertToAndStoreAsync(string key, string container, DocumentTypes outputType);
        Task<IDocumentStoreResult?> ConvertToAndStoreAsync(DocumentTypes inputType, byte[] content, DocumentTypes outputType);
        Task<IDocumentStoreResult?> ConvertToAndStoreAsync(DocumentTypes inputType, Stream content, DocumentTypes outputType);
        Task<IDocumentStoreResult?> ConvertToAndStoreAsync(string fileName, byte[] content, DocumentTypes outputType);
        Task<IDocumentStoreResult?> ConvertToAndStoreAsync(string fileName, Stream content, DocumentTypes outputType);
        Task<IDocumentStoreResult?> ConvertToAndStoreAsync(string fileName, byte[] content, string contentType, DocumentTypes outputType);
        Task<IDocumentStoreResult?> ConvertToAndStoreAsync(string fileName, Stream content, string contentType, DocumentTypes outputType);
    }
}
