using Microsoft.EntityFrameworkCore.Metadata;
using System.Collections.Generic;
using System.Linq;

namespace OoBDev.DataLoader.DataReaders
{
    public class DataFileReaderProvider : IDataFileReaderProvider
    {
        public const string DataSourceFilePath = "__DataSourceFilePath";

        private readonly IEnumerable<IDataFileReader> _readers;

        public DataFileReaderProvider(
            IEnumerable<IDataFileReader> readers
            )
        {
            _readers = readers;
        }

#if NET5_0_OR_GREATER
        public IEnumerable<object> ReadFile(IReadOnlyEntityType entity, string filePath) =>
#else
        public IEnumerable<object> ReadFile(IEntityType entity, string filePath) =>
#endif
            from reader in _readers
            where reader.CanReadFile(entity, filePath)
            let data = reader.ReadFile(entity, filePath)
            where data != null
            from item in data.OfType<object>()
            where item != null
            select item;
    }
}
