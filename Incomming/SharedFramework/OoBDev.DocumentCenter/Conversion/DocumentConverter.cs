using OoBDev.DocumentCenter.Contracts;
using OoBDev.DocumentCenter.Contracts.Providers;
using OoBDev.DocumentCenter.Contracts.Storage;
using OoBDev.DocumentCenter.Storage;
using OoBDev.Toolkit.Contracts.Common;
using System;
using System.IO;
using System.Threading.Tasks;

namespace OoBDev.DocumentCenter.Conversion
{
    public class DocumentConverter : IDocumentConverter
    {
        private readonly IDocumentStore _store;
        private readonly IDocumentTypeResolver _resolver;
        private readonly IDocumentConversionProvider _provider;
        private readonly IStreamTools _stream;

        public DocumentConverter(
            IDocumentStore store,
            IDocumentTypeResolver resolver,
            IDocumentConversionProvider provider,
            IStreamTools stream
            )
        {
            _store = store;
            _resolver = resolver;
            _provider = provider;
            _stream = stream;
        }

        #region ConvertToAndStoreAsync

        public async Task<IDocumentStoreResult?> ConvertToAndStoreAsync(string key, DocumentTypes outputType)
        {
            var converted = await this.ConvertToAsync(key, outputType).ConfigureAwait(false);
            if (converted?.Content == null) return null;
            var result = await _store.StoreAsync(converted.FileName, converted.Content, converted.DocumentType).ConfigureAwait(false);
            return result;
        }

        public async Task<IDocumentStoreResult?> ConvertToAndStoreAsync(string key, string? container, DocumentTypes outputType)
        {
            var converted = await this.ConvertToAsync(key, container, outputType).ConfigureAwait(false);
            if (converted?.Content == null) return null;
            return await _store.StoreAsync(converted.FileName, converted.Content, converted.DocumentType).ConfigureAwait(false);
        }

        public async Task<IDocumentStoreResult?> ConvertToAndStoreAsync(DocumentTypes inputType, byte[] content, DocumentTypes outputType)
        {
            var converted = await this.ConvertToAsync(inputType, content, outputType).ConfigureAwait(false);
            if (converted?.Content == null) return null;
            return await _store.StoreAsync(converted.FileName, converted.Content, converted.DocumentType).ConfigureAwait(false);
        }

        public async Task<IDocumentStoreResult?> ConvertToAndStoreAsync(DocumentTypes inputType, Stream content, DocumentTypes outputType)
        {
            var converted = await this.ConvertToAsync(inputType, content, outputType).ConfigureAwait(false);
            if (converted?.Content == null) return null;
            return await _store.StoreAsync(converted.FileName, converted.Content, converted.DocumentType).ConfigureAwait(false);
        }

        public async Task<IDocumentStoreResult?> ConvertToAndStoreAsync(string fileName, byte[] content, DocumentTypes outputType)
        {
            var converted = await this.ConvertToAsync(fileName, content, outputType).ConfigureAwait(false);
            if (converted?.Content == null) return null;
            return await _store.StoreAsync(converted.FileName, converted.Content, converted.DocumentType).ConfigureAwait(false);
        }

        public async Task<IDocumentStoreResult?> ConvertToAndStoreAsync(string fileName, Stream content, DocumentTypes outputType)
        {
            var converted = await this.ConvertToAsync(fileName, content, outputType).ConfigureAwait(false);
            if (converted?.Content == null) return null;
            return await _store.StoreAsync(converted.FileName, converted.Content, converted.DocumentType).ConfigureAwait(false);
        }

        public async Task<IDocumentStoreResult?> ConvertToAndStoreAsync(string fileName, byte[] content, string contentType, DocumentTypes outputType)
        {
            var converted = await this.ConvertToAsync(fileName, content, contentType, outputType).ConfigureAwait(false);
            if (converted?.Content == null) return null;
            return await _store.StoreAsync(converted.FileName, converted.Content, converted.DocumentType).ConfigureAwait(false);
        }

        public async Task<IDocumentStoreResult?> ConvertToAndStoreAsync(string fileName, Stream content, string contentType, DocumentTypes outputType)
        {
            var converted = await this.ConvertToAsync(fileName, content, contentType, outputType).ConfigureAwait(false);
            if (converted?.Content == null) return null;
            return await _store.StoreAsync(converted.FileName, converted.Content).ConfigureAwait(false);
        }

        #endregion ConvertToAndStoreAsync

        #region ConvertToAsync

        public async Task<IDocumentContentResult?> ConvertToAsync(string key, DocumentTypes outputType)
        {
            var content = await _store.GetAsync(key).ConfigureAwait(false);
            if (content?.Content == null) return null;
            var converted = await _provider.ConvertAsync(content.DocumentType, content.Content, outputType).ConfigureAwait(false);
            var result = new DocumentContentResult
            {
                Content = converted,
                ContentType = _resolver.GetMimeType(outputType),
                DocumentType = outputType,
                FileName = Path.ChangeExtension(key, _resolver.GetExtension(outputType)),
            };
            return result;
        }

        public async Task<IDocumentContentResult?> ConvertToAsync(string key, string? container, DocumentTypes outputType)
        {
            var content = await _store.GetAsync(key, container).ConfigureAwait(false);
            if (content?.Content == null) return null;
            var converted = await _provider.ConvertAsync(content.DocumentType, content.Content, outputType).ConfigureAwait(false);
            var result = new DocumentContentResult
            {
                Content = converted,
                ContentType = _resolver.GetMimeType(outputType),
                DocumentType = outputType,
                FileName = Path.ChangeExtension(key, _resolver.GetExtension(outputType)),
            };
            return result;
        }

        public async Task<IDocumentContentResult?> ConvertToAsync(DocumentTypes inputType, byte[] content, DocumentTypes outputType)
        {
            var converted = await _provider.ConvertAsync(inputType, content, outputType).ConfigureAwait(false);
            if (converted == null) return null;
            var result = new DocumentContentResult
            {
                Content = converted,
                ContentType = _resolver.GetMimeType(outputType),
                DocumentType = outputType,
                FileName = _resolver.GenerateFileName(outputType),
            };
            return result;
        }

        public async Task<IDocumentContentResult?> ConvertToAsync(DocumentTypes inputType, Stream content, DocumentTypes outputType) =>
            await ConvertToAsync(inputType, await _stream.ToArrayAsync(content).ConfigureAwait(false), outputType).ConfigureAwait(false);

        public async Task<IDocumentContentResult?> ConvertToAsync(string fileName, byte[] content, DocumentTypes outputType)
        {
            var inputType = _resolver.GetByFileName(fileName);
            var converted = await _provider.ConvertAsync(inputType, content, outputType).ConfigureAwait(false);
            if (converted == null) return null;
            var result = new DocumentContentResult
            {
                Content = converted,
                ContentType = _resolver.GetMimeType(outputType),
                DocumentType = outputType,
                FileName = Path.ChangeExtension(fileName, _resolver.GetExtension(outputType)),
            };
            return result;
        }

        public async Task<IDocumentContentResult?> ConvertToAsync(string fileName, Stream content, DocumentTypes outputType) =>
            await ConvertToAsync(fileName, await _stream.ToArrayAsync(content).ConfigureAwait(false), outputType).ConfigureAwait(false);

        public async Task<IDocumentContentResult?> ConvertToAsync(string fileName, byte[] content, string contentType, DocumentTypes outputType)
        {
            var inputType = _resolver.GetByMime(contentType);
            var converted = await _provider.ConvertAsync(inputType, content, outputType).ConfigureAwait(false);
            if (converted == null) return null;
            var result = new DocumentContentResult
            {
                Content = converted,
                ContentType = _resolver.GetMimeType(outputType),
                DocumentType = outputType,
                FileName = Path.ChangeExtension(fileName, _resolver.GetExtension(outputType)),
            };
            return result;
        }

        public async Task<IDocumentContentResult?> ConvertToAsync(string fileName, Stream content, string contentType, DocumentTypes outputType) =>
            await ConvertToAsync(fileName, await _stream.ToArrayAsync(content).ConfigureAwait(false), contentType, outputType).ConfigureAwait(false);

        #endregion ConvertToAsync
    }
}
