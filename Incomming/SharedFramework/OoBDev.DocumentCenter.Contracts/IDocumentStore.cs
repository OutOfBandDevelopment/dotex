using OoBDev.DocumentCenter.Contracts.Storage;
using System.IO;
using System.Threading.Tasks;

namespace OoBDev.DocumentCenter.Contracts
{
    public interface IDocumentStore
    {

        Task<IDocumentStoreResult?> StoreAsync(byte[] content, DocumentTypes contentType);
        Task<IDocumentStoreResult?> StoreAsync(Stream content, DocumentTypes contentType);
        Task<IDocumentStoreResult?> StoreAsync(byte[] content, string contentType);
        Task<IDocumentStoreResult?> StoreAsync(Stream content, string contentType);
        Task<IDocumentStoreResult?> StoreAsync(string fileName, byte[] content);
        Task<IDocumentStoreResult?> StoreAsync(string fileName, Stream content);
        Task<IDocumentStoreResult?> StoreAsync(string fileName, byte[] content, string contentType);
        Task<IDocumentStoreResult?> StoreAsync(string fileName, Stream content, string contentType);
        Task<IDocumentStoreResult?> StoreAsync(string fileName, byte[] content, DocumentTypes contentType);
        Task<IDocumentStoreResult?> StoreAsync(string fileName, Stream content, DocumentTypes contentType);

        Task<IDocumentContentResult?> GetAsync(string key);
        Task<IDocumentContentResult?> GetAsync(string key, string? container);
    }
}
