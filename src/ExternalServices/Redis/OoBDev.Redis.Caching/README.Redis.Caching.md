# OoBDev.Redis.Caching

Distributed caching provider for `OoBDev.Caching`, backed by Redis through StackExchange.Redis.

## Registration

```csharp
services.TryAddRedisCachingServices();
```

This registers `RedisCachingProvider` as the transient `ICachingProvider` and under the key `redis` (`RedisGlobals.ProviderKey`), plus `IConnectionMultiplexerFactory`. To select it for the attribute-driven caching of `OoBDev.Caching`, set `OoBDev:CachingProvider:Type` to `redis` (see the [Caching readme](../../../Framework/OoBDev.Caching/README.Caching.md)). The provider needs `IObjectConverter` and `IJsonSerializer` from the framework.

## Configuration

**Table 1 — Redis caching settings**

| Key | Default | Note |
|-----|---------|------|
| `Redis:ConnectionMultiplexer:Config` | none (required) | StackExchange.Redis configuration string, for example `server1:6379,password=...,ssl=true`. The connection is opened on first use; a missing value throws `InvalidOperationException` naming the key. |

See the [StackExchange.Redis configuration reference](https://stackexchange.github.io/StackExchange.Redis/Configuration.html) for the string options.

## Behaviour

- `StoreAsync` serializes the object to JSON and sets the string key with the given expiration; a blank key or null data is ignored.
- `RetrieveAsync` reads the string and converts it to the requested type with `IObjectConverter`; a missing key or blank key returns `null`.
- `FlushAsync` deletes one key.
- Entries are shared by every instance that uses the same Redis and survive an application restart.

## Verification

`OoBDev.Redis.Caching.Tests`: Unit tests for the provider with a mocked connection, and Integration tests against the Redis container of the test stack (`REDIS_CONNECTION_STRING`, default `localhost:6379`; see `docs/architecture/testing/integration/`). Start the container with `containers/testing/scripts/integration-up.sh`.
