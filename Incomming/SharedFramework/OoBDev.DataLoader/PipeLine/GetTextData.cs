using OoBDev.DataLoader.DataReaders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;

namespace OoBDev.DataLoader.PipeLine
{
    public class GetTextData : IDataPipelineHandler
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger _logger;

        public const string __GetText = nameof(__GetText);

        public GetTextData(
            IServiceProvider serviceProvider,
            ILogger<GetTextData> logger
            )
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

#if NET5_0_OR_GREATER
        // IReadOnlyEntityType
        public JToken Handle(DbContext context, IReadOnlyEntityType entityType, JToken input)
#else
        public JToken Handle(DbContext context, IEntityType entityType, JToken input)
#endif
        {
            if (!(input is JObject json))
            {
                _logger.LogWarning($"{input} is not of type {nameof(JObject)}");
                return input;
            }

            if (input.ToString().Contains(__GetText))
            {

            }

            var sourcePath = (string?)input[DataFileReaderProvider.DataSourceFilePath];

            var children = from jp in json[__GetText]?.Value<JObject>()?.Properties() ?? Enumerable.Empty<JProperty>()
                           select new
                           {
                               jp.Name,
                               jp.Value,
                           };
            var direct = from jp in json.Properties() ?? Enumerable.Empty<JProperty>()
                         where jp.Name.StartsWith(__GetText + "_")
                         select new
                         {
                             Name = jp.Name[(__GetText.Length + 1)..],
                             jp.Value,
                         };

            var properties = children.Concat(direct).ToArray();

            foreach (var property in properties)
            {
                if (string.IsNullOrWhiteSpace(property.Name)) continue;
                if (property.Value.Type == JTokenType.String)
                {
                    var sourceFilePath = property.Value.Value<string>();

                    var path = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(sourcePath), sourceFilePath));
                    if (File.Exists(path))
                    {
                        var fileName = Path.GetFileName(path);
                        _logger.LogInformation($"Read file for {{{nameof(property.Name)}}} from {{{nameof(fileName)}}}", property.Name, fileName);

                        var content = File.ReadAllText(path);
                        json[property.Name] = content;
                    }
                    else
                    {
                        _logger.LogWarning("File not found \"{sourceFilePath}\" for {input}", sourceFilePath, input);
                    }
                }

                property.Value.Parent?.Remove();
            }

            return input;
        }
    }
}
