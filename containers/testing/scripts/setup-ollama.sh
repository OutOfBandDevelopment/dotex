#!/bin/bash
# Setup script for Ollama integration testing
# Pulls phi3 model (small, CPU-friendly model for CI/CD)
#
# NOTE: This script is DEPRECATED - Ollama now auto-initializes on container startup.
# The container's custom entrypoint automatically pulls the phi3 model.
# This script is kept for manual testing/troubleshooting only.

set -e

CONTAINER_NAME="${CONTAINER_NAME:-oobd-test-ollama}"
MODEL="${OLLAMA_MODEL:-phi3}"

echo "========================================="
echo "Ollama Setup Script"
echo "========================================="
echo "Container: $CONTAINER_NAME"
echo "Model: $MODEL"
echo ""

# Check if container is running
if ! docker ps --format '{{.Names}}' | grep -q "^${CONTAINER_NAME}$"; then
    echo "Error: Container '$CONTAINER_NAME' is not running"
    echo "Please start the integration test stack first:"
    echo "  cd containers/testing"
    echo "  ./scripts/integration-up.sh"
    exit 1
fi

# Wait for Ollama to be healthy
echo "Waiting for Ollama to be ready..."
MAX_ATTEMPTS=30
ATTEMPT=0

while [ $ATTEMPT -lt $MAX_ATTEMPTS ]; do
    if docker exec $CONTAINER_NAME curl -sf http://localhost:11434/api/tags > /dev/null 2>&1; then
        echo "Ollama is ready!"
        break
    fi
    ATTEMPT=$((ATTEMPT+1))
    echo "Attempt $ATTEMPT/$MAX_ATTEMPTS - waiting..."
    sleep 2
done

if [ $ATTEMPT -eq $MAX_ATTEMPTS ]; then
    echo "Error: Ollama did not become ready in time"
    exit 1
fi

# Pull the model
echo ""
echo "Pulling model: $MODEL"
echo "This may take several minutes on first run..."
echo ""

docker exec $CONTAINER_NAME ollama pull $MODEL

echo ""
echo "========================================="
echo "Ollama Setup Complete!"
echo "========================================="
echo "Model '$MODEL' is ready for testing"
echo ""
echo "Test with:"
echo "  docker exec $CONTAINER_NAME ollama run $MODEL 'Hello!'"
echo ""
