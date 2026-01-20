using Microsoft.EntityFrameworkCore.Metadata;
using System.Collections;

namespace OoBDev.DataLoader.DataReaders
{
    public interface IDataFileReader
    {
#if NET5_0_OR_GREATER
        bool CanReadFile(IReadOnlyEntityType entity, string filePath);
        IEnumerable? ReadFile(IReadOnlyEntityType entity, string filePath);
#else
        bool CanReadFile(IEntityType entity, string filePath);
        IEnumerable? ReadFile(IEntityType entity, string filePath);
#endif
    }
}
