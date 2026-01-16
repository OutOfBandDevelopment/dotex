using OoBDev.Toolkit.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace OoBDev.DataLoader.PipeLine
{
    public class DataPipelineProcessor : IDataPipelineProcessor
    {
        private readonly IDataPipelineHandler[] _handlers;
        private readonly IObjectConverter _converter;

        public DataPipelineProcessor(
            IEnumerable<IDataPipelineHandler> handlers,
            IObjectConverter converter
            )
        {
            _converter = converter;
            _handlers = handlers.OrderBy(h => h.GetType().GetCustomAttribute<DataPipelinePriorityAttribute>()?.Priority ?? 0).ToArray();
        }

#if NET5_0_OR_GREATER
        // IReadOnlyEntityType
        public object? ConvertToEntity(DbContext context, IReadOnlyEntityType entityType, object? item) =>
#else
        // IEntityType
        public object? ConvertToEntity(DbContext context, IEntityType entityType, object? item) =>
#endif
        _handlers.Aggregate(
                 _converter.ToJson(item),
                 (input, handler) => input == null ? null : handler.Handle(context, entityType, input),
                 result => _converter.Convert(result, entityType.ClrType)
                 );

#if NET5_0_OR_GREATER
        // IReadOnlyEntityType
        public IEnumerable<object> ConvertToEntities(DbContext context, IReadOnlyEntityType entityType, IEnumerable<object> items)
#else
        // IEntityType
        public IEnumerable<object> ConvertToEntities(DbContext context, IEntityType entityType, IEnumerable<object> items)
#endif
        {
            foreach (var item in items)
            {
                var result = ConvertToEntity(context, entityType, item);

                if (result != null)
                    yield return result;
            }
        }
    }
}
