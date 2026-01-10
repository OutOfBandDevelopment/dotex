using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Collections.Generic;

namespace OoBDev.DataLoader.PipeLine
{
    public interface IDataPipelineProcessor
    {
#if NET5_0_OR_GREATER
        object ? ConvertToEntity(DbContext context, IReadOnlyEntityType entityType, object ? item);
        IEnumerable<object> ConvertToEntities(DbContext context, IReadOnlyEntityType entityType, IEnumerable<object> items);
#else
        object ? ConvertToEntity(DbContext context, IEntityType entityType, object? item);
        IEnumerable<object> ConvertToEntities(DbContext context, IEntityType entityType, IEnumerable<object> items);
#endif

    }
}