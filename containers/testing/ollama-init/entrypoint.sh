#!/bin/bash
# Ollama custom entrypoint that pulls phi3 model on startup

set -e

echo "Starting Ollama service..."

# Start Ollama in the background
/bin/ollama serve &
OLLAMA_PID=$!

echo "Waiting for Ollama to be ready..."
sleep 5

# Wait for Ollama API to be available
MAX_ATTEMPTS=30
ATTEMPT=0
while [ $ATTEMPT -lt $MAX_ATTEMPTS ]; do
    if curl -sf http://localhost:11434/api/tags > /dev/null 2>&1; then
        echo "Ollama is ready!"
        break
    fi
    ATTEMPT=$((ATTEMPT+1))
    sleep 2
done

if [ $ATTEMPT -eq $MAX_ATTEMPTS ]; then
    echo "Warning: Ollama did not become ready in time"
else
    # Pull phi3 model
    echo "Pulling phi3 model (this may take a few minutes on first run)..."
    /bin/ollama pull phi3 || echo "Warning: Failed to pull phi3 model (may already exist)"
    echo "Ollama initialization complete!"
fi

# Keep Ollama running in foreground
wait $OLLAMA_PID
