# Services

[← Docker infrastructure](docker-infrastructure.md) · [Writing tests →](writing-tests.md)

One section per stack. Values are the test defaults from `src/.runsettings`; they are throwaway credentials for local containers only. Test property names are the keys read with `TestContext.GetRequiredProperty<T>()`, and the complete list is in [TEST_VARIABLES.md](../../../../TEST_VARIABLES.md).

## Contents

- [SQL Server](#sql-server)
- [MongoDB](#mongodb)
- [Redis](#redis)
- [PostgreSQL](#postgresql)
- [RabbitMQ](#rabbitmq)
- [DevTerm peers](#devterm-peers-mosquitto-ser2net-loopback-device)
- [OpenSearch](#opensearch)
- [Qdrant](#qdrant)
- [Apache Tika](#apache-tika)
- [smtp4dev](#smtp4dev)
- [Azurite](#azurite)
- [Moto](#moto)
- [Service Bus emulator](#service-bus-emulator)
- [Keycloak](#keycloak)
- [SBert](#sbert)
- [Ollama](#ollama)
- [Aspire dashboard](#aspire-dashboard)
- [Dashboards](#dashboards)

**Table 1 — Summary**

| Service | Image | Tests connect to | Credentials |
|---------|-------|------------------|-------------|
| SQL Server | `mssql/server:2022-latest` | `localhost,1433` | `sa` / `IntegrationTest123!` |
| MongoDB | `mongo:latest` | `mongodb://localhost:27017` | none |
| Redis | `valkey/valkey:8-alpine` | `localhost:6379` | none |
| PostgreSQL | `ghcr.io/pgmq/pg17-pgmq:v1.8.0` | `localhost:5434` | `postgres` / `IntegrationTest123!` |
| RabbitMQ | `rabbitmq:management` | `amqp://localhost:5673` | `guest` / `guest` |
| OpenSearch | `opensearchproject/opensearch` | `https://localhost:9200` | `admin` / `IntegrationTest123!` |
| Qdrant | `qdrant/qdrant` | `http://localhost:6333` | none |
| Apache Tika | `apache/tika` | `http://localhost:9998` | none |
| smtp4dev | `rnwood/smtp4dev` | SMTP 25, IMAP 143, UI 7777 | none |
| Azurite | `azure-storage/azurite` | `http://127.0.0.1:10000/devstoreaccount1` | well-known dev account key |
| Moto | `motoserver/moto` | `http://localhost:4566` | `test` / `test` |
| Service Bus emulator | `azure-messaging/servicebus-emulator` | `sb://localhost:5672` | emulator SAS key |
| Keycloak | `keycloak/keycloak` | `http://localhost:8081` | `admin` / `admin` |
| SBert | built from `containers/` | `http://localhost:5080` | none |
| Ollama | `oobdev/ollama-phi3` (built) | `http://localhost:11435` | none |
| Aspire dashboard | `mcr.microsoft.com/dotnet/aspire-dashboard` | OTLP `http://localhost:4318`, UI `http://localhost:18888` | API key header for the telemetry API |

## SQL Server

Developer edition. Used for SQL vector and matrix tests (`OoBDev.Data.Vectors.DB.Tests`) and as the backing store of the Service Bus emulator. Data volume `sqlserver-test-data`. Keys: `SQLSERVER_CONNECTION_STRING`, `SQLSERVER_HOST`, `SQLSERVER_PORT`, `SQLSERVER_USERNAME`, `SQLSERVER_PASSWORD`. The password is also the compose variable `SQL_SA_PASSWORD`.

## MongoDB

Document store tests. Keys: `MONGODB_CONNECTION_STRING`, `MONGODB_DATABASE_NAME`. Tests create a database named `IntegrationTest_<guid>` and drop it in cleanup.

## PostgreSQL

PostgreSQL 17 with the pgmq extension created at first start (`containers/testing/postgres/01-pgmq.sql`), database `oobdev_test`, ephemeral volume. Host port 5434 because 5432 and 5433 are common local installs. Used by the event sourcing Postgres provider tests and the pgmq queue adapter. Key: `POSTGRES_CONNECTION_STRING`.

## Redis

256 MB cap, `allkeys-lru`, no persistence. Used by the caching provider tests. Keys: `REDIS_CONNECTION_STRING`, `REDIS_HOST`, `REDIS_PORT`.

## RabbitMQ

AMQP is published on 5673 so the Service Bus emulator can keep 5672; the management UI is on 15672. Keys: `RABBITMQ_HOST`, `RABBITMQ_PORT`, `RABBITMQ_AMQP_PORT`, `RABBITMQ_MANAGEMENT_PORT`, `RABBITMQ_USERNAME`, `RABBITMQ_PASSWORD`, `RABBITMQ_CONNECTION_STRING`.

## DevTerm peers (Mosquitto, ser2net, loopback-device)

Disposable peers for the DevTerm transports, configured under `containers/testing/devterm/`. Mosquitto is anonymous MQTT on 1883; ser2net serves RFC 2217 on 2217 with a port behind it that echoes every byte; loopback-device is a TCP line device on 7778. RabbitMQ also publishes STOMP (21613, because 61613 sits in a Windows reserved range), AMQPS (5671) and STOMP over TLS (21614); the TLS listeners need `sh containers/testing/devterm/make-test-certs.sh` first (git-ignored `devterm/certs/ca.pem`). Keys: `MOSQUITTO_HOST`, `MOSQUITTO_PORT`, `SER2NET_HOST`, `SER2NET_PORT`, `LOOPBACK_DEVICE_HOST`, `LOOPBACK_DEVICE_PORT`, `RABBITMQ_STOMP_PORT`, `RABBITMQ_STOMP_TLS_PORT`.

## OpenSearch

Security plugin on, self-signed certificate (tests use `OPENSEARCH_USE_HTTPS=true` and accept the certificate). Needs `vm.max_map_count` of at least 262144 on Linux hosts. Keys: `OPENSEARCH_URL`, `OPENSEARCH_USERNAME`, `OPENSEARCH_PASSWORD`.

## Qdrant

Vector database over the query API. Keys: `QDRANT_URL`, `QDRANT_HOST`, `QDRANT_PORT`. Storage and snapshot volumes persist between runs, so tests use unique collection names.

## Apache Tika

Stateless document detection and extraction. Keys: `TIKA_URL`, `TIKA_HOST`, `TIKA_PORT`.

## smtp4dev

Captures outgoing mail and serves it over IMAP; the web UI on 7777 shows messages. Keys: `SMTP_HOST`, `SMTP_PORT`, `SMTP_URL`, `IMAP_HOST`, `IMAP_PORT`.

## Azurite

Azure Storage emulator (blob 10000, queue 10001, table 10002) with the standard development account `devstoreaccount1`. Keys: `AZURITE_URL`, `AZURITE_BLOB_SERVICE_URL`, `AZURITE_CONNECTION_STRING`. No test project uses it yet.

## Moto

AWS emulator (SQS and others) on 4566. The one-shot `moto-init` container creates the test queues from `moto-init/`, so tests can rely on `integration-test-queue`. Keys: `MOTO_URL`, `SQS_ENDPOINT`, `SQS_TEST_QUEUE`, `AWS_REGION`, `AWS_ACCESS_KEY_ID`, `AWS_SECRET_ACCESS_KEY`. Moto replaced LocalStack.

## Service Bus emulator

Starts after SQL Server and reads its queue and topic definition from `servicebus-config/Config.json`. It has no health check, so the scripts wait on its port. Keys: `SERVICEBUS_CONNECTION_STRING`, `SERVICEBUS_HOST`, `SERVICEBUS_PORT`, `SERVICEBUS_TEST_QUEUE`, `SERVICEBUS_TEST_TOPIC`.

## Keycloak

Imports `keycloak-config/integration-test-realm.json` on start: realm `integration-test`, client `integration-test-client`, user `testuser`. Keys: `KEYCLOAK_URL`, `KEYCLOAK_REALM`, `KEYCLOAK_CLIENT_ID`, `KEYCLOAK_CLIENT_SECRET`, `KEYCLOAK_ADMIN_USERNAME`, `KEYCLOAK_ADMIN_PASSWORD`, `KEYCLOAK_TEST_USERNAME`, `KEYCLOAK_TEST_PASSWORD`. Details in [KEYCLOAK-TESTING.md](../../../../containers/testing/KEYCLOAK-TESTING.md).

## SBert

Python sentence-embedding service used by the SBert client tests and as the reference for the in-process ONNX embedder. Keys: `SBERT_URL`, `SBERT_HOST`, `SBERT_PORT`. The model project must be built before the AllMiniLm tests.

## Ollama

CPU-only inference with `phi3` baked into the image at build time, so first start does not download 2 GB. Published on 11435 to leave a local Ollama on 11434 alone. Keys: `OLLAMA_URL`, `OLLAMA_HOST`, `OLLAMA_PORT`, `OLLAMA_MODEL`, `OLLAMA_EMBEDDING_MODEL`, `OLLAMA_VISION_MODEL`.

## Aspire dashboard

One container with an OTLP receiver (4317 gRPC, 4318 HTTP) and the Aspire dashboard UI on 18888 (unsecured). The OpenTelemetry tests export spans and logs to OTLP and read them back through the dashboard telemetry API (`/api/telemetry/spans`, `/api/telemetry/logs`), which needs the `x-api-key` header. Keys: `OTEL_EXPORTER_OTLP_ENDPOINT`, `OTEL_DASHBOARD_URL`, `OTEL_DASHBOARD_API_KEY`.

## Dashboards

nginx on 8080 is a landing page that links the web UIs (smtp4dev, RabbitMQ, Keycloak, OpenSearch Dashboards, Qdrant) and exposes `/health`. OpenSearch Dashboards on 5601 is served under `/opensearch-dashboards`. See [NGINX-DASHBOARD.md](../../../../containers/testing/NGINX-DASHBOARD.md). Keys: `NGINX_URL`, `OPENSEARCH_DASHBOARDS_URL`.

[← Docker infrastructure](docker-infrastructure.md) · [Writing tests →](writing-tests.md)
