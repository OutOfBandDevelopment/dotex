using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Collections.Generic;

namespace OoBDev.DataLoader.Providers
{
    public interface IDataEntityProvider
    {
#if NET5_0_OR_GREATER
        IEnumerable<IReadOnlyEntityType> GetEntityTypes(DbContext context);
#else
        IEnumerable<IEntityType> GetEntityTypes(DbContext context);
#endif
    }
}
