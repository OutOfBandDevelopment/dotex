using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Buffers;
using MQTTnet;
using MQTTnet.Protocol;

namespace OoBDev.System.IO.Mqtt;

/// <summary>The production <see cref="IMqttConnection"/>, backed by MQTTnet.</summary>
internal sealed class MqttNetConnection : IMqttConnection
{
    private readonly MqttClientFactory _factory = new();
    private readonly IMqttClient _client;

    public MqttNetConnection()
    {
        _client = _factory.CreateMqttClient();
        _client.ApplicationMessageReceivedAsync += e =>
        {
            MessageReceived?.Invoke(e.ApplicationMessage.Topic, e.ApplicationMessage.Payload.ToArray());
            return Task.CompletedTask;
        };
        _client.DisconnectedAsync += e =>
        {
            Disconnected?.Invoke(e.Exception);
            return Task.CompletedTask;
        };
    }

    public event Action<string, byte[]>? MessageReceived;

    public event Action<Exception?>? Disconnected;

    public async Task ConnectAsync(MqttDeviceAdapterOptions options, CancellationToken cancellationToken)
    {
        var builder = new MqttClientOptionsBuilder()
            .WithTcpServer(options.Host, options.Port)
            .WithTimeout(TimeSpan.FromMilliseconds(options.TimeoutMs));
        if (!string.IsNullOrEmpty(options.ClientId))
        {
            builder.WithClientId(options.ClientId);
        }

        if (!string.IsNullOrEmpty(options.Username))
        {
            builder.WithCredentials(options.Username, options.Password);
        }

        await _client.ConnectAsync(builder.Build(), cancellationToken).ConfigureAwait(false);
    }

    public async Task SubscribeAsync(string topicFilter, int qualityOfService, CancellationToken cancellationToken)
    {
        var subscribe = _factory.CreateSubscribeOptionsBuilder()
            .WithTopicFilter(topicFilter, (MqttQualityOfServiceLevel)qualityOfService)
            .Build();
        await _client.SubscribeAsync(subscribe, cancellationToken).ConfigureAwait(false);
    }

    public async Task PublishAsync(string topic, ReadOnlyMemory<byte> payload, int qualityOfService, CancellationToken cancellationToken)
    {
        var message = new MqttApplicationMessageBuilder()
            .WithTopic(topic)
            .WithPayload(payload.ToArray())
            .WithQualityOfServiceLevel((MqttQualityOfServiceLevel)qualityOfService)
            .Build();
        await _client.PublishAsync(message, cancellationToken).ConfigureAwait(false);
    }

    public async Task DisconnectAsync(CancellationToken cancellationToken)
    {
        if (_client.IsConnected)
        {
            await _client.DisconnectAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
        }
    }

    public ValueTask DisposeAsync()
    {
        _client.Dispose();
        return ValueTask.CompletedTask;
    }
}
