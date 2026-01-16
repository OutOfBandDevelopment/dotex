using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore.Metadata;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Linq;

namespace OoBDev.DataLoader.DataReaders
{
    public class CsvDataFileReader : IDataFileReader
    {
#if NET5_0_OR_GREATER
        // IReadOnlyEntityType
        public bool CanReadFile(IReadOnlyEntityType entity, string filePath) =>
#else
        // IEntityType
        public bool CanReadFile(IEntityType entity, string filePath) =>
#endif
        string.Equals(".csv", Path.GetExtension(filePath), StringComparison.InvariantCultureIgnoreCase);

#if NET5_0_OR_GREATER
        // IReadOnlyEntityType
        public IEnumerable? ReadFile(IReadOnlyEntityType entity, string filePath)
#else
        // IEntityType
        public IEnumerable? ReadFile(IEntityType entity, string filePath)
#endif
        {
            using var reader = new StreamReader(filePath);
            var csvConfig = new CsvConfiguration(CultureInfo.CurrentCulture)
            {
                AllowComments = true,
                HasHeaderRecord = true,
                IgnoreBlankLines = true,
                LineBreakInQuotedFieldIsBadData = true,
                Delimiter = ",",
            };

            using var csv = new CsvReader(reader, csvConfig);

            if (csv.Read() && !csv.ReadHeader())
                throw new InvalidOperationException("Headers are missing");

            var headers = csv.HeaderRecord?.Select(h => h.Trim()).ToArray() ??
                throw new InvalidOperationException("Headers are missing");

            while (csv.Parser.Read())
            {
                var asJson = ToJson(headers, csv.Parser.Record);

                asJson[DataFileReaderProvider.DataSourceFilePath] = filePath;

                yield return asJson;
            }
        }

        private JObject ToJson(string[] headers, string[] values) =>
            new JObject(headers.Zip(values, (h, v) => new JProperty(h, v == "NULL" ? null : v)));
    }
}
