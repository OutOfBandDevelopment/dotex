# Data Source Providers - Architecture Design

**Feature:** Data Source Providers
**Epic:** 05 - Master Data & Test Data Management
**Status:** Proposed
**Last Updated:** 2026-01-22

---

## Overview

Data Source Providers implement a plugin architecture for loading data from various sources (JSON, CSV, XML, SQL, APIs). Each provider implements `IDataSourceProvider` and handles format-specific parsing, streaming, and error handling.

---

## Architectural Principles

1. **Provider Pattern**: Pluggable data sources
2. **Strategy Pattern**: Different parsing strategies per format
3. **Streaming First**: Memory-efficient for large data sets
4. **Factory Pattern**: Automatic provider selection
5. **Single Responsibility**: One provider per format

---

## System Context

```
┌─────────────────────────────────────────────────────────┐
│               Master Data Loader                         │
└─────────────────────┬───────────────────────────────────┘
                      │
                      │ Uses
                      ▼
┌─────────────────────────────────────────────────────────┐
│          Data Source Provider Factory                    │
└─────────────────────┬───────────────────────────────────┘
                      │
                      │ Selects
                      ▼
┌─────────────────────────────────────────────────────────┐
│  IDataSourceProvider Implementations                     │
│  ├── JsonDataSourceProvider                            │
│  ├── CsvDataSourceProvider                             │
│  ├── XmlDataSourceProvider                             │
│  ├── SqlDataSourceProvider                             │
│  └── ApiDataSourceProvider                             │
└─────────────────────┬───────────────────────────────────┘
                      │
                      │ Reads from
                      ▼
┌─────────────────────────────────────────────────────────┐
│              External Data Sources                       │
│  (Files, Databases, APIs)                               │
└─────────────────────────────────────────────────────────┘
```

---

## Component Architecture

```
OoBDev.Framework.Data.MasterData.Providers/
├── Abstractions/
│   ├── IDataSourceProvider.cs
│   ├── IDataSourceProviderFactory.cs
│   └── DataSourceProviderBase.cs
├── Json/
│   ├── JsonDataSourceProvider.cs
│   ├── JsonStreamParser.cs
│   └── JsonOptions.cs
├── Csv/
│   ├── CsvDataSourceProvider.cs
│   ├── CsvStreamParser.cs
│   └── CsvOptions.cs
├── Xml/
│   ├── XmlDataSourceProvider.cs
│   ├── XmlStreamParser.cs
│   └── XmlOptions.cs
├── Sql/
│   ├── SqlDataSourceProvider.cs
│   ├── SqlStreamReader.cs
│   └── SqlOptions.cs
├── Api/
│   ├── ApiDataSourceProvider.cs
│   ├── PaginationStrategies/
│   │   ├── OffsetPagination.cs
│   │   ├── CursorPagination.cs
│   │   └── PageBasedPagination.cs
│   └── ApiOptions.cs
├── Factory/
│   ├── DataSourceProviderFactory.cs
│   └── ProviderRegistry.cs
└── Extensions/
    └── ServiceCollectionExtensions.cs
```

---

## Core Components

### 1. IDataSourceProvider

```csharp
public interface IDataSourceProvider
{
    string SourceType { get; }
    bool CanHandle(string source);
    Task<DataSet> LoadAsync(string source, CancellationToken ct = default);
    Task<DataSet> LoadAsync(string source, DataSourceOptions options, CancellationToken ct = default);
    IAsyncEnumerable<DataRow> StreamAsync(string source, CancellationToken ct = default);
}

public abstract class DataSourceProviderBase : IDataSourceProvider
{
    protected ILogger Logger { get; }

    public abstract string SourceType { get; }
    public abstract bool CanHandle(string source);

    public virtual async Task<DataSet> LoadAsync(string source, CancellationToken ct = default)
    {
        var rows = new List<DataRow>();
        await foreach (var row in StreamAsync(source, ct))
        {
            rows.Add(row);
        }
        return new DataSet(rows);
    }

    public abstract IAsyncEnumerable<DataRow> StreamAsync(
        string source,
        CancellationToken ct = default);
}
```

---

### 2. JsonDataSourceProvider

```csharp
public class JsonDataSourceProvider : DataSourceProviderBase
{
    public override string SourceType => "json";

    public override bool CanHandle(string source)
    {
        return source.EndsWith(".json", StringComparison.OrdinalIgnoreCase) ||
               source.EndsWith(".jsonl", StringComparison.OrdinalIgnoreCase);
    }

    public override async IAsyncEnumerable<DataRow> StreamAsync(
        string source,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        using var stream = File.OpenRead(source);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: ct);

        var root = document.RootElement;

        if (root.ValueKind == JsonValueKind.Array)
        {
            foreach (var element in root.EnumerateArray())
            {
                ct.ThrowIfCancellationRequested();
                yield return ParseJsonElement(element);
            }
        }
        else
        {
            yield return ParseJsonElement(root);
        }
    }

    private DataRow ParseJsonElement(JsonElement element)
    {
        var row = new DataRow();

        foreach (var property in element.EnumerateObject())
        {
            row[property.Name] = ParseJsonValue(property.Value);
        }

        return row;
    }

    private object? ParseJsonValue(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number => element.TryGetInt64(out var l) ? l : element.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            JsonValueKind.Array => element.EnumerateArray().Select(ParseJsonValue).ToArray(),
            JsonValueKind.Object => ParseJsonElement(element),
            _ => element.GetRawText()
        };
    }
}
```

---

### 3. CsvDataSourceProvider

```csharp
public class CsvDataSourceProvider : DataSourceProviderBase
{
    public override string SourceType => "csv";

    public override bool CanHandle(string source)
    {
        return source.EndsWith(".csv", StringComparison.OrdinalIgnoreCase);
    }

    public override async IAsyncEnumerable<DataRow> StreamAsync(
        string source,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        using var reader = new StreamReader(source);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

        await csv.ReadAsync();
        csv.ReadHeader();
        var headers = csv.HeaderRecord;

        if (headers == null)
            yield break;

        while (await csv.ReadAsync())
        {
            ct.ThrowIfCancellationRequested();

            var row = new DataRow();
            foreach (var header in headers)
            {
                row[header] = csv.GetField(header);
            }

            yield return row;
        }
    }
}
```

---

### 4. XmlDataSourceProvider

```csharp
public class XmlDataSourceProvider : DataSourceProviderBase
{
    public override string SourceType => "xml";

    public override bool CanHandle(string source)
    {
        return source.EndsWith(".xml", StringComparison.OrdinalIgnoreCase);
    }

    public override async IAsyncEnumerable<DataRow> StreamAsync(
        string source,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var document = await Task.Run(() => XDocument.Load(source), ct);

        // Default: iterate over all child elements of root
        foreach (var element in document.Root!.Elements())
        {
            ct.ThrowIfCancellationRequested();
            yield return ParseXmlElement(element);
        }
    }

    private DataRow ParseXmlElement(XElement element)
    {
        var row = new DataRow();

        // Attributes
        foreach (var attribute in element.Attributes())
        {
            row[attribute.Name.LocalName] = attribute.Value;
        }

        // Elements
        foreach (var child in element.Elements())
        {
            row[child.Name.LocalName] = child.Value;
        }

        return row;
    }
}
```

---

### 5. SqlDataSourceProvider

```csharp
public class SqlDataSourceProvider : DataSourceProviderBase
{
    public override string SourceType => "sql";

    public override bool CanHandle(string source)
    {
        return source.StartsWith("SELECT", StringComparison.OrdinalIgnoreCase);
    }

    public override async IAsyncEnumerable<DataRow> StreamAsync(
        string source,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var options = GetSqlOptions();
        using var connection = new SqlConnection(options.ConnectionString);
        await connection.OpenAsync(ct);

        using var command = new SqlCommand(source, connection);
        using var reader = await command.ExecuteReaderAsync(ct);

        while (await reader.ReadAsync(ct))
        {
            var row = new DataRow();

            for (int i = 0; i < reader.FieldCount; i++)
            {
                var name = reader.GetName(i);
                var value = reader.IsDBNull(i) ? null : reader.GetValue(i);
                row[name] = value;
            }

            yield return row;
        }
    }
}
```

---

### 6. ApiDataSourceProvider

```csharp
public class ApiDataSourceProvider : DataSourceProviderBase
{
    private readonly HttpClient _httpClient;

    public override string SourceType => "api";

    public override bool CanHandle(string source)
    {
        return source.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
               source.StartsWith("https://", StringComparison.OrdinalIgnoreCase);
    }

    public override async IAsyncEnumerable<DataRow> StreamAsync(
        string source,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var options = GetApiOptions();
        var url = source;
        int page = 0;

        while (true)
        {
            ct.ThrowIfCancellationRequested();

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            AddAuthentication(request, options);

            var response = await _httpClient.SendAsync(request, ct);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(ct);
            var document = JsonDocument.Parse(json);

            foreach (var element in ExtractDataElements(document.RootElement, options))
            {
                yield return ParseJsonElement(element);
            }

            // Pagination
            if (!TryGetNextPage(response, options, ref url, ref page))
                break;
        }
    }

    private bool TryGetNextPage(
        HttpResponseMessage response,
        ApiOptions options,
        ref string url,
        ref int page)
    {
        return options.PaginationStrategy switch
        {
            PaginationStrategy.Offset => TryOffsetPagination(ref url, ref page, options),
            PaginationStrategy.Cursor => TryCursorPagination(response, ref url),
            PaginationStrategy.PageBased => TryPageBasedPagination(ref url, ref page, options),
            _ => false
        };
    }
}
```

---

## Factory Pattern

### DataSourceProviderFactory

```csharp
public class DataSourceProviderFactory : IDataSourceProviderFactory
{
    private readonly IEnumerable<IDataSourceProvider> _providers;

    public DataSourceProviderFactory(IEnumerable<IDataSourceProvider> providers)
    {
        _providers = providers;
    }

    public IDataSourceProvider GetProvider(string source)
    {
        foreach (var provider in _providers)
        {
            if (provider.CanHandle(source))
                return provider;
        }

        throw new NotSupportedException($"No provider found for source: {source}");
    }
}
```

---

## Configuration

### Provider Options

```csharp
public class CsvOptions : DataSourceOptions
{
    public char Delimiter { get; set; } = ',';
    public bool HasHeader { get; set; } = true;
    public string Encoding { get; set; } = "utf-8";
}

public class XmlOptions : DataSourceOptions
{
    public string? RootPath { get; set; }
    public Dictionary<string, string> AttributeMapping { get; set; } = new();
}

public class SqlOptions : DataSourceOptions
{
    public string ConnectionString { get; set; } = string.Empty;
    public int CommandTimeout { get; set; } = 30;
}

public class ApiOptions : DataSourceOptions
{
    public string BaseUrl { get; set; } = string.Empty;
    public string Endpoint { get; set; } = string.Empty;
    public AuthenticationType AuthType { get; set; }
    public string? Token { get; set; }
    public PaginationStrategy PaginationStrategy { get; set; }
}
```

---

## Dependency Injection

```csharp
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDataSourceProviders(
        this IServiceCollection services)
    {
        services.TryAddSingleton<IDataSourceProvider, JsonDataSourceProvider>();
        services.TryAddSingleton<IDataSourceProvider, CsvDataSourceProvider>();
        services.TryAddSingleton<IDataSourceProvider, XmlDataSourceProvider>();
        services.TryAddSingleton<IDataSourceProvider, SqlDataSourceProvider>();
        services.TryAddSingleton<IDataSourceProvider, ApiDataSourceProvider>();

        services.TryAddSingleton<IDataSourceProviderFactory, DataSourceProviderFactory>();

        return services;
    }
}
```

---

## Performance Optimization

### Streaming

```csharp
// Memory-efficient streaming
await foreach (var row in provider.StreamAsync("large-file.json"))
{
    await ProcessRowAsync(row);
    // Memory released after each iteration
}
```

### Parallel Processing

```csharp
// Parallel loading with Channels
var channel = Channel.CreateBounded<DataRow>(1000);

var producerTask = Task.Run(async () =>
{
    await foreach (var row in provider.StreamAsync(source))
    {
        await channel.Writer.WriteAsync(row);
    }
    channel.Writer.Complete();
});

var consumerTask = Task.Run(async () =>
{
    await foreach (var row in channel.Reader.ReadAllAsync())
    {
        await ProcessRowAsync(row);
    }
});

await Task.WhenAll(producerTask, consumerTask);
```

---

## Error Handling

```csharp
public abstract class DataSourceProviderBase : IDataSourceProvider
{
    protected async Task<T> ExecuteWithRetryAsync<T>(
        Func<Task<T>> operation,
        int maxRetries = 3)
    {
        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                return await operation();
            }
            catch (Exception ex) when (IsTransient(ex) && i < maxRetries - 1)
            {
                Logger.LogWarning(ex, "Transient error on attempt {Attempt}", i + 1);
                await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, i)));
            }
        }

        throw new DataSourceException("Max retries exceeded");
    }

    private bool IsTransient(Exception ex)
    {
        return ex is HttpRequestException ||
               ex is SqlException sqlEx && sqlEx.Number < 0;
    }
}
```

---

## References

- Epic 05: Master Data & Test Data Management
- Feature: Master Data Loader
- Requirements: Data Source Providers Requirements
