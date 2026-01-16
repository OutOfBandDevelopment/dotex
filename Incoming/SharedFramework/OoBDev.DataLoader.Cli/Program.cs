using OoBDev.DataLoader.Providers;
using OoBDev.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;

namespace OoBDev.DataLoader.Cli
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var serviceCollection = new ServiceCollection()
                .AddDataloaderServices()
                .AddRequiredFramework()
                ;

            var serviceProvider = serviceCollection.BuildServiceProvider();

            var _logger = serviceProvider.GetRequiredService<ILogger<Program>>();
            var template = serviceProvider.GetRequiredService<IDatabaseDeploymentTemplate>();


            var _contextProvider = serviceProvider.GetRequiredService<IDbContextProvider>();
            var _dataEntityProvider = serviceProvider.GetRequiredService<IDataEntityProvider>();
            var _sourceDataPipeline = serviceProvider.GetRequiredService<ISourceDataPipeline>();
            var _sourceDataValidationProvider = serviceProvider.GetRequiredService<ISourceDataValidationProvider>();

            var dbContexts =
                from dbContext in _contextProvider.GetDbContexts(template)
                let entityTypes = _dataEntityProvider.GetEntityTypes(dbContext)
                let sourceEntities = _sourceDataPipeline.ReadEntityData(template, dbContext, entityTypes)
                let validEntities = _sourceDataValidationProvider.Validate(template, sourceEntities)
                select new
                {
                    DbContext = dbContext,
                    EntityTypes = validEntities //.ToArray(),
                };

            foreach (var dbContext in dbContexts) //.ToArray()
            {
                var context = dbContext.DbContext;

                _logger.LogInformation($"{nameof(dbContext.DbContext)}: {{{nameof(dbContext.DbContext)}}}", dbContext.DbContext);

                foreach (var entityType in dbContext.EntityTypes)
                {
                    _logger.LogInformation($"{nameof(entityType.EntityType)}: {{{nameof(entityType.EntityType)}}}", entityType.EntityType);

                    try
                    {
                        if (template.DisableForeignKeysBeforeInsert)
                            foreach (var fk in entityType.EntityType.GetForeignKeys())
                            {
                                //Note: disable FKs
                                var command = $@"ALTER TABLE [{ entityType.EntityType.GetSchema()}].[{ entityType.EntityType.GetTableName()}] NOCHECK CONSTRAINT [{fk.GetConstraintName()}];";
                                _logger.LogInformation($"{command}: {{{nameof(entityType.EntityType)}}}", entityType.EntityType);
                                context.Database.ExecuteSqlRaw(command);
                            }

                        var pk = entityType.EntityType.FindPrimaryKey();

                        if (pk == null)
                        {
                            _logger.LogWarning($"No Primary Key for {{{nameof(entityType.EntityType)}}} can not import", entityType.EntityType);
                            continue;
                        }

                        var identityProperty = (
                            from prop in entityType.EntityType.GetProperties()
                            where prop.PropertyInfo != null
                            let attributes = prop.PropertyInfo.GetCustomAttributes<DatabaseGeneratedAttribute>()
                            where attributes.Any(a => a.DatabaseGeneratedOption == DatabaseGeneratedOption.Identity)
                            select prop.PropertyInfo).FirstOrDefault();

                        var defaultIdentityValue = (identityProperty?.PropertyType?.IsValueType ?? false) ? Activator.CreateInstance(identityProperty.PropertyType) : null;

                        var buildOut = from entity in entityType.Entities
                                       let identityValue = identityProperty?.GetValue(entity.Entity)
                                       select new
                                       {
                                           entity.Key,
                                           IsDefaultKey = entity.Key == entityType.DefaultPrimaryKey,
                                           IdentityValue = identityValue,
                                           Model = entity,
                                           Entity = entity.Entity,
                                       };
                        var entities = buildOut.ToArray();

                        var changed = false;
                        foreach (var entity in entities.Where(e => e.IsDefaultKey))
                        {
                            //Note: insert entities where PK is default value
                            _logger.LogInformation($"Add entry for {nameof(entity.IsDefaultKey)} as {{{nameof(entity.Entity)}}}", entity.Entity);
                            context.Add(entity.Entity);
                            changed = true;
                        }
                        if (changed) context.SaveChanges();

                        changed = false;
                        foreach (var entity in entities.Where(e => !e.IsDefaultKey && (
                            (defaultIdentityValue != null && defaultIdentityValue.Equals(e.IdentityValue)) ||
                            (defaultIdentityValue == null && e.IdentityValue == null)
                        )))
                        {
                            //Note: insert entities where PK is not default value and identity is secondary value
                            var keyValues = pk.GetValuesFrom(entity.Entity);
                            var entry = keyValues.Length == 0 ? null : context.Find(entityType.EntityType.ClrType, keyValues);

                            if (entry == null)
                            {
                                _logger.LogInformation($"Add entry for !{nameof(entity.IsDefaultKey)} as {{{nameof(entity.Entity)}}}", entity.Entity);
                                context.Add(entity.Entity);
                                changed = true;
                            }
                        }
                        if (changed) context.SaveChanges();

                        if (identityProperty != null)
                        {
                            changed = false;
                            foreach (var entity in entities.Where(e => !e.IsDefaultKey && !(
                                (defaultIdentityValue != null && defaultIdentityValue.Equals(e.IdentityValue)) ||
                                (defaultIdentityValue == null && e.IdentityValue == null)
                            )))
                            {
                                //insert values where identity is not default and is already set
                                var keyValues = pk.GetValuesFrom(entity.Entity);
                                var entry = keyValues.Length == 0 ? null : context.Find(entityType.EntityType.ClrType, keyValues);

                                if (entry == null)
                                {
                                    _logger.LogInformation($"Add entry for !{nameof(entity.IsDefaultKey)} as {{{nameof(entity.Entity)}}} {{{nameof(entity.Key)}}} {{{nameof(entity.IdentityValue)}}}", entity.Entity, entity.Key, entity.IdentityValue);
                                    context.Add(entity.Entity);
                                    changed = true;
                                }
                            }
                            if (changed)
                                using (var transaction = context.Database.BeginTransaction())
                                {
                                    var command = $"SET IDENTITY_INSERT [{entityType.EntityType.GetSchema()}].[{entityType.EntityType.GetTableName()}] ON";
                                    _logger.LogInformation($"{command}: {{{nameof(entityType.EntityType)}}}", entityType.EntityType);
                                    context.Database.ExecuteSqlRaw(command);
                                    context.SaveChanges();
                                    command = $"SET IDENTITY_INSERT [{entityType.EntityType.GetSchema()}].[{entityType.EntityType.GetTableName()}] OFF";
                                    _logger.LogInformation($"{command}: {{{nameof(entityType.EntityType)}}}", entityType.EntityType);
                                    context.Database.ExecuteSqlRaw(command);
                                    transaction.Commit();
                                }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error: {{{nameof(entityType.EntityType)}}} => {{{nameof(ex.Message)}}}", entityType.EntityType, ex.Message);
                        _logger.LogDebug($"Error: {{{nameof(entityType.EntityType)}}} => {{{nameof(Exception)}}}", entityType.EntityType, ex);
                        throw;
                    }
                    finally
                    {
                        if (template.DisableForeignKeysBeforeInsert)
                            foreach (var fk in entityType.EntityType.GetForeignKeys())
                            {
                                //Note: enable FKs
                                var command = $@"ALTER TABLE [{ entityType.EntityType.GetSchema()}].[{ entityType.EntityType.GetTableName()}] CHECK CONSTRAINT [{fk.GetConstraintName()}];";
                                _logger.LogInformation($"{command}: {{{nameof(entityType.EntityType)}}}", entityType.EntityType);
                                context.Database.ExecuteSqlRaw(command);
                            }
                    }
                }
            }

            //    //    //TODO: add data dedup
            //    //    //todo: add other output types
            //    //    //todo: add identity insert support
            //    //    //TODO: remove queue from document uploading

            //    //    //if (mapped.Any(m => m.hasDuplicates))
            //    //    //{
            //    //    //    throw new DuplicateDataException(mapped.Where(s => s.hasDuplicates).Select(s => s.Entity).ToArray());
            //    //    //}

            //    //    //TODO: add "import" to store back into DBContext
            //    //    //TODO: import should ignore CreatedOn/CreatedModifiedBy
            //    //    //TODO: add "export" which should allow for search/filter/sort/page objects as well as excluding based on existing maps.
            //}
        }
    }
}