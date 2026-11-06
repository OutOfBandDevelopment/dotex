# Data Source Providers - Requirements Specification

**Feature:** Data Source Providers
**Epic:** 05 - Master Data & Test Data Management
**Status:** Proposed
**Last Updated:** 2026-01-22

---

## Overview

Data Source Providers implement pluggable data sources for the Master Data Loader, enabling loading from JSON, CSV, XML, SQL databases, and REST APIs. Each provider follows a consistent interface while handling format-specific parsing and streaming.

---

## Business Requirements

### BR-1: Multi-Format Support
**Priority:** P0 (Critical)
**Description:** System must load data from multiple file formats and sources

**Acceptance Criteria:**
- JSON file support (local and remote)
- CSV file support with configurable delimiters
- XML file support with XPath queries
- SQL database query support
- REST API support with pagination

**Business Value:**
- Flexibility in data source selection
- Integration with existing data systems
- Support for legacy formats

---

### BR-2: Streaming Support
**Priority:** P0 (Critical)
**Description:** System must support streaming large data sets

**Acceptance Criteria:**
- Memory-efficient streaming (< 100 MB for 1M records)
- Async enumerable support
- Backpressure handling
- Configurable batch sizes

**Business Value:**
- Handle large data sets without memory issues
- Faster processing with streaming
- Better resource utilization

---

### BR-3: Format Detection
**Priority:** P1 (High)
**Description:** System must automatically detect file formats

**Acceptance Criteria:**
- Detect format by file extension
- Detect format by content inspection
- Fallback to explicit configuration
- Support custom format registration

**Business Value:**
- Reduced configuration overhead
- Better developer experience
- Fewer errors from format mismatches

---

## Functional Requirements

### FR-1: JSON Provider
**Priority:** P0 (Critical)

**Description:** Load data from JSON files and strings

**Requirements:**
- FR-1.1: Parse JSON files
- FR-1.2: Parse JSON strings
- FR-1.3: Support nested objects
- FR-1.4: Support arrays
- FR-1.5: Stream large JSON files
- FR-1.6: Handle UTF-8, UTF-16, UTF-32 encodings
- FR-1.7: JSON Path queries

**Acceptance Criteria:**
```csharp
var provider = new JsonDataSourceProvider();
var dataSet = await provider.LoadAsync("countries.json");

Assert.IsTrue(dataSet.Rows.Count > 0);
Assert.IsTrue(dataSet.Rows.All(r => r.ContainsKey("Code")));
```

---

### FR-2: CSV Provider
**Priority:** P0 (Critical)

**Description:** Load data from CSV files

**Requirements:**
- FR-2.1: Parse CSV files
- FR-2.2: Configurable delimiter (comma, pipe, tab, etc.)
- FR-2.3: Handle quoted fields
- FR-2.4: Handle header row
- FR-2.5: Custom column mapping
- FR-2.6: Type inference
- FR-2.7: Stream large CSV files

**Acceptance Criteria:**
```csharp
var provider = new CsvDataSourceProvider();
var options = new CsvOptions
{
    Delimiter = '|',
    HasHeader = true
};

var dataSet = await provider.LoadAsync("products.csv", options);

Assert.IsTrue(dataSet.Rows.Count > 0);
```

---

### FR-3: XML Provider
**Priority:** P1 (High)

**Description:** Load data from XML files

**Requirements:**
- FR-3.1: Parse XML files
- FR-3.2: XPath query support
- FR-3.3: Namespace handling
- FR-3.4: Attribute mapping
- FR-3.5: Element mapping
- FR-3.6: Stream large XML files

**Acceptance Criteria:**
```csharp
var provider = new XmlDataSourceProvider();
var options = new XmlOptions
{
    RootPath = "/catalog/product",
    AttributeMapping = new Dictionary<string, string>
    {
        ["id"] = "ProductId"
    }
};

var dataSet = await provider.LoadAsync("catalog.xml", options);
Assert.IsTrue(dataSet.Rows.Count > 0);
```

---

### FR-4: SQL Provider
**Priority:** P1 (High)

**Description:** Load data from SQL databases

**Requirements:**
- FR-4.1: Execute SELECT queries
- FR-4.2: Support parameterized queries
- FR-4.3: Multiple database support (SQL Server, PostgreSQL, MySQL, Oracle)
- FR-4.4: Connection string configuration
- FR-4.5: Stream result sets
- FR-4.6: Transaction support

**Acceptance Criteria:**
```csharp
var provider = new SqlDataSourceProvider();
var options = new SqlOptions
{
    ConnectionString = "Server=...;Database=...;",
    Query = "SELECT Code, Name FROM Countries WHERE Active = 1"
};

var dataSet = await provider.LoadAsync(options.Query, options);
Assert.IsTrue(dataSet.Rows.Count > 0);
```

---

### FR-5: API Provider
**Priority:** P1 (High)

**Description:** Load data from REST APIs

**Requirements:**
- FR-5.1: HTTP GET requests
- FR-5.2: HTTP POST requests with body
- FR-5.3: Authentication (Bearer, Basic, API Key)
- FR-5.4: Pagination support (offset, cursor, page-based)
- FR-5.5: Rate limiting
- FR-5.6: Retry logic
- FR-5.7: Response parsing (JSON, XML)

**Acceptance Criteria:**
```csharp
var provider = new ApiDataSourceProvider();
var options = new ApiOptions
{
    BaseUrl = "https://api.example.com",
    Endpoint = "/v1/countries",
    AuthType = AuthenticationType.Bearer,
    Token = "your-token",
    PaginationStrategy = PaginationStrategy.Offset
};

var dataSet = await provider.LoadAsync(options.Endpoint, options);
Assert.IsTrue(dataSet.Rows.Count > 0);
```

---

## Non-Functional Requirements

### NFR-1: Performance
**Priority:** P0 (Critical)

**Requirements:**
- Parse 10,000 records in < 2 seconds
- Stream 1M records with < 100 MB memory
- Parallel processing support
- Efficient encoding detection

**Measurement:**
```csharp
var sw = Stopwatch.StartNew();
var dataSet = await provider.LoadAsync("large-file.json");
sw.Stop();

Assert.IsTrue(sw.Elapsed.TotalSeconds < 5);
```

---

### NFR-2: Reliability
**Priority:** P0 (Critical)

**Requirements:**
- Handle malformed data gracefully
- Retry transient failures (network, database)
- Clear error messages
- Partial success support

---

### NFR-3: Extensibility
**Priority:** P1 (High)

**Requirements:**
- Plugin architecture for custom providers
- Override built-in providers
- Custom format parsers
- Middleware support

---

## Data Requirements

### DR-1: Supported Encodings
- UTF-8 (default)
- UTF-16 (LE/BE)
- UTF-32 (LE/BE)
- ASCII
- ISO-8859-1 (Latin-1)
- BOM detection

### DR-2: Supported Formats

**JSON:**
- JSON Lines (`.jsonl`)
- Standard JSON arrays
- Nested JSON objects

**CSV:**
- RFC 4180 compliant
- Custom delimiters
- Quoted fields with embedded delimiters

**XML:**
- Well-formed XML
- Namespaces
- Attributes and elements

---

## Integration Requirements

### IR-1: Master Data Loader Integration
**Priority:** P0 (Critical)

**Requirements:**
- IDataSourceProvider interface
- Factory registration
- Configuration binding

---

### IR-2: Logging and Telemetry
**Priority:** P1 (High)

**Requirements:**
- ILogger integration
- OpenTelemetry support
- Performance metrics

---

## Constraints

### Technical Constraints
- Target framework: net10.0
- Nullable reference types enabled
- No implicit usings
- Follow OoBDev architectural patterns

### Business Constraints
- Must handle production-scale data
- Must be secure (no injection attacks)
- Must support air-gapped environments

---

## Success Metrics

1. **Format Support**: 5+ formats supported
2. **Performance**: 10K records in < 2 seconds
3. **Reliability**: 99.9% success rate
4. **Code Quality**: ≥ 85% test coverage

---

## Open Questions

1. **Q:** Should we support Excel files?
   **A:** Phase 2 - convert to CSV first

2. **Q:** How do we handle schema evolution?
   **A:** Version-aware parsing with fallbacks

3. **Q:** Should we support write operations?
   **A:** No, read-only for v1

---

## Dependencies

### Internal Dependencies
- OoBDev.Framework.Data.MasterData
- OoBDev.Framework.Data.Abstractions

### External Dependencies
- System.Text.Json (≥ 10.0.0)
- CsvHelper (≥ 33.0.0)
- System.Xml.Linq (≥ 10.0.0)
- System.Data.SqlClient (≥ 5.0.0)

---

## References

- Epic 05: Master Data & Test Data Management
- Feature: Master Data Loader
- Feature: Test Data Loader
