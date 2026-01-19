# Integration Testing Infrastructure - Local Validation Checklist

**Status**: 🟡 Awaiting Local Testing
**Last Updated**: 2026-01-19

---

## Prerequisites Verification

Before starting Docker services, verify:

- [ ] **Docker Desktop** (Windows/Mac) or **Docker Engine** (Linux) is installed and running
  ```bash
  docker version
  docker compose version
  ```

- [ ] **Required ports are available**:
  ```bash
  # Check if ports are already in use
  netstat -an | grep -E "1433|5672|6333|8081|9200|9998|10000|27017"
  # On Windows: netstat -an | findstr "1433 5672 6333 8081 9200 9998 10000 27017"
  ```

- [ ] **Sufficient disk space** (at least 10GB free for Docker images/volumes)
  ```bash
  df -h  # Linux/macOS
  # On Windows: Check drive properties
  ```

---

## Local Testing Procedure

### Step 1: Start Services

**Linux/macOS**:
```bash
cd containers/testing
./scripts/integration-up.sh --wait
```

**Windows**:
```cmd
cd containers\testing
scripts\integration-up.bat --wait
```

**Expected Output**:
```
======================================================================
OoBDev Integration Test Stack - Starting Services
======================================================================

Starting Docker containers...
✅ Docker services started successfully

Waiting for services to become healthy...
✅ All services are healthy and ready for testing!

Services running:
  - Apache Tika:        http://localhost:9998
  - SMTP4Dev:           http://localhost:7777
  - MongoDB:            mongodb://localhost:27017
  - SQL Server:         localhost,1433 (sa/IntegrationTest123!)
  - RabbitMQ:           amqp://localhost:5672, http://localhost:15672
  - OpenSearch:         https://localhost:9200 (admin/IntegrationTest123!)
  - Qdrant:             http://localhost:6333
  - Azurite:            http://localhost:10000 (Blob), 10001 (Queue), 10002 (Table)
  - LocalStack:         http://localhost:4566
  - Keycloak:           http://localhost:8081 (admin/admin)
  - SBert:              http://localhost:5080
```

**Checklist**:
- [ ] Script completes without errors
- [ ] All 11 services show as "healthy"
- [ ] Wait time is under 2 minutes
- [ ] No port conflict errors

---

### Step 2: Verify Service Health

Test each service is responding:

#### Apache Tika (Document Processing)
```bash
curl -f http://localhost:9998/tika
# Expected: Tika version info
```
- [ ] ✅ Tika responds

#### SMTP4Dev (Email Testing)
```bash
curl -f http://localhost:7777
# Expected: HTML content (SMTP4Dev web interface)
```
- [ ] ✅ SMTP4Dev web UI accessible

#### MongoDB
```bash
mongosh --eval "db.adminCommand('ping')"
# Expected: { ok: 1 }
```
**If mongosh not installed**, skip this - tests will verify.
- [ ] ✅ MongoDB responds (or skipped if no mongosh)

#### SQL Server
```bash
# Linux/macOS (if sqlcmd installed):
sqlcmd -S localhost,1433 -U sa -P 'IntegrationTest123!' -Q "SELECT @@VERSION"

# Windows (if sqlcmd installed):
sqlcmd -S localhost,1433 -U sa -P "IntegrationTest123!" -Q "SELECT @@VERSION"
```
**If sqlcmd not installed**, skip this - tests will verify.
- [ ] ✅ SQL Server responds (or skipped if no sqlcmd)

#### RabbitMQ Management UI
```bash
# Open in browser:
open http://localhost:15672  # macOS
start http://localhost:15672 # Windows
xdg-open http://localhost:15672 # Linux
# Login: guest/guest
```
- [ ] ✅ RabbitMQ management UI accessible

#### OpenSearch
```bash
curl -k -u admin:IntegrationTest123! https://localhost:9200/_cluster/health
# Expected: JSON with cluster health
```
- [ ] ✅ OpenSearch responds

#### Qdrant
```bash
curl http://localhost:6333/health
# Expected: {"title":"qdrant - vector search engine","version":"..."}
```
- [ ] ✅ Qdrant responds

#### Azurite (Azure Storage Emulator)
```bash
curl http://localhost:10000/devstoreaccount1?comp=list
# Expected: XML response (blob list)
```
- [ ] ✅ Azurite Blob service responds

#### LocalStack (AWS Emulator)
```bash
curl http://localhost:4566/_localstack/health
# Expected: JSON with service status
```
- [ ] ✅ LocalStack responds

#### Keycloak
```bash
curl http://localhost:8081/health/ready
# Expected: {"status":"UP"}
```
- [ ] ✅ Keycloak responds

#### SBert (Sentence Embeddings)
```bash
curl http://localhost:5080/health
# Expected: {"status":"healthy"}
```
- [ ] ✅ SBert responds

---

### Step 3: Check Container Status

```bash
cd containers/testing
docker compose -f docker-compose.integration-tests.yml ps
```

**Expected Output**: All containers show "Up" and "healthy"

```
NAME                    STATUS                   PORTS
oobd-test-tika          Up (healthy)             0.0.0.0:9998->9998/tcp
oobd-test-smtp          Up (healthy)             0.0.0.0:25->25/tcp, 0.0.0.0:7777->80/tcp
oobd-test-mongodb       Up (healthy)             0.0.0.0:27017->27017/tcp
oobd-test-sqlserver     Up (healthy)             0.0.0.0:1433->1433/tcp
oobd-test-rabbitmq      Up (healthy)             0.0.0.0:5672->5672/tcp, 0.0.0.0:15672->15672/tcp
oobd-test-opensearch    Up (healthy)             0.0.0.0:9200->9200/tcp, 0.0.0.0:9600->9600/tcp
oobd-test-qdrant        Up (healthy)             0.0.0.0:6333->6333/tcp, 0.0.0.0:6334->6334/tcp
oobd-test-azurite       Up (healthy)             0.0.0.0:10000-10002->10000-10002/tcp
oobd-test-localstack    Up (healthy)             0.0.0.0:4566->4566/tcp
oobd-test-keycloak      Up (healthy)             0.0.0.0:8081->8080/tcp
oobd-test-sbert         Up (healthy)             0.0.0.0:5080->5000/tcp
```

- [ ] ✅ All 11 containers show "Up"
- [ ] ✅ All 11 containers show "(healthy)"

---

### Step 4: Test Cleanup

Stop and remove all test data:

**Linux/macOS**:
```bash
./scripts/integration-down.sh --clean
```

**Windows**:
```cmd
scripts\integration-down.bat --clean
```

**Expected Output**:
```
======================================================================
OoBDev Integration Test Stack - Stopping Services
======================================================================

Mode: CLEAN (will remove volumes and test data)

Stopping Docker containers...

✅ Docker services stopped and volumes removed

All test data has been cleaned up:
  - MongoDB databases deleted
  - SQL Server databases deleted
  - RabbitMQ queues deleted
  - OpenSearch indices deleted
  - Qdrant collections deleted
  - Azurite blobs/queues/tables deleted
  - Keycloak data deleted
```

**Verify cleanup**:
```bash
docker compose -f docker-compose.integration-tests.yml ps
# Expected: No containers running

docker volume ls | grep oobd-test
# Expected: No volumes listed
```

- [ ] ✅ All containers stopped
- [ ] ✅ All volumes removed
- [ ] ✅ Script completes without errors

---

### Step 5: Restart Test (Clean State Verification)

Start services again to verify they work from clean state:

```bash
# Start services
./scripts/integration-up.sh --wait  # or integration-up.bat --wait on Windows

# Verify health (should be same as Step 1)
# ...

# Stop and cleanup
./scripts/integration-down.sh --clean  # or integration-down.bat --clean on Windows
```

- [ ] ✅ Services start successfully on second run
- [ ] ✅ All services become healthy
- [ ] ✅ Cleanup works correctly

---

## Troubleshooting Common Issues

### Issue: Port Already in Use

**Symptom**: Error like `bind: address already in use`

**Solution**:
```bash
# Find process using the port (example for port 1433)
lsof -i :1433  # Linux/macOS
netstat -ano | findstr :1433  # Windows

# Stop the conflicting process or change port mapping in docker-compose file
```

### Issue: Services Don't Become Healthy

**Symptom**: Timeout after 120 seconds

**Solution**:
```bash
# View logs for failing service
docker compose -f docker-compose.integration-tests.yml logs [service-name]

# Common issues:
# - SQL Server: Increase wait time for first-time startup
# - OpenSearch: vm.max_map_count too low on Linux (see README.md)
# - Keycloak: Slow first startup (database initialization)

# Try with longer timeout
./scripts/wait-for-services.sh 300  # Wait 5 minutes
```

### Issue: Docker Not Running

**Symptom**: `Cannot connect to the Docker daemon`

**Solution**:
```bash
# Start Docker Desktop (Windows/Mac)
# Or start Docker service (Linux):
sudo systemctl start docker
```

### Issue: Disk Space

**Symptom**: `no space left on device`

**Solution**:
```bash
# Remove unused Docker resources
docker system prune -a --volumes

# Check disk usage
docker system df
```

---

## After Successful Testing

Once all checklist items are complete:

### 1. Update TODO.md
```markdown
- [x] Week 1: Docker infrastructure setup
- [x] Test Docker stack locally (all 11 services healthy)
- [ ] Week 2: Migrate tests to Integration category
```

### 2. Enable GitHub Actions Workflow

Edit `.github/workflows/integration-tests.yml`:

```yaml
on:
  # Uncomment after successful local testing:
  schedule:
    - cron: '0 16 * * *'  # Daily at 4 PM UTC

  workflow_dispatch:  # Manual trigger
```

### 3. Commit and Push

```bash
git add .github/workflows/integration-tests.yml
git commit -m "Enable integration tests workflow after local validation"
git push
```

### 4. Verify CI/CD

Manually trigger the workflow in GitHub Actions to verify it works in CI/CD environment.

---

## Test Results Summary

**Date Tested**: _____________

**Tester**: _____________

**Platform**: □ Linux  □ macOS  □ Windows

**Results**:
- [ ] All 11 services start successfully
- [ ] All health checks pass within 2 minutes
- [ ] Manual service verification passes (or skipped)
- [ ] Cleanup works correctly
- [ ] Restart from clean state works

**Issues Encountered**: _____________________________________________

**Notes**: _____________________________________________

---

**Next Steps**: Proceed to Week 2 (Test Migration) in the implementation plan.
