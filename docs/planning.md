% Complete	Task Name	Notes
0%	Template	
49%	Framework	
0%	   Telemetry	
0%	      Add Logging Integration and Context support for Application Insights	https://learn.microsoft.com/en-us/azure/azure-monitor/app/opentelemetry-overview?tabs=aspnetcore#opentelemetry
0%	      Add Logging Integration and Context support for OpenTelemetry	Prometheus for metrics collection, Grafana for creating a dashboard, and Jaeger
0%	      Add HTTP Client Factory to support to enable Context carry though	
0%	      Add Context to Message Queueing	
0%	      Extensions	
0%	         Metrics	https://learn.microsoft.com/en-us/dotnet/core/diagnostics/built-in-metrics-diagnostics#microsoftextensionsdiagnosticshealthchecks
0%	         Event Counters	https://learn.microsoft.com/en-us/dotnet/core/diagnostics/event-counters
0%	         Tracing	
0%	         Logging	
77%	   Document Services	
91%	      Conversion	
100%	         Chaining	
100%	         Formats	
100%	            MS Doc to HTML (out of process - Apache Tika)	
100%	            MS Doc (docx) to HTML (out of process - Apache Tika)	
100%	            Epub to HTML (out of process - Apache Tika)	
100%	            Open Document (odt) to HTML (out of process - Apache Tika)	
100%	            Adobe PDF to HTML (out of process - Apache Tika)	
100%	            Real Text Format (rtf) to HTML (out of process - Apache Tika)	
100%	            Markdown to HTML (in process - Markdig)	
100%	            HTML to Markdown (in process - Mystic Mind)	
100%	            HTML to PDF (in process - wkhtmlpdf)	
0%	         Format extensions	
0%	            Markdig extensions	
0%	               PlantUML	
0%	      Packing	
0%	         Files to ZIP	
0%	         ZIP to Files	
83%	   Identity Access Management (IAM)	
100%	      Authentication	
100%	         JWT/Oauth	
100%	            Azure B2C	
100%	            Keycloak	
50%	      Account Management	
100%	         Azure B2C	
0%	         Keycloak	
100%	      Authorization	
100%	         Claims/Attributes based Rights filtering	
100%	         Authentication Requirement Filtering	
17%	   Blob Storage	
100%	      Azure Storage Containers	
0%	      AWS Simple Storage Service (S3)	
0%	      Microsoft OneDrive	
0%	      Google Cloud Storage	
0%	      Microsoft SQL - File Stream	
0%	      Postgresql Blob Storage	
50%	   Text Templating	
50%	      Data Sourcing	
0%	         dotnet Objects	
100%	         JSON	
100%	         XML	
0%	         YAML	
50%	      Providers	
100%	         Handle Bars	
100%	         XSLT	
0%	         Handle Bars - Extensions	
0%	         XSLT - Extensions	
14%	   Communications	
10%	      Inbound	
50%	         IMAP (Mailkit)	
0%	         POP3 (Mailkit)	
0%	         SMS	
0%	         Microsoft Teams	
0%	         Slack	
18%	      Outbound	
90%	         SMTP (Mailkit)	missing attachments
0%	         SMS	
0%	         Microsoft Teams	
0%	         Slack	
0%	         Email - SendGrid	
49%	   Framework Extensions	
60%	      Mongo DB	
100%	         Extend BSON to JSON serializer to support objectid	
100%	         Extend BSON to JSON serializer to support datetimeoffset	
100%	         Transparent Client abstraction/context factory	
0%	         Vector Query	
0%	         Full Text Query	
25%	      Open Search	
0%	         Vector Query	
50%	         Full Text Query	
90%	      Qdrant	
90%	         Vector Query	
25%	      Postgrsql 	
50%	         Vector Query	
0%	         Full Text Query	
70%	   Machine Learning / Artificial Intelligence	
45%	      Content Competition	
90%	         Ollama	
0%	         Open AI	
65%	      Generative Chat	
50%	         Ollama	
80%	         Open AI	
85%	      Embedding	
100%	         Ollama	
50%	         Open AI	
95%	         SBERT	
90%	            Client Abstraction	
100%	            Docker Host	
67%	   Message Queueing	
100%	      Rabbit MQ	
100%	      Azure Storage Queues	
50%	      AWS Simple Queue Service (SQS) - LocalStack	
0%	      Kafka	
50%	      Azure Service Bus	
100%	      In process	
28%	   Health Checks	
80%	      Endpoint Generation	currently unfiltered might want claims/authentication support as well as the ability to disable endpoint entirely
0%	      Transparent Extension	automatically registered is using Microsoft.Extensions.Diagnostics.HealthChecks
26%	      Extensions	
100%	         Apache Tika 	
100%	         Mailkit SMTP	
100%	         Mailkit IMAP	
0%	         Mailkit POP3	
0%	         Azure Storage Queue Readers	
0%	         Azure Storage Queue Writers	
0%	         Azure Storage Containers 	
0%	         MongoDB	
0%	         PostgreSQL	
0%	         Microsoft SQL Server	
0%	         Rabbit MQ Readers	
0%	         Rabbit MQ Writers	
100%	         Ollama	
100%	         SBERT	
0%	         Qdrant	
0%	         OpenAI	
0%	         OpenSearch	
0%	         AWS - Localstack	
0%	         Kafka	
68%	   ASP.Net Extensions	
97%	      Swagger-UI/Open API	
100%	         Authentication - JWT/Oauth	
100%	         Rights Enumeration	
100%	         Authentication Enumeration	
100%	         File form Upload	
90%	         Health Check endpoint enumeration	needs the ability to remap endpoint
90%	         Search Query Endpoint Enumeration	support json post/get query would like to add normal form post
100%	         Grouping endpoints by Assembly	
0%	      Middleware	
0%	         Search Query (Iqueryable action to custom model)	
0%	         Correlation Info Context 	
0%	         Culture Info	
83%	   Framework Extensions	
75%	      Query/LINQ	
100%	         Common Model to Search/Filter/Sort/Page	
100%	         Ienumerable to async	
100%	         Iasyncenumerable to sync	
0%	         vector/search scoring	
75%	      Serialization	
100%	         JSON	
100%	         XML	
0%	         YAML	
100%	         BSON	
0%	      Configuration	
0%	         Command Line Mapping	
100%	      IO	
100%	         Stream Chunking by Length/Overlap	
100%	      Reflection	
100%	         Safe Value Mapping	
100%	         Safe Type Conversion	
100%	         Safe parsing	
100%	         Method Resolution Extensions	
98%	      Common	
90%	         String breaking by whitespace	should add tab/Unicode whitespace support...
100%	         Hashing - MD5	
100%	         Hashing - SHA256	
100%	         Hashing - SHA512	
92%	   Testability Extensions	
100%	      Abstracted Framework Features	
100%	         Date Time	
100%	         GUID	
100%	         Hashing	
100%	         Logging	
100%	      Result Capture	Attach serialized/generated results as a file attachment to the test run results...
50%	      Procedural Data Generation	needs back ported from older framework
0%	   Retired Abstractions	These were in the older shared framework, may have value and could be backported
0%	      Address Geolocation	
0%	         US Census	
0%	         Bing Maps	
0%	         Google Maps	
0%	      Accounts Billable	
0%	         Bill.com	
0%	      Entity Framework	
0%	         SQL Server Extensions	
0%	            Extended Properties Query Support	
0%	            Extensions to Tag tables with extended properties show related entity	
0%	            Improved deterministic naming for constraints and indexes	
0%	            Migration builder to add database with extended property of related version number	
0%	            Migration extension to support classification and masking	replacement for inmemory database to test relation queries against data from embedded resources. Allowed deterministic unit testing against DBContext
0%	         Embedded Resource Database	
0%	      Caching	
0%	         Common abstraction for caching	
0%	         Implmentations	
0%	            In Memory (Microsoft Extensions.caching.Memory)	
0%	            Redis	
63%	Common Tools	
100%	   Existing	
100%	      Text Template Conversion	
100%	      Source Link Correction	
44%	   Planned/Needs Ported	
50%	      Data Loader - Entity Framework	needs migrated from older framework
25%	      Data Loader - Mongo	needs migrated from nucleus and made more normalized
50%	      DACPAC Merge/Compiler	needs migrated from older framework
50%	      PackMan - Nuget Update automation tool	needs migrated from older framework.
                
                supports shared/pinned versions and support for multi-targeting 
23%	Testing 	
0%	   Integration Testing	
45%	   Unit Testing	
64%	Container Services	
0%	   Docker Registry	
0%	      Docker Hub	
0%	      Azure Docker Registry	
76%	   Docker Templates	
100%	      Apache Tika	
100%	      Azurite	
100%	      Keycloak	
100%	      MongoDB	
100%	      Ollama	
100%	      Open WebUI (Ollama UI)	
100%	      ParadeDB	
100%	      PG Admin	
100%	      Qdrant	
100%	      RabbitMQ	
100%	      Sbert	
100%	      SMTP4dev	
100%	      Microsoft SQL Server	
100%	      Kafka + Kraft (no zookeeper)	
0%	      Cosmos DB Emulator	HYPERLINK "https://learn.microsoft.com/en-us/azure/cosmos-db/how-to-develop-emulator?tabs=docker-linux%2Ccsharp&pivots=api-nosql"Use the emulator for development and CI - Azure Cosmos DB | Microsoft Learn 
100%	      OpenSearch	
100%	      LocalStack	
0%	      Prometheus	https://prometheus.io/docs/introduction/overview/
0%	      Jaeger	https://www.jaegertracing.io/
0%	      Grafana	https://grafana.com/
0%	      PlantUML	
0%	   Kubernetes	
0%	      Scale to zero	
0%	      Help Chart templates	
89%	Reporting	
100%	   XML Docs to Markdown	
100%	   Software Bill of Materials (sbom)	
100%	   SBOM to Markdown	
100%	   Service Endpoints 	
100%	   Swagger/OpenAPI - JSON	
100%	   Swagger/OpenAPI - YAML	
25%	   SonarQube	Had working locally could enable and add to pipeline but requires deploy instance
86%	Scripts	
75%	   Local	
100%	      Build	
100%	      Unit Test	
100%	      Generation Documentation	
0%	      docker-compose to k8s	https://kompose.io/ https://skaffold.dev/
100%	   Pipelines	
100%	      Azure DevOps Gated Build	
100%	      Azure DevOps Deployment	
100%	      Github Actions Gated Build	
13%	Proof of Concepts	
50%	   Vector Search	
0%	      MongoDB	
0%	      OpenSearch	
100%	      Postgresql/paradedb - pg_vector	
100%	      Qdrant	
0%	   Databases	
0%	      Distributed Postgresql - Citrus + CosmosDB	https://github.com/citusdata/citus
0%	   Cloud Providers	
0%	      AWS - Localstack	
0%	      Google Cloud	https://cloud.google.com/sdk/gcloud/reference/emulators/firestore
0%	      Firebase	https://firebase.google.com/firebase-and-gcp/
0%	   Distributed Caching	
0%	      Redis	
0%	      Memcache	https://www.memcached.org/
0%	   Business Process Management / Workflow Engine	
0%	      Review and select	https://github.com/meirwah/awesome-workflow-engines
0%	   Business Rules Engine	
0%	      review and select	https://www.nected.ai/blog/open-source-rules-engine
0%	      Microsoft RulesEngine	https://github.com/microsoft/RulesEngine
0%	      Drools	https://www.drools.org/
0%	   Application Runtimes	
0%	      DAPR	https://dapr.io/
0%	      Istio	https://istio.io/latest/about/service-mesh/
0%	Application Platform	
0%	   Modular	
0%	   Identity Access Management	
0%	      Authentication	
0%	      Authorization	
0%	      User Management	
0%	      Module / Roles mapping	
0%	      Roles / Rights mapping	
0%	      User / Roles mapping	
0%	   User Profile	
0%	      Details	
0%	      Contact	
0%	      Avatar	
0%	      Notification Preferences	
0%	      Messaging Preferences	
0%	      Language/Culture Preference	
0%	   Application Notifications	
0%	      Toast	
0%	      Snackbar	
0%	      Banner	
0%	      Notification Editior	
0%	   Messaging	
0%	      In App	
0%	      Email	
0%	      SMS	
0%	      Chat	
0%	         Slack	
0%	         Teams	
0%	   Scheduler	
0%	      Events	
0%	      Calendar	
0%	   Event Processing	
0%	      Publish	
0%	      Subscription	
0%	   Components	
0%	      Searchable Grids	
0%	         Data Export	
0%	      Dashboard	
0%	         Analytics	
0%	      Reports	
0%	   Chatbot	
0%	      Knowledge base	
0%	      Enhanced Language Model	
0%	      Dataloader	
