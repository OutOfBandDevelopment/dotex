using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace OoBDev.DocumentCenter.Contracts.Storage
{
    public interface IBlobContainerClient
    {
        string ContainerName { get; }

        Task UploadAsync(string name, Stream data, string contentType);
        Task<IBlobContentResult?> DownloadAsync(string key);
        Task<bool> DeleteAsync(string key);
        IAsyncEnumerable<IBlobContentInfoResult> ListAllAsync();
    }
    public interface IBlobContainerClient<T> : IBlobContainerClient
    {
    }
}