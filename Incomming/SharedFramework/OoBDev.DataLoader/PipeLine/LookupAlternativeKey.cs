using OoBDev.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Data;
using System.Linq;

namespace OoBDev.DataLoader.PipeLine
{
    public class LookupAlternativeKey<TEntity> : ILookupAlternativeKey
        where TEntity : class
    {
#if NET5_0_OR_GREATER
        // IReadOnlyEntityType
        public JToken? Existing(DbContext context, IReadOnlyEntityType entityType, JToken input, IReadOnlyKey key)
#else
        // IEntityType
        public JToken? Existing(DbContext context, IEntityType entityType, JToken input, IKey key)
#endif
        {
            var keyValues = from pi in key.Properties
                            let value = input[pi.Name]
                            where value != null
                            let realValue = value.ToObject(pi.ClrType)

                            let expression = ExpressionTreeBuilder.BuildExpression<TEntity>(pi.PropertyInfo)
                            let predicate = expression.BuildPredicate(realValue)
                            select predicate;

            var matched = keyValues.Aggregate(
                context.Set<TEntity>().AsQueryable(),
                (q, p) => q.Where(p),
                q => q.FirstOrDefault()
                );

            if (matched == null) return null;
            return JObject.FromObject(matched);
        }
    }

    public class LookupAlternativeKey : IDataPipelineHandler
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger _logger;

        public LookupAlternativeKey(
            IServiceProvider serviceProvider,
            ILogger<LookupAlternativeKey> logger
            )
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

#if NET5_0_OR_GREATER
        // IReadOnlyEntityType
        public static ILookupAlternativeKey GetLookup(IServiceProvider serviceProvider, IReadOnlyEntityType entityType) =>
#else
        // IEntityType
        public static ILookupAlternativeKey GetLookup(IServiceProvider serviceProvider, IEntityType entityType) =>
#endif
        (ILookupAlternativeKey)ActivatorUtilities.CreateInstance(serviceProvider, typeof(LookupAlternativeKey<>).MakeGenericType(entityType.ClrType));


#if NET5_0_OR_GREATER
        // IReadOnlyEntityType
        public JToken Handle(DbContext context, IReadOnlyEntityType entityType, JToken input)
#else
        // IEntityType
        public JToken Handle(DbContext context, IEntityType entityType, JToken input)
#endif
        {
            var keys = entityType.GetKeys();
            if (keys.Any(k => !k.IsPrimaryKey()))
            {
                var primaryKey = entityType.FindPrimaryKey();
                if (primaryKey != null)
                {
                    _logger.LogInformation($"Check if primary key is set: {{entityType}}", entityType);

                    var values = from pi in primaryKey.Properties
                                 let value = input[pi.Name]
                                 where value != null
                                 let realValue = value.ToObject(pi.ClrType)
                                 let defaultType = GetDefault(pi.ClrType)
                                 select new
                                 {
                                     RealValue = realValue,
                                     IsDefault = defaultType.Equals(realValue)
                                 };

                    if (values.All(v => v.IsDefault))
                    {
                        _logger.LogWarning($"Lookup primary key by alternative for : {{entityType}}: {{json}}", entityType, input);

                        var alternateKeys = keys.Where(k => !k.IsPrimaryKey()).ToArray();

                        var lookup = GetLookup(_serviceProvider, entityType);

                        foreach (var alternateKey in alternateKeys)
                        {
                            var value = lookup.Existing(context, entityType, input, alternateKey);
                            if (value != null)
                            {
                                foreach (var prop in primaryKey.Properties)
                                {
                                    if (input[prop.Name] == null)
                                    {
                                        input[prop.Name] = value[prop.Name];
                                    };
                                }
                                _logger.LogWarning($"Updated values for : {{entityType}}: {{json}}", entityType, input);

                                break;
                            }
                        }
                    }
                }
            }

            return input;
        }

        internal static object? GetDefault(Type t) =>
            t.IsValueType ? Activator.CreateInstance(t) : null;
    }
}
