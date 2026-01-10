using OoBDev.DataLoader.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OoBDev.DataLoader.Providers
{
    public class SourceDataValidationProvider : ISourceDataValidationProvider
    {
        private readonly ILogger _logger;

        public SourceDataValidationProvider(
            ILogger<SourceDataValidationProvider> logger
            )
        {
            _logger = logger;
        }

        public IEnumerable<ValidEntityTypeModel> Validate(IDatabaseDeploymentTemplate template, IEnumerable<SourceEntityTypeReferenceModel> sourceEntities)
        {
            if (template.DuplicateHandling == DuplicateHandling.Fail && sourceEntities.Any(e => e.HasDuplicates))
            {
                var duplicates = from contextEntity in sourceEntities
                                 where contextEntity.HasDuplicates
                                 from entity in contextEntity.Entities
                                 where entity.Count() > 1
                                 select new
                                 {
                                     contextEntity.EntityType,
                                     entity.Key,
                                 };
                foreach (var duplicate in duplicates)
                {
                    _logger.LogError($"Duplicates of \"{{{nameof(duplicate.EntityType)}}}\" detected for \"{{key}}\"", duplicate.EntityType, duplicate.Key);
                }
                throw new DuplicateDataException(duplicates.Select(e => e.EntityType).Distinct().ToArray());
            }

            var query = from sourceEntity in sourceEntities

                        let defaultEntities = from entitySet in sourceEntity.Entities
                                              where entitySet.Key == sourceEntity.DefaultPrimaryKey
                                              from entity in entitySet
                                              where entity.Entity != null
                                              select new ValidEntityModel
                                              {
                                                  Key = entitySet.Key,
                                                  Entity = entity.Entity ?? throw new NotSupportedException(),
                                              }

                        let otherEntities = from entitySet in sourceEntity.Entities
                                            where entitySet.Key != sourceEntity.DefaultPrimaryKey
                                            let entity = template.DuplicateHandling switch
                                            {
                                                DuplicateHandling.Last => entitySet.LastOrDefault()?.Entity,
                                                DuplicateHandling.First => entitySet.FirstOrDefault()?.Entity,
                                                _ => entitySet.SingleOrDefault()?.Entity,
                                            }
                                            where entity != null
                                            select new ValidEntityModel
                                            {
                                                Key = entitySet.Key,
                                                Entity = entity,
                                            }

                        let entities = defaultEntities.Concat(otherEntities)

                        select new ValidEntityTypeModel
                        {
                            EntityType = sourceEntity.EntityType,
                            DbContext = sourceEntity.DbContext,
                            DefaultPrimaryKey = sourceEntity.DefaultPrimaryKey,
                            Entities = entities.ToArray(),
                        };

            foreach (var i in query)
                yield return i;

            //return query;
        }
    }

}