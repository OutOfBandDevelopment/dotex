# OoBDev.OpenSearch

Registers a low-level OpenSearch client built from configuration. It gives other libraries (and your code) an `IOpenSearchLowLevelClient` for the OpenSearch container of the test stack or a real cluster; it adds no search model of its own.

## Use

```csharp
services.TryAddOpenSearchServices(configuration);                 // section OpenSearchOptions
services.TryAddOpenSearchServices(configuration, "MySearch");     // or a named section
```

```json
{
  "OpenSearchOptions": {
    "HostName": "localhost",
    "Port": 9200,
    "IndexName": "documents",
    "UserName": "admin",
    "Password": "..."
  }
}
```

In `DEBUG` builds the section name is a required argument (builder parameters are forwarded explicitly, rule `coding-standards`).

**Table 1 — `OpenSearchOptions`**

| Key | Meaning |
|-----|---------|
| `HostName` | Required |
| `Port` | Default 9200 |
| `IndexName` | Required; the index the consumers use |
| `UserName`, `Password` | Basic authentication, applied only when both are non-blank |

Options are validated by data annotations. `IOpenSearchClientFactory` (`OpenSearchClientFactory`, transient) creates the client, and `IOpenSearchLowLevelClient` is registered from it. The client connects over plain `http://host:port`, enables HTTP compression and throws on failed requests (`ThrowExceptions(true)`); TLS and certificate settings are not configurable yet.

`LexicalProvider` is an obsolete empty class (its body is commented out, kept until removal); do not use it.

## Tests

`OoBDev.OpenSearch.Tests` holds two `Integration` tests against the OpenSearch container (`OPENSEARCH_*` parameters in `.runsettings`).
