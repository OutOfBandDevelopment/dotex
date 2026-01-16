using OoBDev.DataLoader.Providers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OoBDev.DataLoader.DataReaders
{
    public class DataFileProvider : IDataFileProvider
    {
        private readonly static IReadOnlyCollection<string> _extensions = new[] { ".json", ".csv" };

        private readonly IDataEntityProvider _entities;

        public DataFileProvider(IDataEntityProvider entities)
        {
            _entities = entities;
        }

#if NET5_0_OR_GREATER
        public IEnumerable<(IReadOnlyEntityType entity, string filePath)> GetDataFiles(string basePath, DbContext dbContext) =>
#else
        public IEnumerable<(IEntityType entity, string filePath)> GetDataFiles(string basePath, DbContext dbContext) =>
#endif
        from entity in _entities.GetEntityTypes(dbContext)
            from file in GetDataFiles(basePath, entity)
            select (entity, file);

#if NET5_0_OR_GREATER
        public IEnumerable<string> GetDataFiles(string basePath, IReadOnlyEntityType entity)
#else
        public IEnumerable<string> GetDataFiles(string basePath, IEntityType entity)
#endif
        {
            var realBasePath = Path.GetFullPath(basePath);

            var schema = entity.GetSchema();
            var table = entity.GetTableName();
            var entityName = entity.ShortName();

            var fileNames = new[]
            {
                $"{schema}.{table}",
                $"{schema}.{entityName}",
                $"[{schema}].[{table}]",
                $"[{schema}].[{entityName}]",
            };

            var possibleNames = from ext in _extensions
                                from fn in fileNames
                                select $"{fn}{ext}";

            var fullNames = from name in possibleNames
                            select Path.GetFullPath(Path.Combine(realBasePath, name));

            var filtered = from fullPath in fullNames
                           where fullPath.StartsWith(realBasePath) //remove files that try to escape basePath
                           where File.Exists(fullPath) //remove files that don't exist
                           select fullPath;

            return filtered;
        }
    }
}
