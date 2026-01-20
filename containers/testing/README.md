# OoBDev Integration Test Infrastructure

This directory contains Docker-based testing infrastructure for running integration tests against real external services. The infrastructure supports both local development and CI/CD pipeline execution.

## Table of Contents

- [Overview](#overview)
- [Architecture](#architecture)
- [Quick Start](#quick-start)
- [Services](#services)
- [Usage](#usage)
- [Environment Variables](#environment-variables)
- [CI/CD Integration](#cicd-integration)
- [Troubleshooting](#troubleshooting)

---

## Overview

The integration test stack provides **13 Docker-based services** needed for Integration test category:

| Category | Services |
|----------|----------|
| **Stateless** | Apache Tika, SMTP4Dev |
| **Stateful** | MongoDB, SQL Server, RabbitMQ, Redis, OpenSearch, Qdrant |
| **Emulators** | Azurite (Azure Storage), LocalStack (AWS), Azure Service Bus Emulator |
| **Identity** | Keycloak |
| **AI/ML** | SBert (CPU-only) |

**Design Principles:**
- ✅ **Shared Infrastructure**: Same containers for local development and CI/CD
- ✅ **Ephemeral Volumes**: Clean state on each test run (`down -v`)
- ✅ **Health Checks**: Ensures services are ready before tests run
- ✅ **Isolated Network**: Test-specific network avoids conflicts
- ✅ **Configurable**: Environment variables for connection strings

---

## Architecture

### Container Deployment Diagram

```plantuml
@startuml
!define CONTAINER_BG_COLOR #E8F4F8
!define DATABASE_BG_COLOR #E8FFE8
!define QUEUE_BG_COLOR #FFF4E8
!define IDENTITY_BG_COLOR #F0E8FF
!define AI_BG_COLOR #FFE8CC

skinparam componentStyle rectangle

' Integration Test Network
package "oobd-integration-test-net (Bridge Network)" {

    ' Stateless Services
    component "Apache Tika\n:9998" <<Container>> #CONTAINER_BG_COLOR {
        [Document Parser]
    }

    component "SMTP4Dev\n:25, :7777" <<Container>> #CONTAINER_BG_COLOR {
        [Email Server]
    }

    ' Stateful Services - Databases
    component "SQL Server\n:1433" <<Container>> #DATABASE_BG_COLOR {
        database "SQL DB" as sqldb
    }

    component "MongoDB\n:27017" <<Container>> #DATABASE_BG_COLOR {
        database "Mongo DB" as mongodb
    }

    component "OpenSearch\n:9200, :9600" <<Container>> #DATABASE_BG_COLOR {
        database "Search Index" as opensearch
    }

    component "Qdrant\n:6333, :6334" <<Container>> #DATABASE_BG_COLOR {
        database "Vector DB" as qdrant
    }

    ' Messaging & Caching Services
    component "RabbitMQ\n:5672, :15672" <<Container>> #QUEUE_BG_COLOR {
        queue "Message Queue" as rabbitmq
    }

    component "Redis\n:6379" <<Container>> #DATABASE_BG_COLOR {
        database "Cache Store" as redis
    }

    ' Cloud Emulators
    component "Azurite\n:10000-10002" <<Container>> #CONTAINER_BG_COLOR {
        [Azure Storage\nEmulator]
    }

    component "LocalStack\n:4566" <<Container>> #CONTAINER_BG_COLOR {
        [AWS Services\nEmulator]
    }

    ' Identity Services
    component "Keycloak\n:8081" <<Container>> #IDENTITY_BG_COLOR {
        [Identity &\nAccess Mgmt]
    }

    ' AI/ML Services
    component "SBert\n:5080" <<Container>> #AI_BG_COLOR {
        [Sentence\nEmbeddings]
    }
}

' External connections
actor "Integration Tests\n(dotnet test)" as tests
cloud "Docker Host" as host

tests --> [Document Parser] : HTTP
tests --> [Email Server] : SMTP/HTTP
tests --> sqldb : T-SQL (1433)
tests --> mongodb : MongoDB Protocol (27017)
tests --> rabbitmq : AMQP (5672)
tests --> opensearch : HTTPS (9200)
tests --> qdrant : HTTP/gRPC (6333/6334)
tests --> [Azure Storage\nEmulator] : HTTP
tests --> [AWS Services\nEmulator] : HTTP
tests --> [Identity &\nAccess Mgmt] : HTTP
tests --> [Sentence\nEmbeddings] : HTTP

host --> tests : Runs on

' Volume Persistence
database "Ephemeral Volumes" as volumes {
    storage "mongodb-test-data"
    storage "sqlserver-test-data"
    storage "rabbitmq-test-data"
    storage "opensearch-test-data"
    storage "qdrant-test-storage"
    storage "azurite-test-data"
    storage "keycloak-test-data"
}

sqldb --> volumes
mongodb --> volumes
rabbitmq --> volumes
opensearch --> volumes
qdrant --> volumes
[Azure Storage\nEmulator] --> volumes
[Identity &\nAccess Mgmt] --> volumes

note right of volumes
  Volumes are created on 'up'
  and destroyed on 'down -v'
  for clean test state
end note

note bottom of tests
  Environment Variables:
  - MONGODB_CONNECTION_STRING
  - SQL_CONNECTION_STRING
  - RABBITMQ_HOST
  - REDIS_CONNECTION_STRING
  - OPENSEARCH_URL
  - QDRANT_URL
  - TIKA_URL
  - SMTP_HOST
  - AZURITE_BLOB_URL
  - KEYCLOAK_URL
  - SBERT_URL
end note

@enduml
```

### Network Topology

```plaintext
┌─────────────────────────────────────────────────────────────┐
│  oobd-integration-test-net (Bridge Network)                 │
│                                                             │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐     │
│  │  Tika    │  │  SMTP    │  │  Mongo   │  │   SQL    │     │
│  │  :9998   │  │  :25     │  │  :27017  │  │  :1433   │     │
│  └──────────┘  └──────────┘  └──────────┘  └──────────┘     │
│                                                             │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐     │
│  │ RabbitMQ │  │  Redis   │  │OpenSearch│  │  Qdrant  │     │
│  │  :5672   │  │  :6379   │  │  :9200   │  │  :6333   │     │
│  └──────────┘  └──────────┘  └──────────┘  └──────────┘     │
│                                                             │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐     │
│  │ Azurite  │  │LocalStack│  │ Keycloak │  │  SBert   │     │
│  │  :10000  │  │  :4566   │  │  :8081   │  │  :5080   │     │
│  └──────────┘  └──────────┘  └──────────┘  └──────────┘     │
│                                                             │
└─────────────────────────────────────────────────────────────┘
         ↑
         │ (port mapping)
         ↓
    Docker Host
    (localhost)
         ↑
         │
         ↓
  .NET Integration Tests
  (TestCategory=Integration)
```

---

## Quick Start

### Prerequisites

- **Docker Desktop** (Windows/Mac) or **Docker Engine** (Linux)
- **Docker Compose** v2.0+
- Ports available: 1433, 5672, 6333, 6379, 8081, 9200, 9998, 10000-10002, 27017

### Start Services (Linux/macOS)

```bash
# From repository root
cd containers/testing

# Start all services and wait for health checks
./scripts/integration-up.sh --wait

# Run integration tests
cd ../../src
dotnet test --filter "TestCategory=Integration"

# Stop services and cleanup
cd ../containers/testing
./scripts/integration-down.sh --clean
```

### Start Services (Windows)

```cmd
REM From repository root
cd containers\testing

REM Start all services and wait for health checks
scripts\integration-up.bat --wait

REM Run integration tests
cd ..\..\src
dotnet test --filter "TestCategory=Integration"

REM Stop services and cleanup
cd ..\containers\testing
scripts\integration-down.bat --clean
```

---

## Services

### Stateless Services

**No persistent data between runs**

| Service | Image | Port(s) | Purpose | Health Check |
|---------|-------|---------|---------|--------------|
| **Apache Tika** | `apache/tika` | 9998 | Document content detection | `curl http://localhost:9998/tika` |
| **SMTP4Dev** | `rnwood/smtp4dev` | 25 (SMTP)<br>7777 (Web) | Email testing | `curl http://localhost:7777` |

### Stateful Services

**Require cleanup between test runs**

| Service | Image | Port(s) | Purpose | Health Check |
|---------|-------|---------|---------|--------------|
| **MongoDB** | `mongo:latest` | 27017 | NoSQL document database | `mongosh --eval "db.adminCommand('ping')"` |
| **SQL Server** | `mcr.microsoft.com/mssql/server:2022-latest` | 1433 | Relational database | `sqlcmd -Q "SELECT 1"` |
| **RabbitMQ** | `rabbitmq:latest` | 5672 (AMQP)<br>15672 (Management) | Message broker | `rabbitmq-diagnostics ping` |
| **Redis** | `redis:7-alpine` | 6379 | In-memory cache store | `redis-cli ping` |
| **OpenSearch** | `opensearchproject/opensearch:latest` | 9200 (HTTP)<br>9600 (Performance) | Search engine | `curl https://localhost:9200/_cluster/health` |
| **Qdrant** | `qdrant/qdrant` | 6333 (HTTP)<br>6334 (gRPC) | Vector database | `curl http://localhost:6333/health` |

### Emulators

**Azure and AWS service emulation**

| Service | Image | Port(s) | Purpose | Health Check |
|---------|-------|---------|---------|--------------|
| **Azurite** | `mcr.microsoft.com/azure-storage/azurite` | 10000 (Blob)<br>10001 (Queue)<br>10002 (Table) | Azure Storage emulator | `nc -z localhost 10000` |
| **LocalStack** | `localstack/localstack` | 4566 | AWS services emulator (SQS, S3, etc.) | `curl http://localhost:4566/_localstack/health` |
| **Service Bus Emulator** | `mcr.microsoft.com/azure-messaging/servicebus-emulator` | 5672 (AMQP) | Azure Service Bus emulator | `nc -z localhost 5672` |

### Identity & AI/ML

| Service | Image | Port(s) | Purpose | Health Check |
|---------|-------|---------|---------|--------------|
| **Keycloak** | Custom (realm import) | 8081 | IAM (OAuth/OIDC) | `curl http://localhost:8080/health/ready` |
| **SBert** | Custom (Python ML) | 5080 | Sentence embeddings | `curl http://localhost:5000/health` |

---

## Usage

### Script Reference

| Script | Purpose | Options |
|--------|---------|---------|
| `integration-up.sh` / `.bat` | Start services | `--wait` (wait for health checks) |
| `integration-down.sh` / `.bat` | Stop services | `--clean` (remove volumes)<br>`--purge` (remove all) |
| `wait-for-services.sh` / `.bat` | Wait for health checks | `[timeout-seconds]` (default: 120) |

### Manual Docker Compose Commands

```bash
# Start services
docker compose -f docker-compose.integration-tests.yml up -d

# Check status
docker compose -f docker-compose.integration-tests.yml ps

# View logs
docker compose -f docker-compose.integration-tests.yml logs -f [service-name]

# Stop and remove volumes
docker compose -f docker-compose.integration-tests.yml down -v

# Restart single service
docker compose -f docker-compose.integration-tests.yml restart mongodb
```

### Test Execution

```bash
# Run all integration tests
dotnet test --filter "TestCategory=Integration"

# Run specific service tests
dotnet test --filter "TestCategory=Integration&FullyQualifiedName~MongoDB"
dotnet test --filter "TestCategory=Integration&FullyQualifiedName~SqlServer"
dotnet test --filter "TestCategory=Integration&FullyQualifiedName~RabbitMQ"

# Run with detailed output
dotnet test --filter "TestCategory=Integration" --logger "console;verbosity=detailed"

# Run with coverage
dotnet test --filter "TestCategory=Integration" --collect:"XPlat Code Coverage"
```

---

## Environment Variables

### Connection Strings

Tests use environment variables for connection strings. See `.env.integration` for defaults.

| Variable | Default | Purpose |
|----------|---------|---------|
| `SQL_CONNECTION_STRING` | `Server=localhost,1433;User Id=sa;Password=IntegrationTest123!;TrustServerCertificate=True` | SQL Server connection |
| `MONGODB_CONNECTION_STRING` | `mongodb://localhost:27017` | MongoDB connection |
| `RABBITMQ_CONNECTION_STRING` | `amqp://guest:guest@localhost:5672/` | RabbitMQ connection |
| `REDIS_CONNECTION_STRING` | `localhost:6379` | Redis connection |
| `OPENSEARCH_URL` | `https://localhost:9200` | OpenSearch endpoint |
| `QDRANT_URL` | `http://localhost:6333` | Qdrant HTTP endpoint |
| `TIKA_URL` | `http://localhost:9998` | Apache Tika endpoint |
| `SMTP_HOST` | `localhost` | SMTP server host |
| `AZURITE_CONNECTION_STRING` | (see .env file) | Azurite connection |
| `LOCALSTACK_URL` | `http://localhost:4566` | LocalStack endpoint |
| `SQS_QUEUE_URL` | `http://localhost:4566/000000000000/{queue-name}` | AWS SQS queue URL |
| `SERVICEBUS_CONNECTION_STRING` | `Endpoint=sb://localhost;...;UseDevelopmentEmulator=true;` | Azure Service Bus connection |
| `KEYCLOAK_URL` | `http://localhost:8081` | Keycloak endpoint |
| `SBERT_URL` | `http://localhost:5080` | SBert endpoint |

### Test Configuration in C#

```csharp
[TestMethod]
[TestCategory(TestCategories.Integration)]
public async Task TestMongoDBOperation()
{
    // Read from environment or use default
    var connectionString = Environment.GetEnvironmentVariable("MONGODB_CONNECTION_STRING")
        ?? "mongodb://localhost:27017";
    var databaseName = $"IntegrationTest_{Guid.NewGuid():N}";

    var config = new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?>
        {
            { "MongoDB:ConnectionString", connectionString },
            { "MongoDB:DatabaseName", databaseName },
        })
        .Build();

    // Use in test...

    // Cleanup in [TestCleanup]
    await client.DropDatabaseAsync(databaseName);
}
```

---

## CI/CD Integration

### GitHub Actions Workflow

Integration tests run **daily at 4 PM UTC** via `.github/workflows/integration-tests.yml`:

```yaml
# Triggered by:
# - Scheduled: Daily at 4 PM UTC (cron: '0 16 * * *')
# - Manual: workflow_dispatch

steps:
  - name: Start Integration Test Services
    working-directory: ./containers/testing
    run: |
      docker compose -f docker-compose.integration-tests.yml up -d

  - name: Wait for Services
    working-directory: ./containers/testing
    run: ./scripts/wait-for-services.sh
    timeout-minutes: 5

  - name: Run Integration Tests
    working-directory: ./src
    env:
      MONGODB_CONNECTION_STRING: mongodb://localhost:27017
      SQL_CONNECTION_STRING: Server=localhost,1433;User Id=sa;Password=IntegrationTest123!;TrustServerCertificate=True
      # ... (other environment variables)
    run: >
      dotnet test
      --configuration Release
      --filter "TestCategory=Integration"
      --collect:"XPlat Code Coverage"

  - name: Stop Docker Services
    if: always()
    working-directory: ./containers/testing
    run: docker compose -f docker-compose.integration-tests.yml down -v
```

### Pipeline Flow

```
Build Pipeline (dotnet.yml)
  ├─ Push/PR trigger
  ├─ Build + Unit/Simulate tests
  ├─ Create packages
  ├─ Upload artifacts (90 days)
  └─ Tag: v{version}
          ↓
Daily at 4 PM UTC
          ↓
Integration Tests (integration-tests.yml)
  ├─ Start Docker services
  ├─ Wait for health checks
  ├─ Run Integration tests
  ├─ Stop Docker services
  └─ Tag: validated-v{version} (on success)
          ↓
Manual Release (release.yml)
  ├─ Find validated artifact
  └─ Deploy to NuGet
```

---

## Troubleshooting

### Services Not Starting

**Problem**: Docker containers fail to start

**Solutions**:
```bash
# Check Docker is running
docker version

# Check port availability
netstat -an | grep 1433   # SQL Server
netstat -an | grep 27017  # MongoDB
# ... check other ports

# View container logs
docker compose -f docker-compose.integration-tests.yml logs mongodb
docker compose -f docker-compose.integration-tests.yml logs sql-server

# Restart with clean state
./scripts/integration-down.sh --purge
./scripts/integration-up.sh --wait
```

### Services Not Healthy

**Problem**: Health checks timeout after 2 minutes

**Solutions**:
```bash
# Check service status
docker compose -f docker-compose.integration-tests.yml ps

# View detailed logs for unhealthy service
docker compose -f docker-compose.integration-tests.yml logs opensearch
docker compose -f docker-compose.integration-tests.yml logs sql-server

# Common issues:
# - SQL Server: Password policy not met (use strong password)
# - OpenSearch: vm.max_map_count too low (Linux host)
# - Keycloak: Slow startup on first run (database initialization)

# Increase wait timeout
./scripts/wait-for-services.sh 300  # Wait up to 5 minutes
```

### Test Failures

**Problem**: Integration tests fail when services are healthy

**Solutions**:
```bash
# Verify connection strings
echo $MONGODB_CONNECTION_STRING
echo $SQL_CONNECTION_STRING

# Test connectivity manually
mongosh --eval "db.adminCommand('ping')"
sqlcmd -S localhost,1433 -U sa -P IntegrationTest123! -Q "SELECT 1"

# Check test data cleanup
# Tests should create unique databases/collections per run
# and clean up in [TestCleanup]

# Run tests with verbose output
dotnet test --filter "TestCategory=Integration" --logger "console;verbosity=detailed"
```

### Port Conflicts

**Problem**: Port already in use

**Solutions**:
```bash
# Find process using port (Linux/macOS)
lsof -i :1433
lsof -i :27017

# Find process using port (Windows)
netstat -ano | findstr :1433
netstat -ano | findstr :27017

# Stop conflicting process or change port mapping in docker-compose file
```

### Disk Space Issues

**Problem**: Docker volumes consuming too much disk space

**Solutions**:
```bash
# Remove test volumes
docker volume rm oobd-test-mongodb-data
docker volume rm oobd-test-sqlserver-data
# ... (or use integration-down.sh --clean)

# Remove all unused Docker volumes
docker volume prune

# Check Docker disk usage
docker system df
```

### OpenSearch Memory Issues (Linux)

**Problem**: OpenSearch fails to start with memory errors

**Solution**:
```bash
# Increase vm.max_map_count on Linux host
sudo sysctl -w vm.max_map_count=262144

# Make permanent
echo "vm.max_map_count=262144" | sudo tee -a /etc/sysctl.conf
```

---

## Related Documentation

- [Test Categories Guide](../../docs/architecture/testing/test-categories.md)
- [Integration Testing Guide](../../docs/architecture/testing/categories/integration/README.md)
- [Docker Infrastructure Guide](../../docs/architecture/testing/docker-infrastructure.md)
- [Environment Variables Reference](../../docs/architecture/testing/environment-variables.md)

---

## Quick Reference

### Startup Sequence

```bash
cd containers/testing
./scripts/integration-up.sh --wait    # Start and wait
cd ../../src
dotnet test --filter "TestCategory=Integration"
cd ../containers/testing
./scripts/integration-down.sh --clean  # Stop and cleanup
```

### Service URLs

- **Apache Tika**: http://localhost:9998
- **SMTP4Dev Web UI**: http://localhost:7777
- **RabbitMQ Management**: http://localhost:15672 (guest/guest)
- **OpenSearch**: https://localhost:9200 (admin/IntegrationTest123!)
- **Keycloak**: http://localhost:8081 (admin/admin)
- **Qdrant**: http://localhost:6333
- **LocalStack**: http://localhost:4566

### Container Names

All containers are prefixed with `oobd-test-`:
- `oobd-test-tika`
- `oobd-test-smtp`
- `oobd-test-mongodb`
- `oobd-test-sqlserver`
- `oobd-test-rabbitmq`
- `oobd-test-redis`
- `oobd-test-opensearch`
- `oobd-test-qdrant`
- `oobd-test-azurite`
- `oobd-test-localstack`
- `oobd-test-servicebus`
- `oobd-test-keycloak`
- `oobd-test-sbert`

---

**Last Updated**: 2026-01-19
**Version**: 1.0.0
