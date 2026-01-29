#!/bin/bash
# Get Azure Service Bus emulator connection string for integration testing
#
# NOTE: Service Bus emulator auto-initializes on container startup using Config.json.
# Queues and topics are automatically created from servicebus-config/Config.json.
# This script just displays connection information for reference.

set -e

SERVICEBUS_HOST="${SERVICEBUS_HOST:-localhost}"
SERVICEBUS_PORT="${SERVICEBUS_PORT:-5672}"

echo "Azure Service Bus Emulator Configuration"
echo "=========================================="
echo ""
echo "The Azure Service Bus emulator is running at:"
echo "  Host: $SERVICEBUS_HOST"
echo "  Port: $SERVICEBUS_PORT (AMQP)"
echo ""
echo "Pre-configured entities:"
echo "  - Queue: integration-test-queue"
echo "  - Topic: integration-test-topic"
echo ""
echo "Connection string for tests:"
echo "  Endpoint=sb://$SERVICEBUS_HOST;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=SAS_KEY_VALUE;UseDevelopmentEmulator=true;"
echo ""
echo "Note: The emulator uses a default connection string."
echo "      Check https://learn.microsoft.com/en-us/azure/service-bus-messaging/overview-emulator"
echo "      for the latest connection string format."
echo ""
echo "Environment variable for tests:"
echo "  export SERVICEBUS_CONNECTION_STRING=\"Endpoint=sb://$SERVICEBUS_HOST;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=SAS_KEY_VALUE;UseDevelopmentEmulator=true;\""
echo ""
echo "Use in C# tests:"
echo '  var connectionString = Environment.GetEnvironmentVariable("SERVICEBUS_CONNECTION_STRING")'
echo '      ?? "Endpoint=sb://localhost;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=SAS_KEY_VALUE;UseDevelopmentEmulator=true;";'
echo ""
echo "✅ Azure Service Bus emulator is ready for testing!"
