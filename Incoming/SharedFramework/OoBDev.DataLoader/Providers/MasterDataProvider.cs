using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace OoBDev.DataLoader.Providers
{
    public class MasterDataProvider : IMasterDataProvider
    {
        public const string MasterData = nameof(MasterData);
        public const BindingFlags PublicStatic = BindingFlags.Static | BindingFlags.Public;

        private readonly IDataEntityProvider _entities;

        public MasterDataProvider(
            IDataEntityProvider entities
            )
        {
            _entities = entities;
        }

#if NET5_0_OR_GREATER
        public IEnumerable<(IReadOnlyEntityType entity, IEnumerable<object> masterData)> GetMasterData(DbContext dbContext) =>
#else
        public IEnumerable<(IEntityType entity, IEnumerable<object> masterData)> GetMasterData(DbContext dbContext) =>
#endif
            from et in _entities.GetEntityTypes(dbContext)
            let md = GetMasterData(et)
            where md != null
            select (entity: et, masterData: md);

#if NET5_0_OR_GREATER
        public IEnumerable<object> GetMasterData(IReadOnlyEntityType entityType) =>
#else
        public IEnumerable<object> GetMasterData(IEntityType entityType) =>
#endif
        (entityType.ClrType.GetMember(MasterData, PublicStatic)?.FirstOrDefault() switch
            {
                FieldInfo mi => mi.GetValue(null),
                PropertyInfo mi => mi.GetValue(null),
                MethodInfo mi when !mi.GetParameters().Any() => mi.Invoke(null, null),
                _ => null
            } as IEnumerable)?.OfType<object>() ?? Enumerable.Empty<object>();
    }
}
