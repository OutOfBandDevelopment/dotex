using Microsoft.EntityFrameworkCore.Metadata;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.IO;

namespace OoBDev.DataLoader.DataReaders
{
    public class JsonDataFileReader : IDataFileReader
    {
#if NET5_0_OR_GREATER
        public bool CanReadFile(IReadOnlyEntityType entity, string filePath) =>
#else
        public bool CanReadFile(IEntityType entity, string filePath) =>
#endif
        string.Equals(".json", Path.GetExtension(filePath), StringComparison.InvariantCultureIgnoreCase);

#if NET5_0_OR_GREATER
        public IEnumerable? ReadFile(IReadOnlyEntityType entity, string filePath)
#else
        public IEnumerable? ReadFile(IEntityType entity, string filePath)
#endif
        {
            var text = File.ReadAllText(filePath);
            var jsonData = JsonConvert.DeserializeObject(text);
            var array = jsonData as JArray;

            foreach (var item in array ?? new JArray())
            {
                item[DataFileReaderProvider.DataSourceFilePath] = filePath;
                yield return item;
            }
        }
    }
}
