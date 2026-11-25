using System;
using System.Linq;
using Microsoft.Extensions.Options;
using OoBDev.DevTerm.Brokers;
using OoBDev.System.IO.Brokers;
using OoBDev.System.IO.Mqtt;

namespace OoBDev.DevTerm.Configuration;

/// <summary>Builds the MQTT, AMQP or STOMP link a profile's <see cref="RoutingOptions.Protocol"/> names.</summary>
public sealed class RoutingLinkFactory : IRoutingLinkFactory
{
    /// <inheritdoc />
    public IRoutingLink Create(RoutingOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        var rules = new RoutingRuleSet { Rules = options.Rules };
        var topics = options.Rules.Where(r => r.Direction == RoutingDirection.BrokerToDevice).Select(r => r.Topic).ToList();
        if (options.Protocol.Equals("amqp", StringComparison.OrdinalIgnoreCase) || options.Protocol.Equals("stomp", StringComparison.OrdinalIgnoreCase))
        {
            var broker = new BrokerDeviceAdapterOptions
            {
                Host = options.Host,
                Port = options.EffectivePort,
                Username = options.Username,
                Password = options.Password,
                UseTls = options.Tls,
                SubscribeTopics = topics,
            };
            IBrokerConnectionFactory factory = options.Protocol.Equals("amqp", StringComparison.OrdinalIgnoreCase)
                ? new AmqpConnectionFactory()
                : new StompConnectionFactory();
            return new BrokerRoutingLink(new BrokerRouterBridge(factory, Options.Create(broker), rules));
        }

        var mqtt = new MqttDeviceAdapterOptions
        {
            Host = options.Host,
            Port = options.EffectivePort,
            Username = options.Username,
            Password = options.Password,
            SubscribeTopics = topics,
        };
        return new MqttRoutingLink(new MqttRouterBridge(new MqttNetConnectionFactory(), Options.Create(mqtt), rules));
    }
}
