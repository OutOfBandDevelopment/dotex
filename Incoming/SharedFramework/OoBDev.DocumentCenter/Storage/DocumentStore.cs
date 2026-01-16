using OoBDev.DocumentCenter.Contracts;
using OoBDev.DocumentCenter.Contracts.Storage;
using OoBDev.Toolkit.Contracts.Common;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;

namespace OoBDev.DocumentCenter.Storage
{
    [BlobContainer(ContainerName = SystemContainers.FileStore)]
    public class DocumentStore : IDocumentStore
    {
        private readonly IDocumentKeyGenerator _key;
        private readonly IBlobContainerClient<DocumentStore> _client;
        private readonly ILogger<DocumentStore> _log;
        private readonly IDocumentTypeResolver _resolver;
        private readonly IDateTools _date;
        private readonly IValidateContent _contentValidation;

        public DocumentStore(
            IDocumentKeyGenerator key,
            IBlobContainerClient<DocumentStore> client,
            ILogger<DocumentStore> log,
            IDocumentTypeResolver resolver,
            IDateTools date,
            IValidateContent contentValidation
            )
        {
            _key = key;
            _client = client;
            _log = log;
            _resolver = resolver;
            _date = date;
            _contentValidation = contentValidation;
        }

        public Task<IDocumentContentResult?> GetAsync(string key) => GetAsync(key, null);
        public async Task<IDocumentContentResult?> GetAsync(string key, string? container)
        {
            if (string.IsNullOrWhiteSpace(container))
            {
                _log.LogInformation($"Container: not provided so \"{this._client.ContainerName}\" will be used");
                container = this._client.ContainerName;
            }
            else if (this._client.ContainerName != container)
            {
                //TODO: we should probably resolve the client per the requested container
                _log.LogWarning($"Container: \"{container}\" requested but \"{this._client.ContainerName}\" will be used");
                container = this._client.ContainerName;
            }

            var result = await _client.DownloadAsync(key).ConfigureAwait(false);
            if (result == null || result.Content == null) throw new FileNotFoundException($"For file found at \"{key}:{container}\"", $"{key}:{container}");

            var docType = _resolver.GetByMimeOrFileName(result.ContentType, key);

            return new DocumentContentResult
            {
                Content = result.Content,
                ContentType = result.ContentType ?? "application/octet-stream",
                DocumentType = docType,
                FileName = key,
            };
        }

        public Task<IDocumentStoreResult?> StoreAsync(byte[] content, DocumentTypes contentType) =>
            StoreAsyncInternal(null, content, _resolver.GetMimeType(contentType));

        public Task<IDocumentStoreResult?> StoreAsync(Stream content, DocumentTypes contentType) =>
            StoreAsyncInternal(null, content, _resolver.GetMimeType(contentType));

        public Task<IDocumentStoreResult?> StoreAsync(byte[] content, string contentType) =>
            StoreAsyncInternal(null, content, contentType);

        public Task<IDocumentStoreResult?> StoreAsync(Stream content, string contentType) =>
            StoreAsyncInternal(null, content, contentType);

        public Task<IDocumentStoreResult?> StoreAsync(string fileName, byte[] content) =>
            StoreAsyncInternal(fileName, content, null);

        public Task<IDocumentStoreResult?> StoreAsync(string fileName, Stream content) =>
            StoreAsyncInternal(fileName, content, null);

        public Task<IDocumentStoreResult?> StoreAsync(string fileName, byte[] content, string contentType) =>
            StoreAsyncInternal(fileName, content, contentType);

        public Task<IDocumentStoreResult?> StoreAsync(string fileName, Stream content, string contentType) =>
            StoreAsyncInternal(fileName, content, contentType);

        public Task<IDocumentStoreResult?> StoreAsync(string fileName, byte[] content, DocumentTypes contentType) =>
            StoreAsyncInternal(fileName, content, _resolver.GetMimeType(contentType));

        public Task<IDocumentStoreResult?> StoreAsync(string fileName, Stream content, DocumentTypes contentType) =>
            StoreAsyncInternal(fileName, content, _resolver.GetMimeType(contentType));

        private async Task<IDocumentStoreResult?> StoreAsyncInternal(string? fileName, byte[] content, string? contentType)
        {
            if (content == null || content.Length == 0) return null;
            using var stream = new MemoryStream(content);
            return await StoreAsyncInternal(fileName, stream, contentType).ConfigureAwait(false);
        }
        private async Task<IDocumentStoreResult?> StoreAsyncInternal(string? fileName, Stream content, string? contentType)
        {
            if (content == null || (content.CanSeek && content.Length == 0)) return null;
            var timeStamp = _date.Now();

            if (string.IsNullOrWhiteSpace(fileName))
            {
                fileName = _resolver.GenerateFileName(contentType);
            }
            if (string.IsNullOrWhiteSpace(contentType))
            {
                contentType = _resolver.GetMimeType(_resolver.GetByFileName(fileName));
            }


            try
            {
                await _contentValidation.EnsureValidContentAsync(content, fileName, contentType).ConfigureAwait(false);

                var fileKey = _key.Generate(fileName, timeStamp);
                _log.LogDebug($"Uploading \"{fileName}\" as \"{fileKey}\"");

                await _client.UploadAsync(name: fileKey, data: content, contentType: contentType).ConfigureAwait(false);

                _log.LogDebug($"Uploaded \"{fileName}\" as \"{_client.ContainerName}/{fileKey}\"");
                return new DocumentStoreResult
                {
                    Key = fileKey,
                    Container = _client.ContainerName,
                };
            }
            catch (Exception ex)
            {
                _log.LogError(ex.Message);
                _log.LogDebug(ex.ToString());
                throw;
            }
        }
    }
}