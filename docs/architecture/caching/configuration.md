# Caching Configuration Guide

**Version:** 1.0.0
**Last Updated:** 2026-01-20

---

## Table of Contents

1. [Configuration Overview](#configuration-overview)
2. [Redis Configuration](#redis-configuration)
3. [Microsoft Memory Cache Configuration](#microsoft-memory-cache-configuration)
4. [Caching Manager Configuration](#caching-manager-configuration)
5. [Environment-Specific Configuration](#environment-specific-configuration)
6. [Advanced Scenarios](#advanced-scenarios)

---

## Configuration Overview

The OoBDev Caching Framework uses **Microsoft.Extensions.Configuration** for all configuration. Configuration can be provided via:

- **appsettings.json** - Default configuration file
- **appsettings.{Environment}.json** - Environment-specific overrides
- **Environment variables** - Runtime configuration
- **Command-line arguments** - Deployment-time configuration
- **Azure App Configuration** - Centralized cloud configuration

### Basic Setup

```csharp
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OoBDev.Caching;
using OoBDev.Redis.Caching;

// Build configuration
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false)
    .AddJsonFile($"appsettings.{environment}.json", optional: true)
    .AddEnvironmentVariables()
    .Build();

// Register services
var services = new ServiceCollection();
services.AddSingleton<IConfiguration>(configuration);
services.AddOoBDevCachingServices();
services.AddRedisCachingServices();

var serviceProvider = services.BuildServiceProvider();
```

---

## Redis Configuration

### appsettings.json

```json
{
  "Redis": {
    "ConnectionString": "localhost:6379,ssl=false,abortConnect=false"
  },
  "Caching": {
    "Disabled": false
  }
}
```

### Configuration Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Redis:ConnectionString` | string | (required) | StackExchange.Redis connection string |
| `Caching:Disabled` | bool | false | Globally disable caching (useful for debugging) |

### Connection String Options

StackExchange.Redis supports extensive connection string configuration:

```
localhost:6379,
ssl=false,
abortConnect=false,
connectTimeout=5000,
syncTimeout=1000,
connectRetry=3,
keepAlive=60,
defaultDatabase=0,
password=yourpassword,
name=myappname
```

**Common Options:**

| Option | Description | Example |
|--------|-------------|---------|
| **Host:Port** | Redis server endpoint | `localhost:6379` |
| **ssl** | Use SSL/TLS | `ssl=true` |
| **abortConnect** | Fail fast on connection error | `abortConnect=false` (recommended) |
| **connectTimeout** | Connection timeout (ms) | `connectTimeout=5000` |
| **syncTimeout** | Sync operation timeout (ms) | `syncTimeout=1000` |
| **connectRetry** | Retry count on connection failure | `connectRetry=3` |
| **keepAlive** | Keep-alive interval (seconds) | `keepAlive=60` |
| **defaultDatabase** | Default database number | `defaultDatabase=0` |
| **password** | Authentication password | `password=secret` |
| **name** | Client name for diagnostics | `name=MyApp` |

**Full Documentation:** https://stackexchange.github.io/StackExchange.Redis/Configuration

### Environment-Specific Configuration

**appsettings.Development.json**
```json
{
  "Redis": {
    "ConnectionString": "localhost:6379,ssl=false"
  },
  "Caching": {
    "Disabled": false  // Enable for development
  }
}
```

**appsettings.Staging.json**
```json
{
  "Redis": {
    "ConnectionString": "staging-redis.example.com:6380,ssl=true,password=${REDIS_PASSWORD}"
  }
}
```

**appsettings.Production.json**
```json
{
  "Redis": {
    "ConnectionString": "prod-redis.example.com:6380,ssl=true,password=${REDIS_PASSWORD},connectRetry=5,syncTimeout=2000"
  }
}
```

### Azure Redis Cache

```json
{
  "Redis": {
    "ConnectionString": "myredis.redis.cache.windows.net:6380,ssl=true,password=PRIMARY_ACCESS_KEY,abortConnect=false"
  }
}
```

**Get Connection String from Azure Portal:**
1. Navigate to Azure Redis Cache instance
2. Settings → Access keys
3. Copy "Primary connection string (StackExchange.Redis)"

### AWS ElastiCache (Redis)

```json
{
  "Redis": {
    "ConnectionString": "myredis.abc123.0001.use1.cache.amazonaws.com:6379,ssl=false,abortConnect=false"
  }
}
```

**Note:** AWS ElastiCache Redis does not support SSL by default. Use VPC security groups for network isolation.

### Redis Sentinel (High Availability)

```json
{
  "Redis": {
    "ConnectionString": "sentinel1:26379,sentinel2:26379,sentinel3:26379,serviceName=mymaster,ssl=false"
  }
}
```

### Redis Cluster

```json
{
  "Redis": {
    "ConnectionString": "node1:6379,node2:6379,node3:6379,ssl=false,abortConnect=false"
  }
}
```

StackExchange.Redis automatically detects cluster mode and routes commands appropriately.

---

## Microsoft Memory Cache Configuration

### appsettings.json

```json
{
  "MemoryCache": {
    "SizeLimit": 1024,
    "CompactionPercentage": 0.25,
    "ExpirationScanFrequency": "00:05:00"
  },
  "Caching": {
    "Disabled": false
  }
}
```

### Configuration Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `MemoryCache:SizeLimit` | long? | null | Maximum number of entries (null = unlimited) |
| `MemoryCache:CompactionPercentage` | double | 0.25 | Percentage to compact when SizeLimit reached (0.25 = remove 25%) |
| `MemoryCache:ExpirationScanFrequency` | TimeSpan | 00:01:00 | How often to scan for expired items |

### Programmatic Configuration

```csharp
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

services.Configure<MemoryCacheOptions>(options =>
{
    options.SizeLimit = 1024;  // Max 1024 entries
    options.CompactionPercentage = 0.25;  // Remove 25% when full
    options.ExpirationScanFrequency = TimeSpan.FromMinutes(5);
});

services.AddMicrosoftCachingServices();
```

### Memory Limits and Eviction

**Size-based Limits:**
```csharp
services.Configure<MemoryCacheOptions>(options =>
{
    options.SizeLimit = 100;  // Max 100 items
});

// Set size per cache entry
await _cache.StoreAsync("key", data, TimeSpan.FromHours(1));
// Each entry counts as size = 1 by default

// Custom size per entry (advanced):
var cacheEntryOptions = new MemoryCacheEntryOptions
{
    Size = 10,  // This entry counts as 10 units
    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
};
```

**Memory Pressure-based Eviction:**
```csharp
services.Configure<MemoryCacheOptions>(options =>
{
    // No size limit, rely on memory pressure
    options.SizeLimit = null;

    // Compact aggressively under memory pressure
    options.CompactionPercentage = 0.50;  // Remove 50%
});
```

Microsoft.Extensions.Caching.Memory automatically monitors memory pressure via GC and evicts entries when memory is low.

### Environment-Specific Configuration

**appsettings.Development.json**
```json
{
  "MemoryCache": {
    "SizeLimit": null,  // Unlimited in development
    "CompactionPercentage": 0.10
  }
}
```

**appsettings.Production.json**
```json
{
  "MemoryCache": {
    "SizeLimit": 10000,  // Limit to 10K entries
    "CompactionPercentage": 0.25,
    "ExpirationScanFrequency": "00:10:00"  // Scan every 10 minutes
  }
}
```

---

## Caching Manager Configuration

### Disable Caching Globally

```json
{
  "OoBDev": {
    "Caching": {
      "Disabled": true
    }
  }
}
```

When disabled, `CacheableFactory` returns direct instances instead of proxies, effectively bypassing all caching logic.

**Use Cases:**
- Debugging cache-related issues
- Load testing without cache
- Temporary disable in production

### Disable Caching Programmatically

```csharp
services.Configure<CacheableFactoryOptions>(options =>
{
    options.DisableCaching = true;
});
```

Or via environment variable:
```bash
export OoBDev__Caching__Disabled=true
```

### Custom Key Formatter

```csharp
services.AddSingleton<IStringFormatter, CustomStringFormatter>();

public class CustomStringFormatter : IStringFormatter
{
    public string Format(string template, MethodInfo method, object[] args)
    {
        // Custom key generation logic
        // Example: Support nested property access
        // "user:{user.Profile.UserId}" instead of "user:{userId}"
        return template;  // Your implementation
    }
}
```

---

## Environment-Specific Configuration

### Development Environment

**Goal:** Fast iteration, zero external dependencies

```json
{
  "Caching": {
    "Disabled": false  // Enable caching with in-memory provider
  }
}
```

```csharp
if (environment.IsDevelopment())
{
    services.AddMicrosoftCachingServices();  // Fast, no Redis required
}
```

### Staging Environment

**Goal:** Production-like, but isolated

```json
{
  "Redis": {
    "ConnectionString": "staging-redis.internal:6379,ssl=true"
  },
  "Caching": {
    "Disabled": false
  }
}
```

### Production Environment

**Goal:** High availability, performance, monitoring

```json
{
  "Redis": {
    "ConnectionString": "${REDIS_CONNECTION_STRING}",  // From Key Vault
    "ConnectRetry": 5,
    "SyncTimeout": 2000,
    "KeepAlive": 60
  },
  "Caching": {
    "Disabled": false
  }
}
```

**Use Azure Key Vault for Secrets:**
```csharp
builder.Configuration.AddAzureKeyVault(
    new Uri("https://myvault.vault.azure.net/"),
    new DefaultAzureCredential()
);
```

---

## Advanced Scenarios

### 1. Multi-Provider Configuration

Use different providers for different scenarios:

```json
{
  "Caching": {
    "Providers": {
      "Default": "memory",
      "Distributed": "redis"
    }
  },
  "Redis": {
    "ConnectionString": "localhost:6379"
  }
}
```

```csharp
services.AddMicrosoftCachingServices();  // Register as "memory"
services.AddRedisCachingServices();      // Register as "redis"

// Custom factory to select provider
services.AddSingleton<IProviderSelector>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var defaultProvider = config["Caching:Providers:Default"];

    return new ProviderSelector(defaultProvider);
});
```

### 2. Hybrid L1/L2 Configuration

```json
{
  "Caching": {
    "L1": {
      "Enabled": true,
      "SizeLimit": 1000,
      "ExpirationMinutes": 5
    },
    "L2": {
      "Enabled": true,
      "Provider": "redis"
    }
  },
  "Redis": {
    "ConnectionString": "localhost:6379"
  }
}
```

```csharp
services.Configure<HybridCachingOptions>(options =>
{
    options.L1Enabled = configuration.GetValue<bool>("Caching:L1:Enabled");
    options.L1SizeLimit = configuration.GetValue<int>("Caching:L1:SizeLimit");
    options.L2Enabled = configuration.GetValue<bool>("Caching:L2:Enabled");
});
```

### 3. Feature Flags for Caching

```json
{
  "FeatureManagement": {
    "CachingEnabled": true,
    "RedisCachingEnabled": true
  }
}
```

```csharp
using Microsoft.FeatureManagement;

services.AddFeatureManagement();

var featureManager = serviceProvider.GetRequiredService<IFeatureManager>();

if (await featureManager.IsEnabledAsync("CachingEnabled"))
{
    if (await featureManager.IsEnabledAsync("RedisCachingEnabled"))
        services.AddRedisCachingServices();
    else
        services.AddMicrosoftCachingServices();
}
```

### 4. Dynamic Configuration Reload

```csharp
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

services.Configure<RedisOptions>(
    builder.Configuration.GetSection("Redis"));

// Options will update automatically when appsettings.json changes
services.AddSingleton<IOptionsMonitor<RedisOptions>>();
```

### 5. Connection String from Environment Variables

```bash
export Redis__ConnectionString="localhost:6379"
export OoBDev__Caching__Disabled=false
```

Or in `docker-compose.yml`:
```yaml
services:
  myapp:
    environment:
      - Redis__ConnectionString=redis:6379,ssl=false
      - OoBDev__Caching__Disabled=false
```

### 6. Azure App Configuration

```csharp
builder.Configuration.AddAzureAppConfiguration(options =>
{
    options.Connect(Environment.GetEnvironmentVariable("APP_CONFIG_CONNECTION_STRING"))
           .Select(KeyFilter.Any, LabelFilter.Null)
           .Select(KeyFilter.Any, environment)
           .ConfigureRefresh(refresh =>
           {
               refresh.Register("Caching:Disabled", refreshAll: true)
                      .SetCacheExpiration(TimeSpan.FromSeconds(30));
           });
});
```

Centralized configuration with automatic refresh every 30 seconds.

---

## Configuration Best Practices

### 1. Secrets Management

✅ **DO:**
- Store connection strings in Azure Key Vault / AWS Secrets Manager
- Use environment variables for sensitive data
- Rotate passwords regularly

❌ **DON'T:**
- Commit connection strings to source control
- Share production connection strings via email/chat
- Use same credentials across environments

### 2. Environment Separation

✅ **DO:**
- Use separate Redis instances per environment
- Configure via environment-specific appsettings.{env}.json
- Use different connection strings for dev/staging/prod

❌ **DON'T:**
- Share Redis instance between environments
- Use production credentials in development

### 3. Performance Tuning

✅ **DO:**
- Set `abortConnect=false` for Redis (allow retries)
- Configure appropriate timeouts (connectTimeout, syncTimeout)
- Use connection pooling (StackExchange.Redis does this automatically)
- Monitor cache hit rates and adjust expiration

❌ **DON'T:**
- Use default timeouts in production (too short)
- Ignore connection failures (set up alerts)

### 4. Monitoring and Logging

✅ **DO:**
- Log cache configuration on startup
- Monitor Redis connection status
- Track cache hit/miss rates
- Set up alerts for connection failures

```csharp
var logger = serviceProvider.GetRequiredService<ILogger<Startup>>();
logger.LogInformation("Caching Provider: {Provider}", providerName);
logger.LogInformation("Redis ConnectionString: {ConnectionString}",
    configuration["Redis:ConnectionString"]?.Split(',')[0]);  // Log host only
```

---

## Troubleshooting

### Problem: Connection to Redis Fails

**Symptoms:** `RedisConnectionException`, `It was not possible to connect to the redis server(s)`

**Solutions:**
1. Check Redis is running: `redis-cli ping` (should return `PONG`)
2. Verify connection string: `telnet redis-host 6379`
3. Check firewall rules / security groups
4. Set `abortConnect=false` in connection string
5. Increase `connectTimeout=10000` (10 seconds)

### Problem: Caching Not Working

**Symptoms:** Data not cached, always fetching from database

**Solutions:**
1. Check `Caching:Disabled` is `false`
2. Verify `[IsCacheable]` attribute is applied
3. Check method is returning `Task<T>` (not `void` or non-async)
4. Ensure interface is registered with `.Cacheable<TInterface, TImplementation>()`
5. Add logging to verify proxy creation

### Problem: Memory Cache Growing Too Large

**Symptoms:** High memory usage, OOM exceptions

**Solutions:**
1. Set `SizeLimit` in `MemoryCacheOptions`
2. Reduce expiration times
3. Use Redis for large datasets
4. Implement LRU eviction policy
5. Monitor memory usage and set alerts

### Problem: Slow Cache Performance

**Symptoms:** High latency on cached operations

**Solutions:**
1. Check Redis server latency: `redis-cli --latency`
2. Use in-memory cache for hot data (L1)
3. Reduce serialization overhead (smaller objects)
4. Use Redis pipelining for batch operations
5. Consider read replicas for read-heavy workloads

---

## Summary

**Key Configuration Points:**
- ✅ **Redis**: Connection string, SSL, timeouts, retry policy
- ✅ **Memory Cache**: Size limits, eviction policy, expiration scanning
- ✅ **Environment-specific**: Dev (fast), Staging (production-like), Prod (HA)
- ✅ **Secrets**: Use Key Vault / environment variables
- ✅ **Feature Flags**: Enable/disable caching dynamically
- ✅ **Monitoring**: Log configuration, track metrics

**Next Steps:**
1. Choose provider based on deployment model
2. Configure via appsettings.{env}.json
3. Store secrets in Key Vault
4. Test configuration in all environments
5. Set up monitoring and alerts
