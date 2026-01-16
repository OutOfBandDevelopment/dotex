using OoBDev.ComplexEvents.Contracts;
using OoBDev.ComplexEvents.Contracts.Services;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;

namespace OoBDev.ComplexEvents.Common.Resolvers
{
    public class EventHubResolver : IEventResolver
    {
        public const string DefaultHubNameConfigKey = "Azure:EventHub:Default:DefaultHubName";

        private readonly IConfiguration _config;

        public EventHubResolver(
            IConfiguration config
            )
        {
            _config = config;
        }

        public string GetEventHubName<T>()
        {
            var type = typeof(T);
            if (type.IsGenericType)
            {
                type = type.GetGenericTypeDefinition();
            }

            var name = (type.GetCustomAttributes(typeof(EventHubChannelAttribute), false) ?? Array.Empty<object>())
                        .OfType<EventHubChannelAttribute>()
                        .FirstOrDefault(a => !string.IsNullOrWhiteSpace(a.EventHubName))?.EventHubName;

            if (!string.IsNullOrWhiteSpace(name))
            {
                return name;
            }

            var defaultHubName = _config[DefaultHubNameConfigKey];
            if (!string.IsNullOrWhiteSpace(defaultHubName))
            {
                return defaultHubName;
            }

            name = type.FullName;

            if (name.Length > 50)
            {
                return name.Substring(name.Length - 50, 50);
            }
            else
            {
                return name;
            }
        }

        public string GetMessageType(IEventData @event) =>
             $"clr/object+{@event.GetType().FullName}";

        public string GetPartitionKey<T>()
        {
            var type = typeof(T);
            if (type.IsGenericType)
            {
                type = type.GetGenericTypeDefinition();
            }

            var name = (type.GetCustomAttributes(typeof(EventHubChannelAttribute), false) ?? Array.Empty<object>())
                        .OfType<EventHubChannelAttribute>()
                        .FirstOrDefault(a => !string.IsNullOrWhiteSpace(a.PartitionKey))?.PartitionKey;
            if (!string.IsNullOrWhiteSpace(name))
            {
                return name;
            }

            name = type.FullName;
            return name;
        }
    }
}
