#!/bin/bash
# OoBDev Integration Test Stack - Health Check Script
# Waits for all Docker services to become healthy before proceeding
#
# Usage:
#   ./scripts/wait-for-services.sh            # Wait up to 120 seconds (default)
#   ./scripts/wait-for-services.sh 180        # Wait up to 180 seconds
#
# Exit codes:
#   0 - All services are healthy
#   1 - Timeout reached, some services are not healthy
#
# This script is used by:
#   - integration-up.sh --wait (local testing)
#   - GitHub Actions integration-tests.yml (CI/CD)

set -e  # Exit on error

# Get script directory
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
TESTING_DIR="$(cd "${SCRIPT_DIR}/.." && pwd)"

# Change to testing directory
cd "$TESTING_DIR"

# Configuration
TIMEOUT=${1:-120}  # Default 120 seconds (2 minutes)
INTERVAL=5         # Check every 5 seconds
COMPOSE_FILE="docker-compose.integration-tests.yml"

# Service list (container names from docker-compose file)
SERVICES=(
    "oobd-test-tika"
    "oobd-test-smtp"
    "oobd-test-mongodb"
    "oobd-test-sqlserver"
    "oobd-test-rabbitmq"
    "oobd-test-redis"
    "oobd-test-opensearch"
    "oobd-test-qdrant"
    "oobd-test-azurite"
    "oobd-test-localstack"
    "oobd-test-servicebus"
    "oobd-test-keycloak"
    "oobd-test-sbert"
    "oobd-test-ollama"
    "oobd-test-azurinsight"
)

# Colors for output (if terminal supports it)
if [ -t 1 ]; then
    GREEN='\033[0;32m'
    YELLOW='\033[1;33m'
    RED='\033[0;31m'
    NC='\033[0m' # No Color
else
    GREEN=''
    YELLOW=''
    RED=''
    NC=''
fi

echo "======================================================================"
echo "Waiting for Docker services to become healthy..."
echo "======================================================================"
echo ""
echo "Timeout: ${TIMEOUT} seconds"
echo "Check interval: ${INTERVAL} seconds"
echo "Services to check: ${#SERVICES[@]}"
echo ""

# Function to check if a container is healthy
check_container_health() {
    local container_name=$1
    local health_status

    # Check if container exists
    if ! docker ps -a --format '{{.Names}}' | grep -q "^${container_name}$"; then
        echo "not_found"
        return
    fi

    # Get health status (trim whitespace and newlines)
    health_status=$(docker inspect --format='{{.State.Health.Status}}' "$container_name" 2>/dev/null | tr -d '\n\r' || echo "no_healthcheck")

    # If no healthcheck defined, check if container is running
    if [[ "$health_status" == "" || "$health_status" == "no_healthcheck" || "$health_status" == *"has no entry for key"* ]]; then
        local container_state=$(docker inspect --format='{{.State.Status}}' "$container_name" 2>/dev/null | tr -d '\n\r' || echo "unknown")
        if [ "$container_state" = "running" ]; then
            echo "running"
        else
            echo "not_running"
        fi
    else
        echo "$health_status"
    fi
}

# Main health check loop
elapsed=0
all_healthy=false

while [ $elapsed -lt $TIMEOUT ]; do
    all_healthy=true
    unhealthy_services=()
    starting_services=()
    missing_services=()

    # Check each service
    for service in "${SERVICES[@]}"; do
        health=$(check_container_health "$service")

        case "$health" in
            "healthy"|"running")
                # Service is ready
                ;;
            "starting")
                all_healthy=false
                starting_services+=("$service")
                ;;
            "unhealthy")
                all_healthy=false
                unhealthy_services+=("$service")
                ;;
            "not_found")
                all_healthy=false
                missing_services+=("$service")
                ;;
            "not_running")
                all_healthy=false
                unhealthy_services+=("$service")
                ;;
            *)
                all_healthy=false
                unhealthy_services+=("$service")
                ;;
        esac
    done

    # Print status
    if [ "$all_healthy" = true ]; then
        echo -e "${GREEN}✅ All services are healthy!${NC}"
        exit 0
    else
        # Clear previous line (only in interactive terminal)
        if [ -t 1 ]; then
            echo -ne "\r\033[K"
        fi

        # Count services by status
        healthy_count=$((${#SERVICES[@]} - ${#starting_services[@]} - ${#unhealthy_services[@]} - ${#missing_services[@]}))

        echo -ne "${YELLOW}⏳ Waiting... [${elapsed}s/${TIMEOUT}s] "
        echo -ne "Healthy: ${healthy_count}/${#SERVICES[@]} "

        if [ ${#starting_services[@]} -gt 0 ]; then
            echo -ne "Starting: ${#starting_services[@]} "
        fi

        if [ ${#unhealthy_services[@]} -gt 0 ]; then
            echo -ne "Unhealthy: ${#unhealthy_services[@]} "
        fi

        if [ ${#missing_services[@]} -gt 0 ]; then
            echo -ne "Missing: ${#missing_services[@]} "
        fi

        echo -ne "${NC}"

        # Only print newline if not in terminal (for CI/CD logs)
        if [ ! -t 1 ]; then
            echo ""
        fi
    fi

    # Wait before next check
    sleep $INTERVAL
    elapsed=$((elapsed + INTERVAL))
done

# Timeout reached
echo ""
echo ""
echo -e "${RED}❌ Timeout reached (${TIMEOUT}s)${NC}"
echo ""
echo "Services that are not healthy:"
echo ""

# Show detailed status for each service
for service in "${SERVICES[@]}"; do
    health=$(check_container_health "$service")
    case "$health" in
        "healthy"|"running")
            echo -e "  ${GREEN}✅${NC} $service"
            ;;
        "starting")
            echo -e "  ${YELLOW}⏳${NC} $service (still starting)"
            ;;
        "unhealthy"|"not_running")
            echo -e "  ${RED}❌${NC} $service (unhealthy)"
            ;;
        "not_found")
            echo -e "  ${RED}❌${NC} $service (not found)"
            ;;
        *)
            echo -e "  ${RED}❌${NC} $service (status: $health)"
            ;;
    esac
done

echo ""
echo "Troubleshooting:"
echo ""
echo "1. Check service status:"
echo "   docker compose -f ${COMPOSE_FILE} ps"
echo ""
echo "2. View logs for failing services:"
for service in "${SERVICES[@]}"; do
    health=$(check_container_health "$service")
    if [ "$health" != "healthy" ] && [ "$health" != "running" ]; then
        short_name="${service#oobd-test-}"
        echo "   docker compose -f ${COMPOSE_FILE} logs ${short_name}"
    fi
done
echo ""
echo "3. Restart services:"
echo "   ${SCRIPT_DIR}/integration-down.sh --clean"
echo "   ${SCRIPT_DIR}/integration-up.sh"
echo ""

exit 1
