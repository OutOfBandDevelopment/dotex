#!/bin/bash
# OoBDev Integration Test Stack - Shutdown Script
# Stops and optionally removes all Docker services and volumes
#
# Usage:
#   ./scripts/integration-down.sh           # Stop containers, keep volumes
#   ./scripts/integration-down.sh --clean   # Stop containers and remove volumes
#   ./scripts/integration-down.sh --purge   # Stop, remove volumes, and remove network
#
# Note: Using --clean removes all test data (recommended for CI/CD)

set -e  # Exit on error

# Get script directory
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
TESTING_DIR="$(cd "${SCRIPT_DIR}/.." && pwd)"

# Change to testing directory
cd "$TESTING_DIR"

echo "======================================================================"
echo "OoBDev Integration Test Stack - Stopping Services"
echo "======================================================================"
echo ""

# Determine cleanup mode
REMOVE_VOLUMES=false
REMOVE_ORPHANS=false

if [[ "$1" == "--clean" || "$1" == "--purge" ]]; then
    REMOVE_VOLUMES=true
    echo "Mode: CLEAN (will remove volumes and test data)"
else
    echo "Mode: STOP (will keep volumes and test data)"
fi

if [[ "$1" == "--purge" ]]; then
    REMOVE_ORPHANS=true
    echo "Mode: PURGE (will also remove orphaned containers)"
fi

echo ""
echo "Stopping Docker containers..."
echo ""

# Stop services
if [ "$REMOVE_VOLUMES" = true ]; then
    if [ "$REMOVE_ORPHANS" = true ]; then
        docker compose -f docker-compose.integration-tests.yml down -v --remove-orphans
    else
        docker compose -f docker-compose.integration-tests.yml down -v
    fi
else
    docker compose -f docker-compose.integration-tests.yml down
fi

echo ""

if [ "$REMOVE_VOLUMES" = true ]; then
    echo "✅ Docker services stopped and volumes removed"
    echo ""
    echo "All test data has been cleaned up:"
    echo "  - MongoDB databases deleted"
    echo "  - SQL Server databases deleted"
    echo "  - RabbitMQ queues deleted"
    echo "  - OpenSearch indices deleted"
    echo "  - Qdrant collections deleted"
    echo "  - Azurite blobs/queues/tables deleted"
    echo "  - Keycloak data deleted"
    echo ""
else
    echo "✅ Docker services stopped (volumes preserved)"
    echo ""
    echo "Test data has been preserved in Docker volumes."
    echo ""
    echo "To remove volumes and clean up all test data:"
    echo "  ${SCRIPT_DIR}/integration-down.sh --clean"
    echo ""
    echo "To manually remove volumes:"
    echo "  docker volume rm oobd-test-mongodb-data"
    echo "  docker volume rm oobd-test-sqlserver-data"
    echo "  docker volume rm oobd-test-rabbitmq-data"
    echo "  docker volume rm oobd-test-opensearch-data"
    echo "  docker volume rm oobd-test-qdrant-storage"
    echo "  docker volume rm oobd-test-qdrant-snapshots"
    echo "  docker volume rm oobd-test-azurite-data"
    echo "  docker volume rm oobd-test-keycloak-data"
    echo ""
fi

echo "To start services again:"
echo "  ${SCRIPT_DIR}/integration-up.sh"
echo ""
