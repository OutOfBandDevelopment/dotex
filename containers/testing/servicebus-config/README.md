# Azure Service Bus Emulator Configuration

This directory contains configuration files for the Azure Service Bus Emulator used in integration testing.

## Files

### Config.json

The main configuration file for the Service Bus emulator. This file defines:

**Namespaces:**
- `sbemulatorns` - **FIXED** emulator namespace (cannot be changed, 12 characters)

**Queues:**
- `integration-test-queue` - Standard queue with 10 max delivery attempts, 1 minute lock duration

**Topics:**
- `integration-test-topic` - Pub/sub topic with 14-day message TTL
  - **Subscription:** `test-subscription` - 10 max delivery attempts, 1 minute lock duration

**Logging:**
- Console logging (Type: "Console")

## How It Works

The Service Bus emulator requires SQL Server for persistence. The docker-compose configuration:

1. **Starts SQL Server first** (via `depends_on` with health check)
2. **Mounts Config.json** into `/ServiceBus_Emulator/ConfigFiles/Config.json`
3. **Passes SQL credentials** via environment variables:
   - `SQL_SERVER=oobd-test-sqlserver`
   - `MSSQL_SA_PASSWORD=${SQL_SA_PASSWORD:-IntegrationTest123!}`
4. **Memory limits** set to prevent crashes:
   - Limit: 2GB maximum
   - Reservation: 512MB minimum

## Connection String

Use this connection string in integration tests:

```csharp
var connectionString = TestContext.GetProperty<string>("SERVICEBUS_CONNECTION_STRING")
    ?? "Endpoint=sb://localhost;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=SAS_KEY_VALUE;UseDevelopmentEmulator=true;";
```

## Configuration Format

The Config.json follows the Azure Service Bus Emulator schema (from [official Microsoft repo](https://github.com/Azure/azure-service-bus-emulator-installer/blob/main/ServiceBus-Emulator/Config/Config.json)):

```json
{
  "UserConfig": {
    "Namespaces": [
      {
        "Name": "sbemulatorns",
        "Queues": [
          {
            "Name": "queue-name",
            "Properties": {
              "LockDuration": "PT1M",
              "MaxDeliveryCount": 10,
              "RequiresSession": false,
              "DefaultMessageTimeToLive": "P14D",
              "DeadLetteringOnMessageExpiration": false,
              "DuplicateDetectionHistoryTimeWindow": "PT10M",
              "RequiresDuplicateDetection": false
            }
          }
        ],
        "Topics": [
          {
            "Name": "topic-name",
            "Properties": {
              "DefaultMessageTimeToLive": "P14D",
              "DuplicateDetectionHistoryTimeWindow": "PT10M",
              "RequiresDuplicateDetection": false
            },
            "Subscriptions": [
              {
                "Name": "subscription-name",
                "Properties": {
                  "LockDuration": "PT1M",
                  "MaxDeliveryCount": 10,
                  "RequiresSession": false,
                  "DefaultMessageTimeToLive": "P14D",
                  "DeadLetteringOnMessageExpiration": false
                }
              }
            ]
          }
        ]
      }
    ],
    "Logging": {
      "Type": "Console"
    }
  }
}
```

**⚠️ CRITICAL REQUIREMENTS:**

1. **Namespace name MUST be `sbemulatorns`** - This is hardcoded in the emulator and cannot be changed
   - Length must be exactly 12 characters
   - Any other value will cause: `Expected string to be "sbemulatorns" with a length of 12`

2. **Queues and Topics require nested `Properties` object** - All queue/topic/subscription properties must be inside a "Properties" object
   - Omitting Properties will cause null reference exceptions

3. **Logging property name is `Logging`** (not `LoggingConfig`)
   - Valid types: `"Console"` or `"File"`
   - No `Level` field is needed

4. **Duration formats use ISO 8601**
   - `PT1M` = 1 minute
   - `PT1H` = 1 hour
   - `P14D` = 14 days
   - `PT10M` = 10 minutes

## Modifying Configuration

To add new queues or topics:

1. Edit `Config.json` following the schema above
2. **DO NOT change the namespace name** - it must remain `sbemulatorns`
3. Add queues and topics within the `sbemulatorns` namespace
4. Restart the Docker services: `./scripts/integration-down.sh && ./scripts/integration-up.sh`
5. Verify entities are created by checking the emulator logs: `docker logs oobd-test-servicebus`

**Note:** You cannot add additional namespaces - the emulator only supports the single `sbemulatorns` namespace.

## Troubleshooting

### "SQL DB Unhealthy" error

**Symptom:** Emulator exits with code 139, logs show SQL connection failures

**Cause:** SQL Server not ready or connection string incorrect

**Fix:**
1. Verify SQL Server is healthy: `docker ps | grep oobd-test-sqlserver`
2. Check SQL Server logs: `docker logs oobd-test-sqlserver`
3. Verify SQL_SERVER and MSSQL_SA_PASSWORD environment variables match

### Entities not created

**Symptom:** Queues/topics from Config.json don't appear in the emulator

**Cause:** Config.json not mounted correctly or invalid JSON

**Fix:**
1. Validate JSON: `cat servicebus-config/Config.json | jq .`
2. Check mount in container: `docker exec oobd-test-servicebus cat /ServiceBus_Emulator/ConfigFiles/Config.json`
3. Restart emulator after Config.json changes

### "Expected string to be 'sbemulatorns'" error

**Symptom:** Emulator exits with validation error about namespace name

**Full Error:**
```
Recoverable validation failed on user config:
Expected string to be "sbemulatorns" with a length of 12 because NamespaceName is non-modifiable.
```

**Cause:** Config.json uses a namespace name other than `sbemulatorns`

**Fix:**
1. Edit `servicebus-config/Config.json`
2. Change `"Name"` field in the namespace to exactly `"sbemulatorns"` (12 characters)
3. This is hardcoded in the emulator and cannot be customized

### "Logging config cannot be null" or NullReferenceException

**Symptom:** Emulator exits with validation warnings and null reference exception

**Cause:** Config.json format doesn't match the required schema

**Fix:**
1. Edit `servicebus-config/Config.json`
2. Ensure property name is `"Logging"` (not `"LoggingConfig"`)
3. Set `"Type": "Console"` or `"Type": "File"`
4. Do NOT include a `Level` field
5. Ensure Queues/Topics have nested `Properties` objects
6. Use ISO 8601 duration formats (PT1M, P14D, etc.)

## References

- [Azure Service Bus Emulator Overview](https://learn.microsoft.com/en-us/azure/service-bus-messaging/overview-emulator)
- [Test locally with Service Bus Emulator](https://learn.microsoft.com/en-us/azure/service-bus-messaging/test-locally-with-service-bus-emulator)
- [Official Config.json Example](https://github.com/Azure/azure-service-bus-emulator-installer/blob/main/ServiceBus-Emulator/Config/Config.json)
- [Service Bus Emulator GitHub](https://github.com/Azure/azure-service-bus-emulator-installer)
- [Service Bus Emulator Docker Image](https://mcr.microsoft.com/product/azure-messaging/servicebus-emulator/about)
- [Service Bus .NET SDK](https://learn.microsoft.com/en-us/dotnet/api/overview/azure/messaging.servicebus-readme)

---

**Last Updated:** 2026-01-20
**Docker Image:** `mcr.microsoft.com/azure-messaging/servicebus-emulator:latest`
**Port:** 5672 (AMQP)
