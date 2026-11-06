using System;
using System.IO;
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using OoBDev.Example.AppHost;
using Projects;

// Runs the example application and the backend services it uses, reusing the definitions under containers/testing
// (the Ollama Dockerfile and the Keycloak realm import) so the compose stack and this host stay one set of definitions.
var builder = DistributedApplication.CreateBuilder(args);

var repoRoot = Path.GetFullPath(Path.Combine(builder.AppHostDirectory, "..", "..", ".."));
var testing = Path.Combine(repoRoot, "containers", "testing");

builder.AddDockerComposeEnvironment("example");

var redis = builder.AddRedis("redis");

var rabbitmq = builder.AddRabbitMQ("rabbitmq")
    .WithManagementPlugin();

// The sa password is fixed to the SQL_SA_PASSWORD default the compose stacks use (containers/testing/.env.integration). A generated
// password changes with the user secrets, and the persisted data volume below then rejects the login.
var sqlPassword = builder.AddParameter("sql-password", Environment.GetEnvironmentVariable("SQL_SA_PASSWORD") ?? "IntegrationTest123!", secret: true);

// SQL Server 2019 with the SQL CLR vector types from OoBDev.Data.Vectors.DB (the version the dacpac targets, Sql150).
var sql = builder.AddSqlServer("sql", sqlPassword)
    .WithImageTag("2019-latest")
    .WithDataVolume("oobd-aspire-sqlserver-fixed-sa");

#if DEBUG
const string BuildConfiguration = "Debug";
#else
const string BuildConfiguration = "Release";
#endif

var vectorDb = sql.AddDatabase("VectorDb")
    .WithVectorTypes(Path.Combine(repoRoot, "src", "Extensions", "OoBDev.Data.Vectors.DB", "bin", BuildConfiguration, "netstandard2.1", "OoBDev.Data.Vectors.DB.dacpac"));

var smtp = builder.AddContainer("smtp4dev", "rnwood/smtp4dev")
    .WithHttpEndpoint(targetPort: 80, name: "web")
    .WithEndpoint(targetPort: 25, name: "smtp");

builder.AddContainer("apache-tika", "apache/tika")
    .WithHttpEndpoint(targetPort: 9998, name: "http");

// The port is fixed because the token issuer is the browser-visible address; a dynamic port would change the issuer on every run.
var keycloakPort = int.Parse(Environment.GetEnvironmentVariable("TEST_PORT_KEYCLOAK") ?? "8081", System.Globalization.CultureInfo.InvariantCulture);
var keycloak = builder.AddContainer("keycloak", "quay.io/keycloak/keycloak", "latest")
    .WithHttpEndpoint(port: keycloakPort, targetPort: 8080, name: "http")
    .WithEnvironment("KEYCLOAK_ADMIN", "admin")
    .WithEnvironment("KEYCLOAK_ADMIN_PASSWORD", "admin")
    .WithEnvironment("KC_HEALTH_ENABLED", "true")
    .WithEnvironment("KC_HOSTNAME_STRICT", "false")
    .WithBindMount(Path.Combine(testing, "keycloak-config", "integration-test-realm.json"), "/opt/keycloak/data/import/integration-test-realm.json", isReadOnly: true)
    .WithBindMount(Path.Combine(repoRoot, "containers", "example", "keycloak", "oobdev-example-realm.json"), "/opt/keycloak/data/import/oobdev-example-realm.json", isReadOnly: true)
    .WithArgs("start-dev", "--import-realm", "--features=scripts");

var keycloakRealm = $"http://localhost:{keycloakPort}/realms/oobdev-example";

// phi3 is baked into the image at build time, so the first start builds it from the same Dockerfile the compose stack uses.
var ollama = builder.AddDockerfile("ollama", Path.Combine(testing, "ollama"))
    .WithHttpEndpoint(targetPort: 11434, name: "http")
    .WithEnvironment("OLLAMA_HOST", "0.0.0.0")
    .WithVolume("oobd-aspire-ollama", "/root/.ollama");

var api = builder.AddProject<OoBDev_Example_WebApi>("example-api", options => options.ExcludeLaunchProfile = true)
    // The API's launch profile names an https port, which turns on its https redirect; the host proxies plain http.
    .WithHttpEndpoint(env: "ASPNETCORE_HTTP_PORTS")
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
    .WithEnvironment("IDENTITY_PROVIDER", "Keycloak")
    .WithEnvironment("Keycloak__JwtBearerOptions__Authority", keycloakRealm)
    .WithEnvironment("Keycloak__JwtBearerOptions__Audience", "oobdev-example-api")
    .WithEnvironment("Keycloak__JwtBearerOptions__RequireHttpsMetadata", "false")
    .WithEnvironment("Keycloak__JwtBearerOptions__TokenValidationParameters__ValidIssuers__0", keycloakRealm)
    .WithEnvironment("Keycloak__OAuth2OpenApiOptions__AuthorizationUrl", $"{keycloakRealm}/protocol/openid-connect/auth/")
    .WithEnvironment("Keycloak__OAuth2OpenApiOptions__TokenUrl", $"{keycloakRealm}/protocol/openid-connect/token")
    .WaitFor(keycloak)
    .WithEnvironment("OllamaApiClientOptions__Url", ollama.GetEndpoint("http"))
    .WithEnvironment("OllamaApiClientOptions__DefaultModel", "phi3")
    .WithReference(redis)
    .WithReference(rabbitmq)
    .WithReference(vectorDb)
    // The queue reader looks up its own connection string name; appsettings.json points it at a local SQL Server on 1433.
    .WithEnvironment("ConnectionStrings__EmbeddingSentenceTransformer", vectorDb)
    // The vector store provider reads its own options section; without it the API falls back to the in-memory store.
    .WithEnvironment("SqlVectorStoreOptions__ConnectionString", vectorDb)
    .WaitFor(vectorDb)
    .WaitFor(ollama)
    .WaitFor(redis)
    // The mail client and the queue providers read host, port and credentials from their own sections (the compose file sets the same keys),
    // not from the connection string the reference adds; without these they call localhost on the default ports and the send and queue calls fail.
    .WithEnvironment("MailKitSmtpClientOptions__Host", smtp.GetEndpoint("smtp").Property(EndpointProperty.Host))
    .WithEnvironment("MailKitSmtpClientOptions__Port", smtp.GetEndpoint("smtp").Property(EndpointProperty.Port));

// The Telemetry page links to the dashboard and reads recent traces through its API; the defaults point at the compose stack's port 18888.
// ASPNETCORE_URLS is the host's own dashboard address (the launch profile's, or a dynamic one from the CLI).
var dashboardUrl = (builder.Configuration["ASPNETCORE_URLS"] ?? "http://localhost:15888").Split(';')[0];
api.WithEnvironment("Telemetry__DashboardUrl", dashboardUrl)
    .WithEnvironment("Telemetry__PublicUrl", dashboardUrl)
    .WithEnvironment("Telemetry__ApiKey", builder.Configuration["AppHost:DashboardApiKey"] ?? "")
    .WithEnvironment("OpenTelemetryOptions__ServiceName", "example-api")
    .WithEnvironment("OpenTelemetryOptions__Sources__0", "Microsoft.AspNetCore");

foreach (var queue in new[] { "ExampleMessageProvider", "EmailMessageModel" })
{
    api.WithEnvironment($"MessageQueue__{queue}__Config__HostName", rabbitmq.GetEndpoint("tcp").Property(EndpointProperty.Host))
        .WithEnvironment($"MessageQueue__{queue}__Config__Port", rabbitmq.GetEndpoint("tcp").Property(EndpointProperty.Port))
        .WithEnvironment($"MessageQueue__{queue}__Config__UserName", rabbitmq.Resource.UserNameReference)
        .WithEnvironment($"MessageQueue__{queue}__Config__Password", rabbitmq.Resource.PasswordParameter);
}

// Under Visual Studio the child inherits ASPNETCORE_URLS=https://localhost:5001;http://localhost:5000, and URLS outranks HTTP_PORTS, so the API
// would ignore the port the host proxies to and every request through the proxy (including the web site's) would hang.
api.WithEnvironment("ASPNETCORE_URLS", ReferenceExpression.Create($"http://localhost:{api.GetEndpoint("http").Property(EndpointProperty.TargetPort)}"));

// Optional secrets (for example GroqCloudApiClientOptions__ApiKey) come from containers/example/.env, the same file the compose stack reads.
var envFile = Path.Combine(repoRoot, "containers", "example", ".env");
if (File.Exists(envFile))
{
    foreach (var line in File.ReadAllLines(envFile))
    {
        var text = line.Trim();
        if (text.Length == 0 || text.StartsWith('#')) continue;
        var split = text.IndexOf('=');
        if (split > 0) api.WithEnvironment(text[..split].Trim(), text[(split + 1)..].Trim());
    }
}

builder.AddViteApp("example-web", Path.Combine("..", "OoBDev.Example.Web"))
    .WithReference(api)
    .WaitFor(api)
    .WithEnvironment("VITE_OIDC_AUTHORITY", keycloakRealm)
    .WithEnvironment("VITE_OIDC_CLIENT_ID", "oobdev-example-web")
    .WithExternalHttpEndpoints();

builder.Build().Run();
