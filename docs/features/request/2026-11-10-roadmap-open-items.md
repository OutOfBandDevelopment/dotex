# Feature request: unbuilt items from the 2025 roadmap tree

- **Status:** new
- **Requested by:** owner (kept from `docs/planning.md`, removed 2026-11-10)
- **Date:** 2026-11-10
- **Area:** many (storage, communications, search, caching, workflow, platform)

## Need

`docs/planning.md` was a project-plan export (percent complete per task) from 2025. Finished branches are in the code; the open ones (below 100%) are kept here so none is lost. Many are done since (Qdrant, OpenSearch, SBERT, Moto, Aspire) or dropped (Application Insights, Grafana, Azure B2C, Swagger UI).

## Proposed behavior

Triage each open line into a design document and `BACKLOG.md`, or decline it with a reason. The percentage is the 2025 value.

## Acceptance criteria

- Given each open line, when triaged, then it is built, in the backlog, or declined with a reason.

## Existing code checked

Not done per line. Start with `BACKLOG.md`, `docs/design/Communications`, `SharedCaching` and `DocumentServices`.

## Security and operations

Unknown per line.

## Priority

- **Impact:** unknown
- **Needed by:** no date
- **Workaround today:** none

## Open questions

Which areas are still wanted (workflow engine, rules engine, DAPR, S3 and other blob stores, SMS, Teams, Slack)?

## Triage notes

Open lines (indentation shows the original nesting; text after a dash is the original note):

- Template (0%)
- Framework (49%)
  - Telemetry (0%)
    - Add Logging Integration and Context support for Application Insights (0%) - https://learn.microsoft.com/en-us/azure/azure-monitor/app/opentelemetry-overview?tabs=aspnetcore#opentelemetry
    - Add Logging Integration and Context support for OpenTelemetry (0%) - Prometheus for metrics collection, Grafana for creating a dashboard, and Jaeger
    - Add HTTP Client Factory to support to enable Context carry though (0%)
    - Add Context to Message Queueing (0%)
    - Extensions (0%)
      - Metrics (0%) - https://learn.microsoft.com/en-us/dotnet/core/diagnostics/built-in-metrics-diagnostics#microsoftextensionsdiagnosticshealthchecks
      - Event Counters (0%) - https://learn.microsoft.com/en-us/dotnet/core/diagnostics/event-counters
      - Tracing (0%)
      - Logging (0%)
  - Document Services (77%)
    - Conversion (91%)
      - Format extensions (0%)
        - Markdig extensions (0%)
          - PlantUML (0%)
    - Packing (0%)
      - Files to ZIP (0%)
      - ZIP to Files (0%)
  - Identity Access Management (IAM) (83%)
    - Account Management (50%)
      - Keycloak (0%)
  - Blob Storage (17%)
    - AWS Simple Storage Service (S3) (0%)
    - Microsoft OneDrive (0%)
    - Google Cloud Storage (0%)
    - Microsoft SQL - File Stream (0%)
    - Postgresql Blob Storage (0%)
  - Text Templating (50%)
    - Data Sourcing (50%)
      - dotnet Objects (0%)
      - YAML (0%)
    - Providers (50%)
      - Handle Bars - Extensions (0%)
      - XSLT - Extensions (0%)
  - Communications (14%)
    - Inbound (10%)
      - IMAP (Mailkit) (50%)
      - POP3 (Mailkit) (0%)
      - SMS (0%)
      - Microsoft Teams (0%)
      - Slack (0%)
    - Outbound (18%)
      - SMTP (Mailkit) (90%) - missing attachments
      - SMS (0%)
      - Microsoft Teams (0%)
      - Slack (0%)
      - Email - SendGrid (0%)
  - Framework Extensions (49%)
    - Mongo DB (60%)
      - Vector Query (0%)
      - Full Text Query (0%)
    - Open Search (25%)
      - Vector Query (0%)
      - Full Text Query (50%)
    - Qdrant (90%)
      - Vector Query (90%)
    - Postgrsql (25%)
      - Vector Query (50%)
      - Full Text Query (0%)
  - Machine Learning / Artificial Intelligence (70%)
    - Content Competition (45%)
      - Ollama (90%)
      - Open AI (0%)
    - Generative Chat (65%)
      - Ollama (50%)
      - Open AI (80%)
    - Embedding (85%)
      - Open AI (50%)
      - SBERT (95%)
        - Client Abstraction (90%)
  - Message Queueing (67%)
    - AWS Simple Queue Service (SQS) - LocalStack (50%)
    - Kafka (0%)
    - Azure Service Bus (50%)
  - Health Checks (28%)
    - Endpoint Generation (80%) - currently unfiltered might want claims/authentication support as well as the ability to disable endpoint entirely
    - Transparent Extension (0%) - automatically registered is using Microsoft.Extensions.Diagnostics.HealthChecks
    - Extensions (26%)
      - Mailkit POP3 (0%)
      - Azure Storage Queue Readers (0%)
      - Azure Storage Queue Writers (0%)
      - Azure Storage Containers (0%)
      - MongoDB (0%)
      - PostgreSQL (0%)
      - Microsoft SQL Server (0%)
      - Rabbit MQ Readers (0%)
      - Rabbit MQ Writers (0%)
      - Qdrant (0%)
      - OpenAI (0%)
      - OpenSearch (0%)
      - AWS - Localstack (0%)
      - Kafka (0%)
  - ASP.Net Extensions (68%)
    - Swagger-UI/Open API (97%)
      - Health Check endpoint enumeration (90%) - needs the ability to remap endpoint
      - Search Query Endpoint Enumeration (90%) - support json post/get query would like to add normal form post
    - Middleware (0%)
      - Search Query (Iqueryable action to custom model) (0%)
      - Correlation Info Context (0%)
      - Culture Info (0%)
  - Framework Extensions (83%)
    - Query/LINQ (75%)
      - vector/search scoring (0%)
    - Serialization (75%)
      - YAML (0%)
    - Configuration (0%)
      - Command Line Mapping (0%)
    - Common (98%)
      - String breaking by whitespace (90%) - should add tab/Unicode whitespace support...
  - Testability Extensions (92%)
    - Procedural Data Generation (50%) - needs back ported from older framework
  - Retired Abstractions (0%) - These were in the older shared framework, may have value and could be backported
    - Address Geolocation (0%)
      - US Census (0%)
      - Bing Maps (0%)
      - Google Maps (0%)
    - Accounts Billable (0%)
      - Bill.com (0%)
    - Entity Framework (0%)
      - SQL Server Extensions (0%)
        - Extended Properties Query Support (0%)
        - Extensions to Tag tables with extended properties show related entity (0%)
        - Improved deterministic naming for constraints and indexes (0%)
        - Migration builder to add database with extended property of related version number (0%)
        - Migration extension to support classification and masking (0%) - replacement for inmemory database to test relation queries against data from embedded resources. Allowed deterministic unit testing against DBContext
      - Embedded Resource Database (0%)
    - Caching (0%)
      - Common abstraction for caching (0%)
      - Implmentations (0%)
        - In Memory (Microsoft Extensions.caching.Memory) (0%)
        - Redis (0%)
- Common Tools (63%)
  - Planned/Needs Ported (44%)
    - Data Loader - Entity Framework (50%) - needs migrated from older framework
    - Data Loader - Mongo (25%) - needs migrated from example and made more normalized
    - DACPAC Merge/Compiler (50%) - needs migrated from older framework
    - PackMan - Nuget Update automation tool (50%) - needs migrated from older framework.
- Testing (23%)
  - Integration Testing (0%)
  - Unit Testing (45%)
- Container Services (64%)
  - Docker Registry (0%)
    - Docker Hub (0%)
    - Azure Docker Registry (0%)
  - Docker Templates (76%)
    - Cosmos DB Emulator (0%) - Use the emulator for development and CI - Azure Cosmos DB | Microsoft Learn 
    - Prometheus (0%) - https://prometheus.io/docs/introduction/overview/
    - Jaeger (0%) - https://www.jaegertracing.io/
    - Grafana (0%) - https://grafana.com/
    - PlantUML (0%)
  - Kubernetes (0%)
    - Scale to zero (0%)
    - Help Chart templates (0%)
- Reporting (89%)
  - SonarQube (25%) - Had working locally could enable and add to pipeline but requires deploy instance
- Scripts (86%)
  - Local (75%)
    - docker-compose to k8s (0%) - https://kompose.io/ https://skaffold.dev/
- Proof of Concepts (13%)
  - Vector Search (50%)
    - MongoDB (0%)
    - OpenSearch (0%)
  - Databases (0%)
    - Distributed Postgresql - Citrus + CosmosDB (0%) - https://github.com/citusdata/citus
  - Cloud Providers (0%)
    - AWS - Localstack (0%)
    - Google Cloud (0%) - https://cloud.google.com/sdk/gcloud/reference/emulators/firestore
    - Firebase (0%) - https://firebase.google.com/firebase-and-gcp/
  - Distributed Caching (0%)
    - Redis (0%)
    - Memcache (0%) - https://www.memcached.org/
  - Business Process Management / Workflow Engine (0%)
    - Review and select (0%) - https://github.com/meirwah/awesome-workflow-engines
  - Business Rules Engine (0%)
    - review and select (0%) - https://www.nected.ai/blog/open-source-rules-engine
    - Microsoft RulesEngine (0%) - https://github.com/microsoft/RulesEngine
    - Drools (0%) - https://www.drools.org/
  - Application Runtimes (0%)
    - DAPR (0%) - https://dapr.io/
    - Istio (0%) - https://istio.io/latest/about/service-mesh/
- Application Platform (0%)
  - Modular (0%)
  - Identity Access Management (0%)
    - Authentication (0%)
    - Authorization (0%)
    - User Management (0%)
    - Module / Roles mapping (0%)
    - Roles / Rights mapping (0%)
    - User / Roles mapping (0%)
  - User Profile (0%)
    - Details (0%)
    - Contact (0%)
    - Avatar (0%)
    - Notification Preferences (0%)
    - Messaging Preferences (0%)
    - Language/Culture Preference (0%)
  - Application Notifications (0%)
    - Toast (0%)
    - Snackbar (0%)
    - Banner (0%)
    - Notification Editior (0%)
  - Messaging (0%)
    - In App (0%)
    - Email (0%)
    - SMS (0%)
    - Chat (0%)
      - Slack (0%)
      - Teams (0%)
  - Scheduler (0%)
    - Events (0%)
    - Calendar (0%)
  - Event Processing (0%)
    - Publish (0%)
    - Subscription (0%)
  - Components (0%)
    - Searchable Grids (0%)
      - Data Export (0%)
    - Dashboard (0%)
      - Analytics (0%)
    - Reports (0%)
  - Chatbot (0%)
    - Knowledge base (0%)
    - Enhanced Language Model (0%)
    - Dataloader (0%)
