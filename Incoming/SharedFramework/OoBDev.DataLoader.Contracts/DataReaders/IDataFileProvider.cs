using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Collections.Generic;

namespace OoBDev.DataLoader.DataReaders
{
    public interface IDataFileProvider
    {
#if NET5_0_OR_GREATER
        IEnumerable<(IReadOnlyEntityType entity, string filePath)> GetDataFiles(string basePath, DbContext dbContext);
        IEnumerable<string> GetDataFiles(string basePath, IReadOnlyEntityType entity);
#else
        IEnumerable < (IEntityType entity, string filePath)> GetDataFiles(string basePath, DbContext dbContext);
        IEnumerable<string> GetDataFiles(string basePath, IEntityType entity);
#endif
    }
}