using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.ComplexEvents;

/// <summary>Dispatches to the supplied handler instances, ordered by <see cref="ComplexEventHandlerAttribute.Order"/>.</summary>
/// <param name="handlers">Handler instances.</param>
public sealed class ComplexEventDispatcher(IEnumerable<object> handlers) : IComplexEventDispatcher
{
    private readonly object[] _handlers = handlers.ToArray();

    /// <inheritdoc />
    public async Task DispatchAsync<TEvent>(TEvent @event, CancellationToken cancellationToken)
    {
        var matches = _handlers
            .OfType<IComplexEventHandler<TEvent>>()
            .Select((h, i) => (Handler: h, Order: OrderOf(h, typeof(TEvent)), Index: i))
            .OrderBy(x => x.Order).ThenBy(x => x.Index);

        var errors = new List<Exception>();
        foreach (var (handler, _, _) in matches)
        {
            try
            {
                await handler.HandleAsync(@event, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                errors.Add(ex);
            }
        }

        if (errors.Count > 0) throw new AggregateException("One or more event handlers failed", errors);
    }

    private static int OrderOf(object handler, Type eventType)
        => handler.GetType().GetCustomAttributes<ComplexEventHandlerAttribute>().FirstOrDefault(a => a.EventType.IsAssignableFrom(eventType))?.Order ?? 0;
}
