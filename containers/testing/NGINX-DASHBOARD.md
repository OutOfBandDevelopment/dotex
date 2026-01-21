# Nginx Reverse Proxy Dashboard

This directory contains an nginx reverse proxy configuration that provides unified access to all integration test services through a single dashboard.

## Quick Access

Once the integration test stack is running, access the dashboard at:

**http://localhost:8080**

## Features

### 🖥️ Unified Web Interface

The dashboard provides a single entry point to all service web UIs:

- **SMTP4Dev** - `/smtp4dev/` - Email testing interface
- **RabbitMQ** - `/rabbitmq/` - Message queue management console
- **Keycloak** - `/keycloak/` - Identity & Access Management admin console
- **Qdrant** - `/qdrant/` - Vector database dashboard
- **OpenSearch Dashboards** - `/opensearch-dashboards/` - Data visualization

### 📊 Service Dashboard

The root page (`/`) displays:

- **Service Status**: All 14 services with their status
- **Web UIs**: Direct links to all web interfaces
- **API Endpoints**: Proxied access to REST APIs
- **Port Information**: All exposed ports for each service
- **Quick Commands**: Copy-paste commands for common tasks
- **Statistics**: Live service counts

### 🔌 Proxied API Endpoints

Direct API access through the reverse proxy:

| Service | Proxy Path | Direct Port | Description |
|---------|-----------|-------------|-------------|
| Apache Tika | `/tika/` | 9998 | Document parsing API |
| OpenSearch | `/opensearch-api/` | 9200 | Search engine API |
| Qdrant | `/qdrant-api/` | 6333 | Vector database API |
| LocalStack | `/localstack/` | 4566 | AWS services emulator |
| SBert | `/sbert/` | 5000 | ML embeddings API |

## Usage

### Start the Stack with Dashboard

```bash
cd /current/src/containers/testing
./scripts/integration-up.sh --wait
```

The dashboard will be available at `http://localhost:8080`

### Access Individual Services

**Via Dashboard (Recommended):**
```bash
# Open dashboard in browser
open http://localhost:8080

# Click on any service card to access its web UI
```

**Via Reverse Proxy:**
```bash
# SMTP4Dev email interface
open http://localhost:8080/smtp4dev/

# RabbitMQ management console (guest/guest)
open http://localhost:8080/rabbitmq/

# Keycloak admin console (admin/admin)
open http://localhost:8080/keycloak/

# Qdrant dashboard
open http://localhost:8080/qdrant/

# OpenSearch Dashboards (admin/IntegrationTest123!)
open http://localhost:8080/opensearch-dashboards/
```

**Via Direct Ports:**
```bash
# Still works - bypass the proxy
open http://localhost:7777   # SMTP4Dev
open http://localhost:15672  # RabbitMQ
open http://localhost:8081   # Keycloak
open http://localhost:6333   # Qdrant
open http://localhost:5601   # OpenSearch Dashboards
```

### Test API Endpoints Through Proxy

```bash
# Apache Tika - Parse document
curl -T document.pdf http://localhost:8080/tika/

# OpenSearch - Cluster health
curl http://localhost:8080/opensearch-api/_cluster/health

# Qdrant - List collections
curl http://localhost:8080/qdrant-api/collections

# LocalStack - List S3 buckets
aws --endpoint-url=http://localhost:8080/localstack s3 ls

# SBert - Health check
curl http://localhost:8080/sbert/health
```

## Configuration

### Nginx Configuration

**Location:** `nginx/nginx.conf`

The configuration includes:
- Reverse proxy rules for all services
- GZIP compression
- Request buffering
- WebSocket support (for OpenSearch Dashboards)
- Custom headers for proxy forwarding
- 100MB max upload size
- Docker DNS resolver (allows nginx to start before all upstreams are ready)

### Dashboard HTML

**Location:** `nginx/html/index.html`

The dashboard is a static HTML page with:
- Responsive grid layout
- Service cards with status badges
- Direct links to all UIs
- Port and connection information
- Quick command reference
- Live statistics

### Customization

**Add a new service to the dashboard:**

1. Edit `nginx/nginx.conf` and add a location block:
```nginx
location /myservice/ {
    proxy_pass http://myservice:port/;
    proxy_set_header Host $host;
    proxy_set_header X-Real-IP $remote_addr;
    proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
}
```

2. Edit `nginx/html/index.html` and add a service card:
```html
<div class="service-card">
    <div class="service-header">
        <span class="service-name">🎯 My Service</span>
        <span class="service-status status-running">Web UI</span>
    </div>
    <div class="service-description">
        Description of your service
    </div>
    <div class="service-links">
        <a href="/myservice/" class="service-link" target="_blank">Open Dashboard</a>
    </div>
    <div class="port-info">
        <div class="port-item">Port: <span class="port-number">localhost:9999</span></div>
    </div>
</div>
```

3. Restart nginx:
```bash
docker restart oobd-test-nginx
```

## Troubleshooting

### Dashboard not loading

**Check nginx is running:**
```bash
docker ps | grep oobd-test-nginx
```

**Check nginx logs:**
```bash
docker logs oobd-test-nginx
```

**Verify port 8080 is available:**
```bash
lsof -i :8080  # macOS/Linux
netstat -ano | findstr :8080  # Windows
```

### Nginx fails to start with "host not found in upstream"

**Issue:** Nginx cannot resolve upstream service names at startup

**Error:**
```
nginx: [emerg] host not found in upstream "rabbitmq" in /etc/nginx/nginx.conf:57
```

**Solution:** This is already fixed in the configuration using Docker's DNS resolver:
```nginx
# Docker DNS resolver (allows nginx to start even if upstreams aren't ready)
resolver 127.0.0.11 valid=10s;
resolver_timeout 5s;
```

If you still see this error, ensure you're using the latest nginx.conf configuration.

### Service links return 502 Bad Gateway

**Issue:** Target service is not running or not healthy

**Solution:** Check service health
```bash
docker ps --format "table {{.Names}}\t{{.Status}}"
```

Wait for all services to become healthy:
```bash
./scripts/integration-up.sh --wait
```

### Proxy path issues

**Issue:** Service doesn't work correctly behind `/path/` prefix

Some services require additional configuration to work behind a reverse proxy path.

**Solutions:**

1. **RabbitMQ**: Uses rewrite rules to strip `/rabbitmq/` prefix
2. **Keycloak**: Requires `X-Forwarded-Host` header
3. **OpenSearch Dashboards**: Needs WebSocket support (already configured)

If a service doesn't work behind proxy, use direct port access temporarily.

### CORS errors

**Issue:** Browser blocks API calls due to CORS

**Solution:** Add CORS headers to nginx.conf:
```nginx
location /myservice/ {
    # ... existing proxy_pass ...

    add_header Access-Control-Allow-Origin "*" always;
    add_header Access-Control-Allow-Methods "GET, POST, PUT, DELETE, OPTIONS" always;
    add_header Access-Control-Allow-Headers "Authorization, Content-Type" always;

    if ($request_method = 'OPTIONS') {
        return 204;
    }
}
```

## Architecture

```
┌─────────────┐
│   Browser   │
└──────┬──────┘
       │ http://localhost:8080
       ▼
┌─────────────────────┐
│  Nginx (Port 8080)  │
│  - Dashboard (/)    │
│  - Reverse Proxy    │
└──────┬──────────────┘
       │
       ├──────────────► /smtp4dev/        → smtp4dev:80
       ├──────────────► /rabbitmq/        → rabbitmq:15672
       ├──────────────► /keycloak/        → keycloak:8080
       ├──────────────► /qdrant/          → qdrant:6333
       ├──────────────► /opensearch-dashboards/ → opensearch-dashboards:5601
       ├──────────────► /tika/            → apache-tika:9998
       ├──────────────► /opensearch-api/  → opensearch:9200
       ├──────────────► /qdrant-api/      → qdrant:6333
       ├──────────────► /localstack/      → localstack:4566
       └──────────────► /sbert/           → sbert:5000
```

## Benefits

✅ **Single Entry Point**: One URL for all services
✅ **No Port Memorization**: Use service names instead of ports
✅ **Visual Dashboard**: See all services at a glance
✅ **Quick Access**: One-click access to all UIs
✅ **Centralized Logging**: All requests logged through nginx
✅ **Easy Testing**: Simplified API endpoint access
✅ **Documentation**: Built-in service documentation

## See Also

- [Integration Test Infrastructure](./README.md) - Overall testing setup
- [Service Configuration](./docker-compose.integration-tests.yml) - Docker compose configuration
- [Testing Checklist](./TESTING-CHECKLIST.md) - Local validation procedure
