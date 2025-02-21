Here is the documentation for the provided source code files:

**ApacheTikaClient.cs**
```csharp
using Microsoft.Extensions.Logging;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace OoBDev.Apache.Tika
{
    public class ApacheTikaClient : IApacheTikaClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger _logger;

        public ApacheTikaClient(HttpClient httpClient, ILogger<ApacheTikaClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async ValueTask<string> DetectStreamAsync(Stream source)
        {
            // Logic for detecting content type
        }

        public async Task ConvertAsync(Stream source, string sourceContentType, Stream destination, string destinationContentType)
        {
            // Logic for converting documents
        }

        public async Task<string> GetHelloAsync()
        {
            // Logic for getting hello message
        }
    }
}
```
**Class Diagram:**
```
@startuml
class ApacheTikaClient {
  private readonly HttpClient _httpClient
  private readonly ILogger _logger

  +DetectStreamAsync(Stream source)
  +ConvertAsync(Stream source, string sourceContentType, Stream destination, string destinationContentType)
  +GetHelloAsync()

  ApacheTikaClient(HttpClient httpClient, ILogger<ApacheTikaClient> logger)
}
@enduml
```
**ApacheTikaClientOptions.cs**
```csharp
using OoBDev.Documents;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace OoBDev.Apache.Tika
{
    public class ApacheTikaClientOptions
    {
        public string Url { get; set; }
    }
}
```
**Class Diagram:**
```
@startuml
class ApacheTikaClientOptions {
  +Url {get; set;}
}
@enduml
```
**ApacheTikaHealthCheck.cs**
```csharp
using OoBDev.Apache.Tika.Detectors;
using OoBDev.Apache.Tika.Handlers;
using OoBDev.Documents;
using OoBDev.Documents.Conversion;
using OoBDev.Documents.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.Apache.Tika
{
    public class ApacheTikaHealthCheck : IHealthCheck
    {
        private IApacheTikaClient _client;

        public ApacheTikaHealthCheck(HttpClient client)
        {
            _client = client;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            // Logic for health check
        }
    }
}
```
**Class Diagram:**
```
@startuml
class ApacheTikaHealthCheck {
  private IApacheTikaClient _client

  +CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken)
}
@enduml
```
**OoBDev.Apache.Tika.csproj**
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>false</ImplicitUsings>
    <Nullable>enable</Nullable>
    <GenerateDocumentationFile>True</GenerateDocumentationFile>
    <GenerateAssemblyInfo>true</GenerateAssemblyInfo>

    <NoWarn>1701;1702</NoWarn>
    <WarningLevel>0</WarningLevel>

    <PackageReadmeFile>Readme.Apache.Tika.md</PackageReadmeFile>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.Extensions.Configuration.Abstractions" Version="8.0.0" />
    <PackageReference Include="Microsoft.Extensions.DependencyInjection.Abstractions" Version="8.0.1" />
    <PackageReference Include="Microsoft.Extensions.Diagnostics.HealthChecks" Version="8.0.7" />
    <PackageReference Include="Microsoft.Extensions.Logging.Abstractions" Version="8.0.1" />
    <PackageReference Include="Microsoft.Extensions.Options.ConfigurationExtensions" Version="8.0.0" />
    <PackageReference Include="Microsoft.Extensions.Http" Version="8.0.0" />
  </ItemGroup>

  <ItemGroup>
    <InternalsVisibleTo Include="OoBDev.Apache.Tika.Tests" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\..\Framework\OoBDev.Documents.Abstractions\OoBDev.Documents.Abstractions.csproj" />
  </ItemGroup>
</Project>
```
**IApacheTikaClient.cs**
```csharp
using System.IO;
using System.Threading.Tasks;

namespace OoBDev.Apache.Tika
{
    public interface IApacheTikaClient
    {
        ValueTask<string> DetectStreamAsync(Stream source);
        Task Convert