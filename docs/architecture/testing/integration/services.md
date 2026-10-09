# Services

[← Docker infrastructure](docker-infrastructure.md) · [Writing tests →](writing-tests.md)

One section per stack. Values are the test defaults from `src/.runsettings`; they are throwaway credentials for local containers only. Test property names are the keys read with `TestContext.GetRequiredProperty<T>()`, and the complete list is in [TEST_VARIABLES.md](../../../../TEST_VARIABLES.md).

## Contents

- [SQL Server](#sql-server)
- [MongoDB](#mongodb)
- [Redis](#redis)
- [RabbitMQ](#rabbitmq)
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
- [Grafana LGTM](#grafana-lgtm)
- [Dashboards](#dashboards)

**Table 1 — Summary**

| Service | Image | Tests connect to | Credentials |
|---------|-------|------------------|-------------|
| SQL Server | `mssql/server:2022-latest` | `localhost,1433` | `sa` / `IntegrationTest123!` |
| MongoDB | `mongo:latest` | `mongodb://localhost:27017` | none |
| Redis | `redis:7-alpine` | `localhost:6379` | none |
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
| Grafana LGTM | `grafana/otel-lgtm` | OTLP `http://localhost:4318`, UI `http://localhost:3000` | none |

## SQL Server

Developer edition. Used for SQL vector and matrix tests (`OoBDev.Data.Vectors.DB.Tests`) and as the backing store of the Service Bus emulator. Data volume `sqlserver-test-data`. Keys: `SQLSERVER_CONNECTION_STRING`, `SQLSERVER_HOST`, `SQLSERVER_PORT`, `SQLSERVER_USERNAME`, `SQLSERVER_PASSWORD`. The password is also the compose variable `SQL_SA_PASSWORD`.

## MongoDB

Document store tests. Keys: `MONGODB_CONNECTION_STRING`, `MONGODB_DATABASE_NAME`. Tests create a database named `IntegrationTest_<guid>` and drop it in cleanup.

## Redis

256 MB cap, `allkeys-lru`, no persistence. Used by the caching provider tests. Keys: `REDIS_CONNECTION_STRING`, `REDIS_HOST`, `REDIS_PORT`.

## RabbitMQ

AMQP is published on 5673 so the Service Bus emulator can keep 5672; the management UI is on 15672. Keys: `RABBITMQ_HOST`, `RABBITMQ_PORT`, `RABBITMQ_AMQP_PORT`, `RABBITMQ_MANAGEMENT_PORT`, `RABBITMQ_USERNAME`, `RABBITMQ_PASSWORD`, `RABBITMQ_CONNECTION_STRING`.

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

CPU-only inference with `phi3` baked into the image at build time, so first start does not download 2 GB. Published on 11435 to leave a local Ollama on 11434 alone. Keys: `OLLAMA_URL`, `OLLAMA_HOST`, `OLLAMA_PORT`, `OLLAMA_MODEL`, `OLLAMA_EMBEDDING_MODEL`.

## Grafana LGTM

One container with an OTLP receiver (4317 gRPC, 4318 HTTP), Tempo, Loki, Prometheus and Grafana. The OpenTelemetry tests export spans and logs to OTLP and read them back through Grafana's Tempo and Loki proxies. Keys: `OTEL_EXPORTER_OTLP_ENDPOINT`, `OTEL_GRAFANA_URL`.

## Dashboards

nginx on 8080 is a landing page that links the web UIs (smtp4dev, RabbitMQ, Keycloak, OpenSearch Dashboards, Qdrant) and exposes `/health`. OpenSearch Dashboards on 5601 is served under `/opensearch-dashboards`. See [NGINX-DASHBOARD.md](../../../../containers/testing/NGINX-DASHBOARD.md). Keys: `NGINX_URL`, `OPENSEARCH_DASHBOARDS_URL`.

[← Docker infrastructure](docker-infrastructure.md) · [Writing tests →](writing-tests.md)
