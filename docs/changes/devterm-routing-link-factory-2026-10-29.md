# DevTerm routing link factory (2026-10-29)

`RoutingLinkFactory` (the default `IRoutingLinkFactory`) was missing from `OoBDev.DevTerm.Core`: routing had the interface, service and bridges but nothing to build a link from a profile. It now builds an MQTT link (`MqttRouterBridge` over `MqttNetConnectionFactory`) or an AMQP or STOMP link (`BrokerRouterBridge`) from `RoutingOptions.Protocol`, with the broker-to-device rule topics as subscriptions. `AddDevTermCore()` registers it with `TryAddSingleton`. The links are internal `MqttRoutingLink` and `BrokerRoutingLink`.

Tests: `RoutingLinkFactoryTests` (6 Unit, passed). Connecting to a real broker is covered by the existing bridge tests and the Docker broker Integration stack, not run here.
