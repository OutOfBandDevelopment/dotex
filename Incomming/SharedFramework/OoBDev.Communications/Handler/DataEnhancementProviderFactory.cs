using OoBDev.Communications.Contracts;
using OoBDev.Communications.Contracts.Handler;
using OoBDev.Extensions;
using OoBDev.Toolkit.Common;
using OoBDev.Toolkit.Contracts.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace OoBDev.Communications.Handler
{
    public class DataEnhancementProviderFactory : IDataEnhancementProviderFactory
    {
        private readonly IReadOnlyList<ServiceDescriptor> _providers;
        private readonly IServiceProvider _serviceProvider;
        private readonly IObjectSerializer _serializer;

        public DataEnhancementProviderFactory(
            IEnumerable<ServiceDescriptor> providers,
            IServiceProvider serviceProvider,
            IObjectSerializer serializer
            )
        {
            _providers = providers.Where(t => t.ServiceType == typeof(IDataEnhancementProvider)).ToArray();
            _serviceProvider = serviceProvider;
            _serializer = serializer;
        }

        public JObject GetData(object? data) => //TODO: migrate this to Data converter
            data switch
            {
                null => null,
                JObject json => json,
                _ => _serializer.GetAsSerialized(data) switch
                {
                    null => null,
                    string input when string.IsNullOrEmpty(input) => null,
                    string input => JToken.Parse(input) switch
                    {
                        null => null,
                        JObject json => json,
                        JToken value => new JObject(
                            new JProperty("Value", value),
                            new JProperty("Type", value.Type.ToString())
                            )
                    }
                },
            } ?? new JObject();

        public int TotalProviderCount => _providers.Count;

        public IEnumerable<IDataEnhancementProvider> GetProviders(string messageType) => ResolveProviders(messageType).ToArray();

        private IEnumerable<IDataEnhancementProvider> ResolveProviders(string messageType) =>
           from provider in _providers
           let factoryResult = provider.ImplementationFactory?.Invoke(_serviceProvider)
           let type = provider.ImplementationType ??
                      provider.ImplementationInstance?.GetType() ??
                      factoryResult?.GetType()
           where type != null
           let attributes = type.GetAttributes<DataEnhancerAttribute>()
           where !attributes.Any() ||
                 attributes.Any(a =>
                    string.IsNullOrWhiteSpace(a.TargetedMessageType) ||
                    string.Equals(messageType?.Trim(), a.TargetedMessageType?.Trim(), StringComparison.InvariantCultureIgnoreCase)
                 )
           let priority = attributes.Max(a => (int?)a.Priority) ?? 0
           orderby priority
           let instance = factoryResult ?? provider.ImplementationInstance ?? ActivatorUtilities.CreateInstance(_serviceProvider, type)
           select instance as IDataEnhancementProvider;
    }
}