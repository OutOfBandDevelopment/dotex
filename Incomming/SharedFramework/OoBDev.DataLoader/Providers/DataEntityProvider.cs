using OoBDev.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace OoBDev.DataLoader.Providers
{

    public class DataEntityProvider : IDataEntityProvider
    {
        private readonly ILogger _logger;

        public DataEntityProvider(
            ILogger<IDataEntityProvider> logger
            )
        {
            _logger = logger;
        }

#if NET5_0_OR_GREATER
        public IEnumerable<IReadOnlyEntityType> GetEntityTypes(DbContext context)
#else
        public IEnumerable<IEntityType> GetEntityTypes(DbContext context)
#endif
        {
            _logger.LogDebug($"{nameof(GetEntityTypes)} for: {{dbContext}}", context);
#if NET5_0_OR_GREATER
            var collected = new List<IReadOnlyEntityType>();
#else
            var collected = new List<IEntityType>();
#endif
            foreach (var entity in context.Model.GetEntityTypes())
            {
                var dependents = GetDependents(entity, 0);

                foreach (var dependent in dependents)
                {
                    if (collected.Contains(dependent))
                    {
                        _logger.LogDebug($"-- Already Processed Dependent {{entity}}", dependent);
                        continue;
                    }
                    else
                    {
                        _logger.LogDebug($"++ Collect Dependent {{entity}}", entity);
                        yield return dependent;
                        collected.Add(dependent);
                    }
                }

                if (collected.Contains(entity))
                {
                    _logger.LogDebug($"-- Already Processed Entity {{entity}}", entity);
                    continue;
                }
                else
                {
                    _logger.LogDebug($"++ Collect Entity {{entity}}", entity);
                    yield return entity;
                    collected.Add(entity);
                }
            }
        }

#if NET5_0_OR_GREATER        
        internal IEnumerable<IReadOnlyEntityType> GetDependents(IReadOnlyEntityType entity, int depth)
#else
        internal IEnumerable<IEntityType> GetDependents(IEntityType entity, int depth)
#endif
        {
            var dependents = from attribute in entity.ClrType.GetCustomAttributes<DependsOnAttribute>()
                             from type in attribute.Types
#if NET5_0_OR_GREATER   
                             from dependent in entity.Model.GetEntityTypes()
                             where dependent.ClrType == type
#else
                             from dependent in entity.Model.GetEntityTypes(type)
#endif
                             select dependent;
            foreach (var dependent in dependents)
            {
                if (dependent != entity)
                {
                    _logger.LogDebug($"{nameof(GetDependents)}::{nameof(DependsOnAttribute)}[{new string('-', depth)}]: {{entity}}", dependent);
                    foreach (var child in GetDependents(dependent, depth + 1))
                        yield return child;
                    yield return dependent;
                }
            }

            foreach (var fk in entity.GetForeignKeys())
            {
                if (fk.PrincipalEntityType != entity)
                {
                    _logger.LogDebug($"{nameof(GetDependents)}::{nameof(entity.GetForeignKeys)}[{new string('-', depth)}]: {{entity}}", fk.PrincipalEntityType);
                    foreach (var dependent in GetDependents(fk.PrincipalEntityType, depth + 1))
                        yield return dependent;
                    yield return fk.PrincipalEntityType;
                }
            }
        }
    }
}
