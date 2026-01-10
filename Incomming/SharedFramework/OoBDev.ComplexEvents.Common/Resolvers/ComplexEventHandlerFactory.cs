using OoBDev.ComplexEvents.Contracts;
using OoBDev.ComplexEvents.Contracts.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace OoBDev.ComplexEvents.Common.Resolvers
{
    public class ComplexEventHandlerFactory : IComplexEventHandlerFactory
    {
        private readonly IEnumerable<ServiceDescriptor> _handlers;
        private readonly IComplexEventHandlerResolver _resolver;
        private readonly ILogger<ComplexEventHandlerFactory> _log;
        private readonly IServiceProvider _serviceProvider;

        public ComplexEventHandlerFactory(
            IEnumerable<ServiceDescriptor> handlers,
            IComplexEventHandlerResolver resolver,
            ILogger<ComplexEventHandlerFactory> log,
            IServiceProvider serviceProvider
            )
        {
            _handlers = handlers.Where(t => t.ServiceType == typeof(IComplexEventHandler)).ToArray(); //todo: clean this up into a reuseable thing
            _resolver = resolver;
            _log = log;
            _serviceProvider = serviceProvider;
        }

        private IComplexEventHandler[] ResolveHandlers(string target)
        {
            if (string.IsNullOrWhiteSpace(target))
            {
                _log.LogWarning($"No target provided. Nothing to do.");
                return Array.Empty<IComplexEventHandler>();
            }

            if (!_handlers.Any())
            {
                _log.LogWarning($"No handlers registered. Nothing to do.");
                return Array.Empty<IComplexEventHandler>();
            }

            var types = from definition in _handlers
                        let instance = definition.ImplementationInstance ?? definition.ImplementationFactory?.Invoke(_serviceProvider)
                        let type = definition.ImplementationType ?? instance?.GetType()
                        where type != null
                        select new
                        {
                            type,
                            instance
                        };

            var query = from t in types
                        where _resolver.CheckTarget(target, t.type)
                        let instance = (t.instance ?? ActivatorUtilities.CreateInstance(_serviceProvider, t.type)) as IComplexEventHandler
                        where instance != null
                        select instance;

            var selected = query.ToArray();

            if (!selected.Any())
            {
                _log.LogWarning($"No handlers for \"{target}\". Nothing to do.");
                return Array.Empty<IComplexEventHandler>();
            }

            _log.LogInformation($"Matched {selected.Count()} of {_handlers.Count()} handlers for \"{target}\"");

            return selected;
        }

        public IEnumerable<IComplexEventHandler> GetHandlers(string target) => ResolveHandlers(target);
    }
}
