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

// SQL Server 2019 with the SQL CLR vector types from OoBDev.Data.Vectors.DB (the version the dacpac targets, Sql150).
var sql = builder.AddSqlServer("sql")
    .WithImageTag("2019-latest")
    .WithDataVolume("oobd-aspire-sqlserver");

var vectorDb = sql.AddDatabase("VectorDb")
    .WithVectorTypes(Path.Combine(repoRoot, "src", "Extensions", "OoBDev.Data.Vectors.DB", "bin", "Debug", "netstandard2.1", "OoBDev.Data.Vectors.DB.dacpac"));

var smtp = builder.AddContainer("smtp4dev", "rnwood/smtp4dev")
    .WithHttpEndpoint(targetPort: 80, name: "web")
    .WithEndpoint(targetPort: 25, name: "smtp");

builder.AddContainer("apache-tika", "apache/tika")
    .WithHttpEndpoint(targetPort: 9998, name: "http");

var keycloak = builder.AddContainer("keycloak", "quay.io/keycloak/keycloak", "latest")
    .WithHttpEndpoint(targetPort: 8080, name: "http")
    .WithEnvironment("KEYCLOAK_ADMIN", "admin")
    .WithEnvironment("KEYCLOAK_ADMIN_PASSWORD", "admin")
    .WithEnvironment("KC_HEALTH_ENABLED", "true")
    .WithEnvironment("KC_HOSTNAME_STRICT", "false")
    .WithBindMount(Path.Combine(testing, "keycloak-config", "integration-test-realm.json"), "/opt/keycloak/data/import/integration-test-realm.json", isReadOnly: true)
    .WithArgs("start-dev", "--import-realm", "--features=scripts");

// phi3 is baked into the image at build time, so the first start builds it from the same Dockerfile the compose stack uses.
var ollama = builder.AddDockerfile("ollama", Path.Combine(testing, "ollama"))
    .WithHttpEndpoint(targetPort: 11434, name: "http")
    .WithEnvironment("OLLAMA_HOST", "0.0.0.0")
    .WithVolume("oobd-aspire-ollama", "/root/.ollama");

var api = builder.AddProject<OoBDev_Example_WebApi>("example-api", options => options.ExcludeLaunchProfile = true)
    // The API's launch profile names an https port, which turns on its https redirect; the host proxies plain http.
    .WithHttpEndpoint(env: "ASPNETCORE_HTTP_PORTS")
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
    .WithEnvironment("IDENTITY_PROVIDER", "None")
    .WithEnvironment("OllamaApiClientOptions__Url", ollama.GetEndpoint("http"))
    .WithEnvironment("OllamaApiClientOptions__DefaultModel", "phi3")
    .WithReference(redis)
    .WithReference(rabbitmq)
    .WithReference(vectorDb)
    // The queue reader looks up its own connection string name; appsettings.json points it at a local SQL Server on 1433.
    .WithEnvironment("ConnectionStrings__EmbeddingSentenceTransformer", vectorDb)
    .WaitFor(vectorDb)
    .WaitFor(ollama)
    .WaitFor(redis);

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
    .WithExternalHttpEndpoints();

builder.Build().Run();
