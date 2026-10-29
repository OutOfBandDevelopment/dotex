using System;
using System.IO;
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;

// The integration test stack as an Aspire host: the same services, images, host ports and credentials as
// containers/testing/docker-compose.integration-tests.yml and src/.runsettings, so `dotnet test --filter TestCategory=Integration`
// runs unchanged against either. Every host port is overridable with TEST_PORT_<NAME> (as in the compose file).
// Containers are persistent: stopping the host leaves them (and their data volumes) running for the next test run.
var builder = DistributedApplication.CreateBuilder(args);

var repoRoot = Path.GetFullPath(Path.Combine(builder.AppHostDirectory, "..", "..", ".."));
var containers = Path.Combine(repoRoot, "containers");
var testing = Path.Combine(containers, "testing");

static int HostPort(string name, int fallback) =>
    int.TryParse(Environment.GetEnvironmentVariable(name), System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var port) ? port : fallback;

static string Setting(string name, string fallback) =>
    Environment.GetEnvironmentVariable(name) is { Length: > 0 } value ? value : fallback;

var sqlPassword = builder.AddParameter("sql-password", () => Setting("SQL_SA_PASSWORD", "IntegrationTest123!"), secret: true);
var searchPassword = Setting("OPENSEARCH_PASSWORD", "IntegrationTest123!");
var otelKey = Setting("OTEL_DASHBOARD_API_KEY", "oobdev-test-key");
var rabbitUser = builder.AddParameter("rabbitmq-user", () => "guest");
var rabbitPassword = builder.AddParameter("rabbitmq-password", () => "guest", secret: true);

// Stateless services
builder.AddContainer("apache-tika", "apache/tika")
    .WithEndpoint(port: HostPort("TEST_PORT_TIKA", 9998), targetPort: 9998, name: "http", scheme: "http", isProxied: false)
    .WithHttpHealthCheck("/tika")
    .WithLifetime(ContainerLifetime.Persistent);

builder.AddContainer("smtp4dev", "rnwood/smtp4dev")
    .WithEndpoint(port: HostPort("TEST_PORT_SMTP_WEB", 7777), targetPort: 80, name: "http", scheme: "http", isProxied: false)
    .WithEndpoint(port: HostPort("TEST_PORT_SMTP", 25), targetPort: 25, name: "smtp", scheme: "tcp", isProxied: false)
    .WithEndpoint(port: HostPort("TEST_PORT_IMAP", 143), targetPort: 143, name: "imap", scheme: "tcp", isProxied: false)
    .WithHttpHealthCheck("/", endpointName: "http")
    .WithLifetime(ContainerLifetime.Persistent);

// Databases and queues
builder.AddContainer("mongodb", "mongo", "latest")
    .WithEndpoint(port: HostPort("TEST_PORT_MONGODB", 27017), targetPort: 27017, name: "mongo", scheme: "tcp", isProxied: false)
    .WithVolume("oobd-test-mongodb", "/data/db")
    .WithLifetime(ContainerLifetime.Persistent);

var sql = builder.AddSqlServer("sql-server", sqlPassword, HostPort("TEST_PORT_SQLSERVER", 1433))
    .WithImageTag("2022-latest")
    .WithEnvironment("MSSQL_PID", "Developer")
    .WithDataVolume("oobd-test-sqlserver")
    .WithLifetime(ContainerLifetime.Persistent);

builder.AddRabbitMQ("rabbitmq", rabbitUser, rabbitPassword, HostPort("TEST_PORT_RABBITMQ", 5673))
    .WithManagementPlugin(HostPort("TEST_PORT_RABBITMQ_MGMT", 15672))
    .WithDataVolume("oobd-test-rabbitmq")
    .WithLifetime(ContainerLifetime.Persistent);

builder.AddRedis("redis", HostPort("TEST_PORT_REDIS", 6379))
    .WithDataVolume("oobd-test-redis")
    .WithArgs("--maxmemory", "256mb", "--maxmemory-policy", "allkeys-lru", "--appendonly", "no")
    .WithLifetime(ContainerLifetime.Persistent);

var opensearch = builder.AddContainer("opensearch", "opensearchproject/opensearch", "latest")
    .WithEndpoint(port: HostPort("TEST_PORT_OPENSEARCH", 9200), targetPort: 9200, name: "https", scheme: "https", isProxied: false)
    .WithEndpoint(port: HostPort("TEST_PORT_OPENSEARCH_PERF", 9600), targetPort: 9600, name: "perf", scheme: "http", isProxied: false)
    .WithEnvironment("discovery.type", "single-node")
    .WithEnvironment("OPENSEARCH_INITIAL_ADMIN_PASSWORD", searchPassword)
    .WithEnvironment("OPENSEARCH_JAVA_OPTS", "-Xms512m -Xmx512m")
    .WithEnvironment("bootstrap.memory_lock", "true")
    .WithEnvironment("DISABLE_SECURITY_PLUGIN", "false")
    .WithContainerRuntimeArgs("--ulimit", "memlock=-1:-1", "--ulimit", "nofile=65536:65536")
    .WithVolume("oobd-test-opensearch", "/usr/share/opensearch/data")
    .WithLifetime(ContainerLifetime.Persistent);

builder.AddContainer("qdrant", "qdrant/qdrant")
    .WithEndpoint(port: HostPort("TEST_PORT_QDRANT", 6333), targetPort: 6333, name: "http", scheme: "http", isProxied: false)
    .WithEndpoint(port: HostPort("TEST_PORT_QDRANT_GRPC", 6334), targetPort: 6334, name: "grpc", scheme: "http", isProxied: false)
    .WithVolume("oobd-test-qdrant-storage", "/qdrant/storage")
    .WithVolume("oobd-test-qdrant-snapshots", "/qdrant/snapshots")
    .WithLifetime(ContainerLifetime.Persistent);

// Cloud emulators
builder.AddContainer("azurite", "mcr.microsoft.com/azure-storage/azurite")
    .WithEndpoint(port: HostPort("TEST_PORT_AZURITE_BLOB", 10000), targetPort: 10000, name: "blob", scheme: "http", isProxied: false)
    .WithEndpoint(port: HostPort("TEST_PORT_AZURITE_QUEUE", 10001), targetPort: 10001, name: "queue", scheme: "http", isProxied: false)
    .WithEndpoint(port: HostPort("TEST_PORT_AZURITE_TABLE", 10002), targetPort: 10002, name: "table", scheme: "http", isProxied: false)
    .WithArgs(
        "azurite",
        "--blobPort", "10000", "--blobHost", "0.0.0.0",
        "--queuePort", "10001", "--queueHost", "0.0.0.0",
        "--tablePort", "10002", "--tableHost", "0.0.0.0",
        "--location", "/data", "--loose", "--skipApiVersionCheck", "--disableProductStyleUrl")
    .WithVolume("oobd-test-azurite", "/data")
    .WithLifetime(ContainerLifetime.Persistent);

// Moto is an Apache-2.0 AWS emulator; host port 4566 is kept so SQS_ENDPOINT is unchanged.
var moto = builder.AddContainer("moto", "motoserver/moto", "latest")
    .WithEndpoint(port: HostPort("TEST_PORT_MOTO", 4566), targetPort: 5000, name: "http", scheme: "http", isProxied: false)
    .WithEnvironment("AWS_DEFAULT_REGION", "us-east-1")
    .WithHttpHealthCheck("/moto-api/")
    .WithLifetime(ContainerLifetime.Persistent);

// One-shot: creates the SQS queues the integration tests expect, then exits.
builder.AddContainer("moto-init", "amazon/aws-cli", "latest")
    .WithEntrypoint("/bin/sh")
    .WithArgs("/init/01-create-sqs-queues.sh")
    .WithBindMount(Path.Combine(testing, "moto-init"), "/init", isReadOnly: true)
    .WithEnvironment("MOTO_URL", "http://moto:5000")
    .WithEnvironment("AWS_ACCESS_KEY_ID", "test")
    .WithEnvironment("AWS_SECRET_ACCESS_KEY", "test")
    .WithEnvironment("AWS_DEFAULT_REGION", "us-east-1")
    .WaitFor(moto);

// The emulator keeps its metadata in SQL Server and has no health endpoint; tests retry the connection.
builder.AddContainer("servicebus-emulator", "mcr.microsoft.com/azure-messaging/servicebus-emulator", "latest")
    .WithEndpoint(port: HostPort("TEST_PORT_SERVICEBUS", 5672), targetPort: 5672, name: "amqp", scheme: "tcp", isProxied: false)
    .WithEnvironment("ACCEPT_EULA", "Y")
    .WithEnvironment("SQL_SERVER", sql.Resource.Name)
    .WithEnvironment("MSSQL_SA_PASSWORD", sqlPassword)
    .WithBindMount(Path.Combine(testing, "servicebus-config", "Config.json"), "/ServiceBus_Emulator/ConfigFiles/Config.json", isReadOnly: true)
    .WaitFor(sql)
    .WithLifetime(ContainerLifetime.Persistent);

// Telemetry: the Aspire dashboard container, with the telemetry API the OpenTelemetry Integration tests read back from.
builder.AddContainer("otel-dashboard", "mcr.microsoft.com/dotnet/aspire-dashboard", "latest")
    .WithEndpoint(port: HostPort("TEST_PORT_OTEL_DASHBOARD", 18888), targetPort: 18888, name: "http", scheme: "http", isProxied: false)
    .WithEndpoint(port: HostPort("TEST_PORT_OTLP_GRPC", 4317), targetPort: 18889, name: "otlp-grpc", scheme: "http", isProxied: false)
    .WithEndpoint(port: HostPort("TEST_PORT_OTLP_HTTP", 4318), targetPort: 18890, name: "otlp-http", scheme: "http", isProxied: false)
    .WithEnvironment("DASHBOARD__FRONTEND__AUTHMODE", "Unsecured")
    .WithEnvironment("DASHBOARD__OTLP__AUTHMODE", "Unsecured")
    .WithEnvironment("ASPIRE_ALLOW_UNSECURED_TRANSPORT", "true")
    .WithEnvironment("DASHBOARD__API__ENABLED", "true")
    .WithEnvironment("DASHBOARD__API__AUTHMODE", "ApiKey")
    .WithEnvironment("DASHBOARD__API__PRIMARYAPIKEY", otelKey)
    .WithLifetime(ContainerLifetime.Persistent);

// Identity: the integration realm and the example realm are imported at start.
var keycloak = builder.AddContainer("keycloak", "quay.io/keycloak/keycloak", "latest")
    .WithEndpoint(port: HostPort("TEST_PORT_KEYCLOAK", 8081), targetPort: 8080, name: "http", scheme: "http", isProxied: false)
    .WithEnvironment("KEYCLOAK_ADMIN", "admin")
    .WithEnvironment("KEYCLOAK_ADMIN_PASSWORD", "admin")
    .WithEnvironment("KC_HEALTH_ENABLED", "true")
    .WithEnvironment("KC_PROXY", "edge")
    .WithEnvironment("KC_HOSTNAME_STRICT", "false")
    .WithBindMount(Path.Combine(testing, "keycloak-config", "integration-test-realm.json"), "/opt/keycloak/data/import/integration-test-realm.json", isReadOnly: true)
    .WithBindMount(Path.Combine(containers, "example", "keycloak", "oobdev-example-realm.json"), "/opt/keycloak/data/import/oobdev-example-realm.json", isReadOnly: true)
    .WithArgs("start-dev", "--import-realm", "--features=scripts")
    .WithLifetime(ContainerLifetime.Persistent);

// Dashboards and the reverse proxy; nginx.conf reaches the upstreams by these resource names.
var dashboards = builder.AddContainer("opensearch-dashboards", "opensearchproject/opensearch-dashboards", "latest")
    .WithEndpoint(port: HostPort("TEST_PORT_OPENSEARCH_DASHBOARDS", 5601), targetPort: 5601, name: "http", scheme: "http", isProxied: false)
    .WithEnvironment("OPENSEARCH_HOSTS", "[\"https://opensearch:9200\"]")
    .WithEnvironment("DISABLE_SECURITY_DASHBOARDS_PLUGIN", "false")
    .WithEnvironment("OPENSEARCH_USERNAME", "admin")
    .WithEnvironment("OPENSEARCH_PASSWORD", searchPassword)
    .WithEnvironment("SERVER_BASEPATH", "/opensearch-dashboards")
    .WithEnvironment("SERVER_REWRITEBASEPATH", "true")
    .WaitFor(opensearch)
    .WithLifetime(ContainerLifetime.Persistent);

builder.AddContainer("nginx", "nginx", "alpine")
    .WithEndpoint(port: HostPort("TEST_PORT_NGINX", 8080), targetPort: 80, name: "http", scheme: "http", isProxied: false)
    .WithBindMount(Path.Combine(testing, "nginx", "nginx.conf"), "/etc/nginx/nginx.conf", isReadOnly: true)
    .WithBindMount(Path.Combine(testing, "nginx", "html"), "/usr/share/nginx/html", isReadOnly: true)
    .WaitFor(dashboards)
    .WaitFor(keycloak)
    .WithLifetime(ContainerLifetime.Persistent);

// AI services, CPU only. Both build from the repository Dockerfiles; phi3 is baked into the Ollama image at build time.
builder.AddDockerfile("sbert", containers, "DockerFile.sbert")
    .WithEndpoint(port: HostPort("TEST_PORT_SBERT", 5080), targetPort: 5000, name: "http", scheme: "http", isProxied: false)
    .WithLifetime(ContainerLifetime.Persistent);

builder.AddDockerfile("ollama", Path.Combine(testing, "ollama"))
    .WithEndpoint(port: HostPort("TEST_PORT_OLLAMA", 11435), targetPort: 11434, name: "http", scheme: "http", isProxied: false)
    .WithEnvironment("OLLAMA_HOST", "0.0.0.0")
    .WithVolume("oobd-test-ollama", "/root/.ollama")
    .WithLifetime(ContainerLifetime.Persistent);

builder.Build().Run();
