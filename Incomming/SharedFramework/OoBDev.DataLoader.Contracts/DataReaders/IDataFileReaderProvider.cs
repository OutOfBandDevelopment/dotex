using Microsoft.EntityFrameworkCore.Metadata;
using System.Collections.Generic;

namespace OoBDev.DataLoader.DataReaders
{
    public interface IDataFileReaderProvider
    {
#if NET5_0_OR_GREATER
        IEnumerable<object> ReadFile(IReadOnlyEntityType entity, string filePath);
#else
        IEnumerable<object> ReadFile(IEntityType entity, string filePath);
#endif
    }
}
