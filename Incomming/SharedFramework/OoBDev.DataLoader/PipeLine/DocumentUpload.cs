using OoBDev.DataLoader.DataReaders;
using OoBDev.DocumentCenter.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.IO;

namespace OoBDev.DataLoader.PipeLine
{
    [DataPipelinePriority(Priority)]
    public class DocumentUpload : IDataPipelineHandler
    {
        private readonly ILogger _logger;
        private readonly IDocumentStore _documentStore;
        private readonly IServiceProvider _serviceProvider;

        public DocumentUpload(
            ILogger<DocumentUpload> logger,
            IDocumentStore documentStore,
            IServiceProvider serviceProvider
            )
        {
            _logger = logger;
            _documentStore = documentStore;
            _serviceProvider = serviceProvider;
        }

        public const int Priority = 10;

        public const string StorageContainer = nameof(StorageContainer);
        public const string StorageKey = nameof(StorageKey);
        public const string __SourceFilePath = nameof(__SourceFilePath);
        public const string __SourceFileContent = nameof(__SourceFileContent);

#if NET5_0_OR_GREATER
        // IReadOnlyEntityType
        public JToken Handle(DbContext context, IReadOnlyEntityType entityType, JToken input)
#else
        // IEntityType
        public JToken Handle(DbContext context, IEntityType entityType, JToken input)
#endif
        {
            var sourcePath = (string?)input[DataFileReaderProvider.DataSourceFilePath];

            if (!string.IsNullOrWhiteSpace(sourcePath) && input[__SourceFilePath] != null && input[StorageKey] == null)
            {
                _logger.LogDebug("Check if AlternativeKey exists for {input}", input);
                var existing = LookupAlternativeKey.GetLookup(_serviceProvider, entityType).Existing(context, entityType, input, entityType.FindPrimaryKey());
                if (existing != null)
                {
                    input[StorageContainer] = existing[StorageContainer];
                    input[StorageKey] = existing[StorageKey];
                    _logger.LogInformation("Set AlternativeKey for {input} as {container}:{key}", input, existing[StorageContainer], existing[StorageKey]);
                }

                _logger.LogInformation("Try uploading file ");

                var sourceFilePath = (string?)input[__SourceFilePath];
                if (input[StorageKey] == null && !string.IsNullOrWhiteSpace(sourceFilePath))
                {
                    var path = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(sourcePath), sourceFilePath));
                    if (File.Exists(path))
                    {
                        var fileName = Path.GetFileName(path);
                        if (input["Name"] == null)
                            input["Name"] = fileName;


                        _logger.LogInformation("Upload file for {fileName}", fileName);

                        var content = File.ReadAllBytes(path);
                        var contentType = (string?)input?[__SourceFileContent];
                        if (string.IsNullOrWhiteSpace(contentType)) contentType = "application/octet-stream";

                        var result = _documentStore.StoreAsync(fileName, content, contentType).GetAwaiter().GetResult();

                        if (result == null)
                        {
                            input[StorageContainer] = null;
                            input[StorageKey] = null;
                        }
                        else
                        {
                            input[StorageContainer] = result.Container;
                            input[StorageKey] = result.Key;
                        }
                    }
                    else
                    {
                        _logger.LogWarning("File not found \"{sourceFilePath}\" for {input}", sourceFilePath, input);
                    }
                }
            }

            input[__SourceFilePath]?.Parent?.Remove();
            input[__SourceFileContent]?.Parent?.Remove();

            return input;
        }
    }
}
