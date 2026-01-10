using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Collections.Generic;

namespace OoBDev.DataLoader.Providers
{
    public interface IMasterDataProvider
    {
#if NET5_0_OR_GREATER
        IEnumerable<(IReadOnlyEntityType entity, IEnumerable<object> masterData)> GetMasterData(DbContext dbContext);
        IEnumerable<object> GetMasterData(IReadOnlyEntityType entityType);
#else
        IEnumerable < (IEntityType entity, IEnumerable<object> masterData)> GetMasterData(DbContext dbContext);
        IEnumerable<object> GetMasterData(IEntityType entityType);
#endif
    }
}