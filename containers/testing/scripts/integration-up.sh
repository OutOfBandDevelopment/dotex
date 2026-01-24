#!/bin/bash
# OoBDev Integration Test Stack - Startup Script
# Starts all Docker services needed for integration testing
#
# Usage:
#   ./scripts/integration-up.sh                # Start in detached mode
#   ./scripts/integration-up.sh --wait         # Start and wait for health checks
#   ./scripts/integration-up.sh --build        # Rebuild images before starting
#   ./scripts/integration-up.sh --build --wait # Rebuild and wait for health checks
#
# Requirements:
#   - Docker and Docker Compose installed
#   - Ports available: 25, 1433, 4566, 5000, 5080, 5672-5673, 6333-6334, 6379, 7777, 8081, 9200, 9998, 10000-10002, 11434, 15672, 27017

set -e  # Exit on error

# Get script directory
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
TESTING_DIR="$(cd "${SCRIPT_DIR}/.." && pwd)"

# Parse command line arguments
BUILD_FLAG=""
WAIT_FLAG=""

for arg in "$@"; do
    case $arg in
        --build)
            BUILD_FLAG="--build"
            ;;
        --wait)
            WAIT_FLAG="true"
            ;;
    esac
done

# Change to testing directory
cd "$TESTING_DIR"

echo "======================================================================"
echo "OoBDev Integration Test Stack - Starting Services"
echo "======================================================================"
echo ""
echo "Working directory: $TESTING_DIR"
echo "Compose file: docker-compose.integration-tests.yml"
echo "Environment: .env.integration"
echo ""

# Load environment variables
if [ -f ".env.integration" ]; then
    echo "Loading environment from .env.integration"
    set -a
    source .env.integration
    set +a
else
    echo "Warning: .env.integration not found, using defaults"
fi

echo ""
if [ -n "$BUILD_FLAG" ]; then
    echo "Rebuilding Docker images and starting containers..."
else
    echo "Starting Docker containers..."
fi
echo ""

# Start services in detached mode
docker compose -f docker-compose.integration-tests.yml up -d $BUILD_FLAG

echo ""
echo "✅ Docker services started successfully"
echo ""
echo "Services running:"
echo "  - Apache Tika:        http://localhost:9998"
echo "  - SMTP4Dev:           http://localhost:7777"
echo "  - MongoDB:            mongodb://localhost:27017"
echo "  - SQL Server:         localhost,1433 (sa/${SQL_SA_PASSWORD:-IntegrationTest123!})"
echo "  - RabbitMQ:           amqp://localhost:5673, http://localhost:15672"
echo "  - Redis:              localhost:6379"
echo "  - OpenSearch:         https://localhost:9200 (admin/${OPENSEARCH_PASSWORD:-IntegrationTest123!})"
echo "  - Qdrant:             http://localhost:6333"
echo "  - Azurite:            http://localhost:10000 (Blob), 10001 (Queue), 10002 (Table)"
echo "  - LocalStack:         http://localhost:4566"
echo "  - Service Bus:        localhost:5672"
echo "  - Keycloak:           http://localhost:8081 (admin/admin)"
echo "  - SBert:              http://localhost:5080"
echo "  - Ollama:             http://localhost:11434"
echo "  - Azurinsight:        http://localhost:5000"
echo ""

# Check if --wait flag is provided
if [ "$WAIT_FLAG" = "true" ]; then
    echo "Waiting for services to become healthy..."
    echo ""
    "${SCRIPT_DIR}/wait-for-services.sh"

    if [ $? -eq 0 ]; then
        echo ""
        echo "======================================================================"
        echo "✅ All services are healthy!"
        echo "======================================================================"
        echo ""

        # Setup Ollama model if container is running
        echo "Setting up Ollama model..."
        if "${SCRIPT_DIR}/setup-ollama.sh"; then
            echo "✅ Ollama model ready"
        else
            echo "⚠️  Ollama model setup failed (may already be installed)"
        fi

        echo ""
        echo "======================================================================"
        echo "✅ Stack is ready for testing!"
        echo "======================================================================"
        echo ""
        echo "You can now run integration tests:"
        echo "  cd ../../src"
        echo "  dotnet test --filter TestCategory=Integration"
        echo ""
    else
        echo ""
        echo "======================================================================"
        echo "❌ Some services failed to become healthy"
        echo "======================================================================"
        echo ""
        echo "Check service health:"
        echo "  docker compose -f docker-compose.integration-tests.yml ps"
        echo ""
        echo "View logs:"
        echo "  docker compose -f docker-compose.integration-tests.yml logs [service-name]"
        echo ""
        exit 1
    fi
else
    echo "Services are starting in the background..."
    echo ""
    echo "To wait for health checks:"
    echo "  ${SCRIPT_DIR}/wait-for-services.sh"
    echo ""
    echo "To check status:"
    echo "  docker compose -f docker-compose.integration-tests.yml ps"
    echo ""
    echo "To view logs:"
    echo "  docker compose -f docker-compose.integration-tests.yml logs -f [service-name]"
    echo ""
    echo "To stop services:"
    echo "  ${SCRIPT_DIR}/integration-down.sh"
    echo ""
fi
