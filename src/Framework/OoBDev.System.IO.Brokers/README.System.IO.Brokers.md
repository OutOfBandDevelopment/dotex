# OoBDev.System.IO.Brokers

AMQP 0-9-1 (RabbitMQ) and STOMP 1.2 message-broker connections as an `IDeviceAdapter`.

## Description

`BrokerDeviceAdapter` (type `Broker`, path = `Host:Port`) is options-driven by `BrokerDeviceAdapterOptions`: `Host`, `Port` (default 5672), `Username`, `Password`, `VirtualHost`, `Exchange`, `SubscribeTopics` (routing keys or destinations), `PublishTopic`, `UseTls`, `TlsCaCertificatePath` and `TimeoutMs`. The protocol is chosen by the registration: `AddAmqpDeviceAdapter()` (RabbitMQ.Client) or `AddStompDeviceAdapter()` (`StompConnection`, a hand-written STOMP 1.2 client with `StompFrame` encoding).

Topic addressing is carried as text. Each inbound message becomes one line in `Input`, `topic<TAB>payload\n`. A line written to `Output` is published to `PublishTopic`, or to its own topic when written as `topic<TAB>payload`.

```csharp
services.AddAmqpDeviceAdapter();
services.Configure<BrokerDeviceAdapterOptions>(o => { o.Host = "localhost"; o.Exchange = "amq.topic"; o.SubscribeTopics = ["sensors.#"]; });
```

`IBrokerConnection` and `IBrokerConnectionFactory` are the seam over the client libraries and the test double point.

## Verified

9 Unit tests cover the adapter against a mocked connection and the STOMP frame codec. `RabbitMqIntegrationTests` (category `Integration`, 6 tests) round-trips AMQP, STOMP and the TLS listeners through the RabbitMQ of `containers/testing` (`RABBITMQ_*` test parameters); it was not run for this review.
