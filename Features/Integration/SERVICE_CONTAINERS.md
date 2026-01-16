# Service Container Specifications

**Purpose:** Detailed configuration for each Docker service used in integration testing

## Executive Summary

This document provides comprehensive specifications for all Docker containers used in integration testing OoBDev. It details six required services (SQL Server, RabbitMQ, MongoDB, Qdrant, OpenSearch) and two optional services (Ollama, Keycloak) with complete configuration, environment variables, health checks, and initialization procedures. **All containers use ephemeral storage** — no persistent volumes. Initialization scripts (init.sql, definitions.json, init.js) are mounted read-only for schema/configuration setup. Test data is created/imported programmatically by test fixtures during execution using SQL commands, REST APIs, and client libraries. The document includes three test data initialization strategies (SQL scripts, test fixtures, and CSV/JSON imports), a container composition matrix for minimal/standard/full setups, startup optimization strategies, memory and CPU limits, and implementation guidance. All configurations are production-ready for GitHub Actions runners with clean-slate, isolated test execution.

## Table of Contents

- [Service Requirements by OoBDev Project](#service-requirements-by-oodev-project)
- [SQL Server 2019+ (REQUIRED)](#sql-server-2019-required)
- [RabbitMQ 3.12 (REQUIRED)](#rabbitmq-312-required)
- [MongoDB 7.0 (REQUIRED)](#mongodb-70-required)
- [Qdrant Vector Database (REQUIRED)](#qdrant-vector-database-required)
- [OpenSearch 2.0 (REQUIRED)](#opensearch-20-required)
- [Ollama (OPTIONAL)](#ollama-optional)
- [Container Composition Matrix](#container-composition-matrix)
- [Container Startup Optimization](#container-startup-optimization)
- [Memory & CPU Limits](#memory--cpu-limits)
- [Next Steps](#next-steps)

## Service Requirements by OoBDev Project

```plantuml
@startuml ServiceRequirements
!define REQUIRED #E8F4F8
!define OPTIONAL #FFF4E8
!define DEPENDENT #F0E8FF

map "OoBDev.Data.Vectors.DB" as VectorDB {
    Service => SQL Server
    Priority => REQUIRED
    Port => 1433
    Database => VectorDb
    Features => Service Broker
}

map "OoBDev.Microsoft.SqlServer.Server" as SqlServer {
    Service => SQL Server
    Priority => REQUIRED
    Port => 1433
    Database => Multiple
    Features => Native client, CLR integration
}

map "OoBDev.RabbitMQ" as RabbitMQ {
    Service => RabbitMQ
    Priority => REQUIRED
    Port => 5672, 15672
    Features => AMQP, Admin UI
}

map "OoBDev.MongoDB" as MongoDB {
    Service => MongoDB
    Priority => REQUIRED
    Port => 27017
    Features => NoSQL documents
}

map "OoBDev.Qdrant" as Qdrant {
    Service => Qdrant
    Priority => REQUIRED
    Port => 6333
    Features => Vector search
}

map "OoBDev.OpenSearch" as OpenSearch {
    Service => OpenSearch
    Priority => REQUIRED
    Port => 9200
    Features => Full-text search
}

map "OoBDev.Ollama" as Ollama {
    Service => Ollama
    Priority => OPTIONAL
    Port => 11434
    Features => Local LLM
}

map "OoBDev.Keycloak" as Keycloak {
    Service => Keycloak
    Priority => OPTIONAL
    Port => 8080
    Features => OAuth/SAML
}

@enduml
```

**View diagram:** Copy code above to https://www.plantuml.com/plantuml/uml/

---

## SQL Server 2019+ (REQUIRED)

### Purpose
- Database backend for all data projects
- SQL Server Native Client support
- SQLCLR integration (compiled SQL)
- Service Broker messaging
- DAC Framework deployment

### Container Image
```
mcr.microsoft.com/mssql/server:2019-latest
mcr.microsoft.com/mssql/server:2022-latest  (alternative)
```

### Ports
- **1433/TCP** - SQL Server (client connections)

### Environment Variables
```yaml
ACCEPT_EULA: Y                           # Required - agree to license
MSSQL_SA_PASSWORD: L0c@lD3v             # SA password
MSSQL_PID: Developer                     # Edition (Developer is free)
MSSQL_MEMORY_LIMIT_MB: 2048             # Memory limit
MSSQL_COLLATION: SQL_Latin1_General_CP1_CI_AS  # Collation
```

### Storage
**Note:** SQL Server uses ephemeral storage (no persistence required). Each test run starts with a fresh database.

```yaml
# Optional: For test initialization scripts
volumes:
  # Mount initialization SQL scripts (read-only)
  - ./services/sqlserver/init.sql:/docker-entrypoint-initdb.d/init.sql:ro
```

**Data Flow:**
- Container starts with empty SQL Server
- Initialization script creates databases and schema
- Tests import/create test data via SQL during test execution
- Container stopped → all data discarded (intentional)
- Next test run starts fresh

### Health Check
```yaml
healthcheck:
  test: [
    "CMD",
    "/opt/mssql-tools/bin/sqlcmd",
    "-S", "localhost",
    "-U", "sa",
    "-P", "L0c@lD3v",
    "-Q", "SELECT 1"
  ]
  interval: 10s
  timeout: 3s
  retries: 5
  start_period: 40s
```

### Initialization

**Database Schema (via init.sql or DacFx):**
```sql
CREATE DATABASE [VectorDb];
CREATE DATABASE [ExampleDb];
CREATE DATABASE [test_integration];

-- Enable Service Broker (required for messaging)
ALTER DATABASE [VectorDb] SET ENABLE_BROKER;
ALTER DATABASE [ExampleDb] SET ENABLE_BROKER;

-- Run migrations via OoBDev.DacFx DACPAC deployment
-- Creates tables, stored procedures, CLR functions, etc.
```

**Test Data Import Strategies:**

1. **SQL Scripts (Lightweight)**
   - Mount readonly script: `./services/sqlserver/init.sql:/docker-entrypoint-initdb.d/init.sql:ro`
   - Called during container startup
   - Best for: Schema creation, minimal seed data

2. **Test Fixtures (Recommended)**
   ```csharp
   public class SqlServerFixture : IAsyncLifetime
   {
       private SqlConnection _connection;

       public async Task InitializeAsync()
       {
           // Create/connect to database
           // Run migrations
           _connection = new SqlConnection(connectionString);
           await _connection.OpenAsync();

           // Import test data via SQL commands
           using var cmd = _connection.CreateCommand();
           cmd.CommandText = "INSERT INTO Users VALUES (...)";
           await cmd.ExecuteNonQueryAsync();
       }

       public async Task DisposeAsync()
       {
           // Cleanup: Drop databases
           await _connection.CloseAsync();
       }
   }
   ```
   - Called by each test class
   - Best for: Complex data, test isolation

3. **Import CSV/JSON Files**
   - Mount test data directory: `./test-data:/test-data:ro`
   - Tests read and insert data during setup
   - Best for: Large datasets, realistic scenarios
   ```csharp
   var csvData = File.ReadAllLines("/test-data/users.csv");
   foreach (var line in csvData)
   {
       await ImportUserRow(line);
   }
   ```

### Cleanup (Automatic)
```
On test run end:
- Container stopped via docker-compose down
- All databases, tables, data discarded
- Volumes removed (ephemeral)
- Next run starts with fresh SQL Server
```

**No manual cleanup required.**

### Resource Requirements
- **Disk:** 5-10 GB minimum
- **Memory:** 2-4 GB recommended
- **CPU:** 2 cores minimum
- **Startup Time:** 10-15 seconds

### Notes
- Developer edition is free for testing
- SA authentication is acceptable for testing
- Service Broker is required for messaging features

---

## RabbitMQ 3.12 (REQUIRED)

### Purpose
- AMQP message broker
- Message queue implementation
- Test message publishing/consuming
- Queue declaration and management

### Container Image
```
rabbitmq:3.12-management        # Includes management plugin
rabbitmq:3.12-management-alpine # Smaller image
```

### Ports
- **5672/TCP** - AMQP protocol (message broker)
- **15672/TCP** - Management HTTP API

### Environment Variables
```yaml
RABBITMQ_DEFAULT_USER: guest
RABBITMQ_DEFAULT_PASS: guest
RABBITMQ_DEFAULT_VHOST: /
```

### Storage
**Note:** RabbitMQ uses ephemeral storage (no persistence). Queues/exchanges are recreated from definitions.json on startup.

```yaml
volumes:
  # Configuration file (read-only)
  - ./services/rabbitmq/rabbitmq.conf:/etc/rabbitmq/rabbitmq.conf:ro

  # Definitions file: queues, exchanges, bindings (read-only)
  # RabbitMQ loads this on startup and recreates all definitions
  - ./services/rabbitmq/definitions.json:/etc/rabbitmq/definitions.json:ro
```

**Data Flow:**
- Container starts with empty RabbitMQ
- Definitions file loads and creates queues/exchanges
- Tests publish/consume messages during execution
- Container stopped → all queues/data discarded (intentional)
- Next run recreates everything from definitions.json

### Health Check
```yaml
healthcheck:
  test: [
    "CMD",
    "rabbitmq-diagnostics",
    "-q", "ping"
  ]
  interval: 10s
  timeout: 3s
  retries: 5
  start_period: 30s
```

### Initialization

**definitions.json Template:**
```json
{
  "version": 2,
  "vhosts": [
    {
      "name": "/"
    }
  ],
  "users": [
    {
      "name": "guest",
      "password": "guest",
      "tags": "administrator"
    }
  ],
  "permissions": [
    {
      "user": "guest",
      "vhost": "/",
      "configure": ".*",
      "write": ".*",
      "read": ".*"
    }
  ],
  "queues": [
    {
      "name": "test.queue.1",
      "vhost": "/",
      "durable": true,
      "arguments": {}
    },
    {
      "name": "test.queue.2",
      "vhost": "/",
      "durable": true,
      "arguments": {}
    }
  ],
  "exchanges": [
    {
      "name": "test.exchange",
      "vhost": "/",
      "type": "direct",
      "durable": true,
      "auto_delete": false
    }
  ],
  "bindings": [
    {
      "source": "test.exchange",
      "destination": "test.queue.1",
      "destination_type": "queue",
      "routing_key": "test.1"
    },
    {
      "source": "test.exchange",
      "destination": "test.queue.2",
      "destination_type": "queue",
      "routing_key": "test.2"
    }
  ]
}
```

### Test Data (Messages)

**Message Setup in Tests:**
```csharp
public class RabbitMqFixture : IAsyncLifetime
{
    private IConnection _connection;

    public async Task InitializeAsync()
    {
        var factory = new ConnectionFactory { HostName = "localhost" };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        // Verify queues/exchanges exist (created from definitions.json)
        // Publish test messages if needed
        var body = Encoding.UTF8.GetBytes("test message");
        _channel.BasicPublish(
            exchange: "test.exchange",
            routingKey: "test.1",
            basicProperties: null,
            body: body);
    }

    public async Task DisposeAsync()
    {
        _channel?.Close();
        _connection?.Close();
    }
}
```

### Cleanup (Automatic)
On container stop:
- All queues/messages discarded
- All exchanges removed
- Next run loads fresh definitions.json
- No manual cleanup required

### Resource Requirements
- **Disk:** 500 MB
- **Memory:** 512 MB minimum
- **CPU:** 1 core minimum
- **Startup Time:** 5-10 seconds

---

## MongoDB 7.0 (REQUIRED)

### Purpose
- NoSQL document database
- OoBDev.MongoDB tests
- Document storage validation
- Collection management testing

### Container Image
```
mongo:7.0
mongo:7.0-alpine  # Smaller image
```

### Ports
- **27017/TCP** - MongoDB default port

### Environment Variables
```yaml
MONGO_INITDB_ROOT_USERNAME: root
MONGO_INITDB_ROOT_PASSWORD: root
MONGO_INITDB_DATABASE: test_oodev
```

### Storage
**Note:** MongoDB uses ephemeral storage (no persistence). Collections are recreated from init.js on startup.

```yaml
volumes:
  # Initialization script (read-only)
  # MongoDB runs this to create collections, indices, etc.
  - ./services/mongodb/init.js:/docker-entrypoint-initdb.d/init.js:ro
```

**Data Flow:**
- Container starts with empty MongoDB
- init.js script creates collections and indices
- Tests insert/query documents during execution
- Container stopped → all data discarded (intentional)
- Next run executes init.js again

### Health Check
```yaml
healthcheck:
  test: [
    "CMD",
    "mongosh",
    "--eval",
    "db.adminCommand('ping')"
  ]
  interval: 10s
  timeout: 3s
  retries: 5
  start_period: 30s
```

### Initialization

**init.js Script:**
```javascript
// Switch to test database
db = db.getSiblingDB('test_oodev');

// Create collections
db.createCollection("documents");
db.createCollection("vectors");
db.createCollection("logs");

// Create indices
db.documents.createIndex({ "createdAt": 1 });
db.vectors.createIndex({ "embedding": "text" });
db.logs.createIndex({ "timestamp": 1 }, { expireAfterSeconds: 86400 });

// Insert sample data
db.documents.insertOne({
  "_id": ObjectId(),
  "name": "test_document",
  "content": "Test content",
  "createdAt": new Date()
});

print("MongoDB initialization complete");
```

### Test Data (Documents)

**Document Setup in Tests:**
```csharp
public class MongoDbFixture : IAsyncLifetime
{
    private IMongoClient _client;
    private IMongoDatabase _database;

    public async Task InitializeAsync()
    {
        _client = new MongoClient("mongodb://root:root@localhost:27017/test_oodev");
        _database = _client.GetDatabase("test_oodev");

        // Insert test documents
        var collection = _database.GetCollection<BsonDocument>("documents");
        var docs = new[]
        {
            new BsonDocument
            {
                { "name", "test_1" },
                { "content", "Test content 1" },
                { "createdAt", DateTime.UtcNow }
            }
        };
        await collection.InsertManyAsync(docs);
    }

    public async Task DisposeAsync()
    {
        // Connection closes automatically
    }
}
```

### Cleanup (Automatic)
On container stop:
- All databases/collections discarded
- All data removed
- Next run executes init.js again
- No manual cleanup required

### Resource Requirements
- **Disk:** 2-5 GB
- **Memory:** 512 MB minimum
- **CPU:** 1 core minimum
- **Startup Time:** 5-8 seconds

---

## Qdrant Vector Database (REQUIRED)

### Purpose
- Vector similarity search
- Embedding storage
- OoBDev.Qdrant tests
- AI/ML vector operations

### Container Image
```
qdrant/qdrant:latest
qdrant/qdrant:v1.7.0  # Pinned version
```

### Ports
- **6333/TCP** - REST API
- **6334/TCP** - gRPC API (optional)

### Environment Variables
```yaml
QDRANT_API_KEY: test_key_12345
```

### Storage
**Note:** Qdrant uses ephemeral storage (no persistence). Collections are recreated per test run.

```yaml
volumes:
  # Configuration (optional, read-only)
  - ./services/qdrant/config.yaml:/qdrant/config/config.yaml:ro
```

**Data Flow:**
- Container starts with empty Qdrant
- Tests create collections via REST API
- Tests insert vectors during execution
- Container stopped → all data discarded (intentional)
- Next run starts fresh

### Health Check
```yaml
healthcheck:
  test: [
    "CMD",
    "curl",
    "-f",
    "http://localhost:6333/health"
  ]
  interval: 10s
  timeout: 3s
  retries: 5
  start_period: 20s
```

### Test Data (Vectors)

**Collection & Vector Setup in Tests:**
```csharp
public class QdrantFixture : IAsyncLifetime
{
    private readonly HttpClient _client = new();

    public async Task InitializeAsync()
    {
        // Create test collection
        var createRequest = new
        {
            vectors = new { size = 384, distance = "Cosine" }
        };

        var response = await _client.PutAsync(
            "http://localhost:6333/collections/test_vectors",
            new StringContent(
                JsonConvert.SerializeObject(createRequest),
                Encoding.UTF8,
                "application/json"));

        response.EnsureSuccessStatusCode();

        // Insert test vectors
        var vectorData = new
        {
            points = new object[]
            {
                new {
                    id = 1,
                    vector = new float[384],  // Your embedding
                    payload = new { text = "test" }
                }
            }
        };

        await _client.PutAsync(
            "http://localhost:6333/collections/test_vectors/points",
            new StringContent(
                JsonConvert.SerializeObject(vectorData),
                Encoding.UTF8,
                "application/json"));
    }

    public async Task DisposeAsync() { }
}
```

### Cleanup (Automatic)
On container stop:
- All collections discarded
- All vectors removed
- Next run creates new collections
- No manual cleanup required

### Resource Requirements
- **Disk:** 1-5 GB (depends on vectors)
- **Memory:** 256 MB minimum
- **CPU:** 1 core minimum
- **Startup Time:** 3-5 seconds

---

## OpenSearch 2.0 (REQUIRED)

### Purpose
- Full-text search
- Semantic search
- OoBDev.OpenSearch tests
- Index management

### Container Image
```
opensearchproject/opensearch:2.0.0
opensearchproject/opensearch:latest
```

### Ports
- **9200/TCP** - REST API
- **9600/TCP** - Performance Analyzer

### Environment Variables
```yaml
discovery.type: single-node
OPENSEARCH_JAVA_OPTS: "-Xms512m -Xmx512m"
DISABLE_SECURITY_PLUGIN: "true"  # OK for testing
```

### Storage
**Note:** OpenSearch uses ephemeral storage (no persistence). Indices are recreated per test run.

```yaml
# No persistent volumes needed
```

**Data Flow:**
- Container starts with empty OpenSearch
- Tests create indices via REST API
- Tests index documents during execution
- Container stopped → all data discarded (intentional)
- Next run starts fresh

### Health Check
```yaml
healthcheck:
  test: [
    "CMD",
    "curl",
    "-f",
    "http://localhost:9200/_cluster/health"
  ]
  interval: 10s
  timeout: 3s
  retries: 5
  start_period: 30s
```

### Test Data (Documents)

**Index & Document Setup in Tests:**
```csharp
public class OpenSearchFixture : IAsyncLifetime
{
    private readonly HttpClient _client = new();

    public async Task InitializeAsync()
    {
        // Create test index
        var indexSettings = new
        {
            settings = new
            {
                index = new
                {
                    number_of_shards = 1,
                    number_of_replicas = 0
                }
            }
        };

        var response = await _client.PutAsync(
            "http://localhost:9200/test_index",
            new StringContent(
                JsonConvert.SerializeObject(indexSettings),
                Encoding.UTF8,
                "application/json"));

        response.EnsureSuccessStatusCode();

        // Insert test documents
        var doc = new { text = "test content", timestamp = DateTime.UtcNow };
        await _client.PostAsync(
            "http://localhost:9200/test_index/_doc",
            new StringContent(
                JsonConvert.SerializeObject(doc),
                Encoding.UTF8,
                "application/json"));
    }

    public async Task DisposeAsync() { }
}
```

### Cleanup (Automatic)
On container stop:
- All indices discarded
- All documents removed
- Next run creates new indices
- No manual cleanup required

### Resource Requirements
- **Disk:** 2-10 GB
- **Memory:** 1-2 GB recommended
- **CPU:** 2 cores recommended
- **Startup Time:** 8-12 seconds

---

## Ollama (OPTIONAL)

### Purpose
- Local LLM inference
- OoBDev.Ollama tests
- AI model evaluation

### Container Image
```
ollama/ollama:latest
```

### Ports
- **11434/TCP** - API port

### Storage
**Note:** Ollama uses ephemeral storage (no persistence). Models are downloaded per test run or cached temporarily.

```yaml
# Optional: Temporary cache (discarded when container stops)
volumes:
  # Volatile cache during test run (speeds up multiple tests)
  - ollama-cache:/root/.ollama
```

**Note:** On first run, model downloads (2-5 minutes for mistral). Subsequent runs in same container session reuse cached model. Container stops → cache discarded.

### Model Setup

**Pull Minimal Model (within test):**
```csharp
public class OllamaFixture : IAsyncLifetime
{
    private readonly HttpClient _client = new();

    public async Task InitializeAsync()
    {
        // Pull model if needed
        var pullRequest = new { name = "tinyllama" };
        var response = await _client.PostAsync(
            "http://localhost:11434/api/pull",
            new StringContent(
                JsonConvert.SerializeObject(pullRequest),
                Encoding.UTF8,
                "application/json"));

        response.EnsureSuccessStatusCode();
    }

    public async Task DisposeAsync() { }
}
```

**Available Models for Testing:**
- `tinyllama` (~400MB) - Smallest, fastest for testing
- `neural-chat:7b` (~4GB) - Balanced
- `mistral` (~4GB) - Good quality

### Health Check
```yaml
healthcheck:
  test: [
    "CMD",
    "curl",
    "-f",
    "http://localhost:11434/api/tags"
  ]
  interval: 10s
  timeout: 3s
  retries: 5
  start_period: 60s
```

### Resource Requirements
- **Disk:** 4-20 GB (model dependent)
- **Memory:** 4-8 GB (model dependent)
- **CPU:** Multi-core strongly recommended
- **Startup Time:** 2-3 seconds (model load: minutes)

### Note
Ollama is optional due to high resource usage. Consider skipping in CI or using smaller models (tinyllama).

---

## Container Composition Matrix

Which containers to run for different test scenarios:

```plantuml
@startuml ContainerCompositionMatrix
skinparam componentStyle rectangle

card "Minimal Setup" as MINIMAL {
  [SQL Server]
  [RabbitMQ]
}

card "Standard Setup" as STANDARD {
  [SQL Server]
  [RabbitMQ]
  [MongoDB]
  [Qdrant]
  [OpenSearch]
}

card "Full Setup" as FULL {
  [SQL Server]
  [RabbitMQ]
  [MongoDB]
  [Qdrant]
  [OpenSearch]
  [Ollama]
  [Keycloak]
}

note bottom of MINIMAL
  Fastest startup (~30s)
  Tests: SQL, messaging
  Good for: Quick PR checks
end note

note bottom of STANDARD
  Standard setup (~50s)
  Tests: Data, search, vectors
  Good for: Daily releases
end note

note bottom of FULL
  Complete setup (~100s)
  All features tested
  Good for: Release validation
end note

@enduml
```

**View diagram:** Copy code above to https://www.plantuml.com/plantuml/uml/

---

## Container Startup Optimization

```plantuml
@startuml StartupOptimization
participant "Docker" as D
participant "Services" as S
participant "Health" as H

D ->> D: Start all containers (parallel)

par
  D ->> S: SQL Server
  activate S
  S ->> S: Initialize DB
  S -->> H: Ready (10-15s)
  deactivate S
else
  D ->> S: RabbitMQ
  activate S
  S ->> S: Start broker
  S -->> H: Ready (5-10s)
  deactivate S
else
  D ->> S: MongoDB
  activate S
  S ->> S: Init schema
  S -->> H: Ready (5-8s)
  deactivate S
else
  D ->> S: Qdrant
  activate S
  S ->> S: Start service
  S -->> H: Ready (3-5s)
  deactivate S
else
  D ->> S: OpenSearch
  activate S
  S ->> S: Cluster init
  S -->> H: Ready (8-12s)
  deactivate S
end

H ->> D: All services ready
D -->> D: Start test execution

@enduml
```

**View diagram:** Copy code above to https://www.plantuml.com/plantuml/uml/

---

## Memory & CPU Limits

Recommended resource constraints:

```yaml
services:
  sqlserver:
    deploy:
      resources:
        limits:
          memory: 3G
          cpus: '2.0'
        reservations:
          memory: 2G
          cpus: '1.5'

  rabbitmq:
    deploy:
      resources:
        limits:
          memory: 512M
          cpus: '1.0'
        reservations:
          memory: 256M
          cpus: '0.5'

  mongodb:
    deploy:
      resources:
        limits:
          memory: 1G
          cpus: '1.0'
        reservations:
          memory: 512M
          cpus: '0.5'

  qdrant:
    deploy:
      resources:
        limits:
          memory: 2G
          cpus: '2.0'
        reservations:
          memory: 512M
          cpus: '0.5'

  opensearch:
    deploy:
      resources:
        limits:
          memory: 2G
          cpus: '2.0'
        reservations:
          memory: 1G
          cpus: '1.0'
```

**Total Resources:**
- **Memory:** ~10 GB (can reduce with constraints)
- **CPU:** ~8 cores (parallel startup)
- **Disk:** ~10-20 GB

---

## Next Steps

When you implement:

1. Start with SQL Server + RabbitMQ (most critical)
2. Add MongoDB for document tests
3. Add Qdrant + OpenSearch for search
4. Evaluate Ollama (optional, resource-heavy)

Each can be added incrementally without breaking existing tests.
