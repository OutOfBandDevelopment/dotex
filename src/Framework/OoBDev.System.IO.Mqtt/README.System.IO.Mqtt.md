# OoBDev.System.IO.Mqtt

An MQTT broker connection as an `IDeviceAdapter`, over MQTTnet.

## Description

`MqttDeviceAdapter` (type `Mqtt`, path = `Host:Port`) is options-driven by `MqttDeviceAdapterOptions`: `Host`, `Port` (default 1883), `ClientId`, `Username`, `Password`, `SubscribeTopics` (filters, wildcards allowed), `PublishTopic`, `QualityOfService` (0 to 2) and `TimeoutMs`.

Topic addressing is carried as text. Each inbound message becomes one line in `Input`, `topic<TAB>payload\n`. A line written to `Output` is published to `PublishTopic`, or to its own topic when written as `topic<TAB>payload`.

```csharp
services.AddMqttDeviceAdapter();
services.Configure<MqttDeviceAdapterOptions>(o => { o.Host = "localhost"; o.SubscribeTopics = ["sensors/#"]; });
```

`IMqttConnection` and `IMqttConnectionFactory` are the seam over MQTTnet (`MqttNetConnection`) and the test double point.

## Verified

Unit tests cover the adapter against a mocked connection. `MosquittoIntegrationTests` (category `DevLocal`) needs a broker on 127.0.0.1:1883 and ends inconclusive when none is there.
