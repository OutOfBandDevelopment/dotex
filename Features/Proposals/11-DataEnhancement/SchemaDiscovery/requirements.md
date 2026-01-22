# Schema Discovery & Translation - Requirements

**Epic:** 11 - Data Enhancement Pipeline
**Feature:** Schema Discovery & Translation
**Priority:** MEDIUM
**Complexity:** MEDIUM-HIGH
**Estimated LOC:** ~400

---

## Overview

Provides schema introspection and translation capabilities for data containers. Calling services can request the schema of a data model and translate between different schema formats (XSD, JSON Schema, YAML Schema, etc.).

---

## Business Requirements

### BR-1: Schema Introspection
**As a** calling service
**I want** to request the schema of a data container
**So that** I can understand the structure, types, and constraints of the data

**Acceptance Criteria:**
- Get schema for entire data container
- Get schema for specific path (e.g., just "Customer" schema)
- Schema includes: property names, types, constraints, descriptions
- Schema reflects registered providers' data structures

---

### BR-2: Multiple Schema Formats
**As a** developer
**I want** to work with different schema formats based on my needs
**So that** I can use XSD for XML scenarios, JSON Schema for APIs, etc.

**Acceptance Criteria:**
- Support XSD (XML Schema Definition)
- Support JSON Schema (draft-07 or later)
- Support YAML Schema
- Support OpenAPI Schema (subset of JSON Schema)
- Extensible to add custom schema formats

---

### BR-3: Schema Translation
**As a** developer
**I want** to translate schemas between different formats
**So that** I can use XSD schemas with JSON data (or vice versa)

**Acceptance Criteria:**
- Translate XSD ↔ JSON Schema
- Translate XSD ↔ YAML Schema
- Translate JSON Schema ↔ YAML Schema
- Translate Custom ↔ Canonical Schema
- Preserve metadata during translation

---

### BR-4: Runtime Schema Generation
**As a** data container
**I want** to generate schema from registered providers
**So that** schema reflects actual data structure without manual definition

**Acceptance Criteria:**
- Infer schema from provider data
- Combine schemas from multiple providers
- Support dynamic schemas (data-driven)
- Cache generated schemas

---

### BR-5: Schema Metadata
**As a** developer
**I want** rich metadata in schemas
**So that** I can generate UIs, validate data, and document APIs

**Acceptance Criteria:**
- Property descriptions
- Data type information (string, int, object, array, etc.)
- Constraints (required, min/max length, patterns, enums)
- Default values
- Examples
- Custom extensions

---

## Technical Requirements

### TR-1: Schema Provider Abstraction

**Multiple schema formats via provider pattern:**

```csharp
/// <summary>
/// Schema provider abstraction.
/// Implementations: XsdSchemaProvider, JsonSchemaProvider, YamlSchemaProvider, etc.
/// </summary>
public interface ISchemaProvider
{
    /// <summary>
    /// Schema format identifier (e.g., "xsd", "jsonschema", "yamlschema").
    /// </summary>
    string SchemaFormat { get; }

    /// <summary>
    /// Parses schema string into canonical representation.
    /// </summary>
    ICanonicalSchema Parse(string schemaContent);

    /// <summary>
    /// Formats canonical schema to this format.
    /// </summary>
    string Format(ICanonicalSchema schema);

    /// <summary>
    /// Validates data against schema.
    /// </summary>
    ValidationResult Validate(object data, ICanonicalSchema schema);
}
```

---

### TR-2: Canonical Schema Structure

**Syntax-agnostic internal representation:**

```csharp
/// <summary>
/// Canonical schema representation (format-agnostic).
/// </summary>
public interface ICanonicalSchema
{
    /// <summary>
    /// Schema title/name.
    /// </summary>
    string? Title { get; }

    /// <summary>
    /// Schema description.
    /// </summary>
    string? Description { get; }

    /// <summary>
    /// Root type definition.
    /// </summary>
    ISchemaType RootType { get; }

    /// <summary>
    /// Named type definitions (reusable types).
    /// </summary>
    IReadOnlyDictionary<string, ISchemaType> Definitions { get; }

    /// <summary>
    /// Schema metadata/extensions.
    /// </summary>
    IReadOnlyDictionary<string, object> Metadata { get; }
}

/// <summary>
/// Schema type definition.
/// </summary>
public interface ISchemaType
{
    /// <summary>
    /// Type name (e.g., "Customer", "Order").
    /// </summary>
    string? Name { get; }

    /// <summary>
    /// Primitive type (string, integer, number, boolean, object, array, null).
    /// </summary>
    SchemaDataType DataType { get; }

    /// <summary>
    /// Type description.
    /// </summary>
    string? Description { get; }

    /// <summary>
    /// Properties (for object types).
    /// </summary>
    IReadOnlyDictionary<string, ISchemaProperty> Properties { get; }

    /// <summary>
    /// Array item type (for array types).
    /// </summary>
    ISchemaType? ItemType { get; }

    /// <summary>
    /// Constraints (required, min/max, pattern, etc.).
    /// </summary>
    ISchemaConstraints Constraints { get; }

    /// <summary>
    /// Default value.
    /// </summary>
    object? DefaultValue { get; }

    /// <summary>
    /// Example values.
    /// </summary>
    IReadOnlyList<object> Examples { get; }

    /// <summary>
    /// Custom metadata/extensions.
    /// </summary>
    IReadOnlyDictionary<string, object> Metadata { get; }
}

public interface ISchemaProperty
{
    string Name { get; }
    ISchemaType Type { get; }
    bool Required { get; }
    string? Description { get; }
}

public interface ISchemaConstraints
{
    bool? Required { get; }
    int? MinLength { get; }
    int? MaxLength { get; }
    decimal? Minimum { get; }
    decimal? Maximum { get; }
    string? Pattern { get; }
    IReadOnlyList<object>? Enum { get; }
    string? Format { get; }  // date-time, email, uri, etc.
}

public enum SchemaDataType
{
    String,
    Integer,
    Number,
    Boolean,
    Object,
    Array,
    Null,
    Any
}
```

---

### TR-3: Schema Discovery Service

**API for schema introspection:**

```csharp
/// <summary>
/// Discovers and provides schemas for data containers.
/// </summary>
public interface ISchemaDiscoveryService
{
    /// <summary>
    /// Gets schema for entire data container.
    /// </summary>
    Task<ICanonicalSchema> GetSchemaAsync(IDataContainer container);

    /// <summary>
    /// Gets schema for specific path in container.
    /// </summary>
    Task<ICanonicalSchema> GetSchemaAsync(IDataContainer container, string path);

    /// <summary>
    /// Infers schema from data object.
    /// </summary>
    ICanonicalSchema InferSchema(object data);

    /// <summary>
    /// Registers schema for path pattern.
    /// </summary>
    void RegisterSchema(string pathPattern, ICanonicalSchema schema);

    /// <summary>
    /// Gets schema in specific format.
    /// </summary>
    Task<string> GetSchemaAsAsync(IDataContainer container, string format);
}
```

---

### TR-4: Schema Translation Service

**Translate between schema formats:**

```csharp
/// <summary>
/// Translates schemas between different formats.
/// </summary>
public interface ISchemaTranslationService
{
    /// <summary>
    /// Registers schema provider.
    /// </summary>
    void RegisterProvider(ISchemaProvider provider);

    /// <summary>
    /// Translates schema from one format to another.
    /// </summary>
    string Translate(string schemaContent, string sourceFormat, string targetFormat);

    /// <summary>
    /// Parses schema in any registered format.
    /// </summary>
    ICanonicalSchema Parse(string schemaContent, string format);

    /// <summary>
    /// Formats canonical schema to specific format.
    /// </summary>
    string Format(ICanonicalSchema schema, string targetFormat);

    /// <summary>
    /// Gets schema provider by format.
    /// </summary>
    ISchemaProvider? GetProvider(string format);
}
```

---

### TR-5: Schema Format Examples

**XSD (XML Schema):**
```xml
<xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema">
  <xs:element name="Customer">
    <xs:complexType>
      <xs:sequence>
        <xs:element name="FirstName" type="xs:string"/>
        <xs:element name="LastName" type="xs:string"/>
        <xs:element name="Email" type="xs:string"/>
        <xs:element name="Address" type="AddressType"/>
      </xs:sequence>
    </xs:complexType>
  </xs:element>

  <xs:complexType name="AddressType">
    <xs:sequence>
      <xs:element name="Street" type="xs:string"/>
      <xs:element name="City" type="xs:string"/>
      <xs:element name="State" type="xs:string"/>
      <xs:element name="Zip" type="xs:string"/>
    </xs:sequence>
  </xs:complexType>
</xs:schema>
```

**JSON Schema:**
```json
{
  "$schema": "http://json-schema.org/draft-07/schema#",
  "title": "Customer",
  "type": "object",
  "properties": {
    "FirstName": { "type": "string" },
    "LastName": { "type": "string" },
    "Email": { "type": "string", "format": "email" },
    "Address": {
      "type": "object",
      "properties": {
        "Street": { "type": "string" },
        "City": { "type": "string" },
        "State": { "type": "string", "minLength": 2, "maxLength": 2 },
        "Zip": { "type": "string", "pattern": "^\\d{5}$" }
      },
      "required": ["Street", "City", "State", "Zip"]
    }
  },
  "required": ["FirstName", "LastName", "Email"]
}
```

**YAML Schema:**
```yaml
title: Customer
type: object
properties:
  FirstName:
    type: string
  LastName:
    type: string
  Email:
    type: string
    format: email
  Address:
    type: object
    properties:
      Street:
        type: string
      City:
        type: string
      State:
        type: string
        minLength: 2
        maxLength: 2
      Zip:
        type: string
        pattern: '^\d{5}$'
    required:
      - Street
      - City
      - State
      - Zip
required:
  - FirstName
  - LastName
  - Email
```

**Canonical Representation (Internal):**
```csharp
new CanonicalSchema
{
    Title = "Customer",
    RootType = new SchemaType
    {
        DataType = SchemaDataType.Object,
        Properties = new Dictionary<string, ISchemaProperty>
        {
            ["FirstName"] = new SchemaProperty
            {
                Name = "FirstName",
                Type = new SchemaType { DataType = SchemaDataType.String },
                Required = true
            },
            ["LastName"] = new SchemaProperty
            {
                Name = "LastName",
                Type = new SchemaType { DataType = SchemaDataType.String },
                Required = true
            },
            ["Email"] = new SchemaProperty
            {
                Name = "Email",
                Type = new SchemaType
                {
                    DataType = SchemaDataType.String,
                    Constraints = new SchemaConstraints { Format = "email" }
                },
                Required = true
            },
            ["Address"] = new SchemaProperty
            {
                Name = "Address",
                Type = new SchemaType
                {
                    DataType = SchemaDataType.Object,
                    Properties = new Dictionary<string, ISchemaProperty>
                    {
                        ["Street"] = new SchemaProperty { Name = "Street", Type = StringType, Required = true },
                        ["City"] = new SchemaProperty { Name = "City", Type = StringType, Required = true },
                        ["State"] = new SchemaProperty
                        {
                            Name = "State",
                            Type = new SchemaType
                            {
                                DataType = SchemaDataType.String,
                                Constraints = new SchemaConstraints { MinLength = 2, MaxLength = 2 }
                            },
                            Required = true
                        },
                        ["Zip"] = new SchemaProperty
                        {
                            Name = "Zip",
                            Type = new SchemaType
                            {
                                DataType = SchemaDataType.String,
                                Constraints = new SchemaConstraints { Pattern = @"^\d{5}$" }
                            },
                            Required = true
                        }
                    }
                },
                Required = false
            }
        }
    }
}
```

---

### TR-6: Runtime Schema Inference

**Generate schema from data:**

```csharp
public class SchemaInferenceService
{
    public ICanonicalSchema InferSchema(object data)
    {
        var type = InferType(data);
        return new CanonicalSchema
        {
            RootType = type
        };
    }

    private ISchemaType InferType(object? value)
    {
        if (value == null)
            return new SchemaType { DataType = SchemaDataType.Null };

        var type = value.GetType();

        // Primitives
        if (type == typeof(string))
            return new SchemaType { DataType = SchemaDataType.String };
        if (type == typeof(int) || type == typeof(long))
            return new SchemaType { DataType = SchemaDataType.Integer };
        if (type == typeof(decimal) || type == typeof(double) || type == typeof(float))
            return new SchemaType { DataType = SchemaDataType.Number };
        if (type == typeof(bool))
            return new SchemaType { DataType = SchemaDataType.Boolean };

        // Array/Collection
        if (value is IEnumerable enumerable and not string)
        {
            var firstItem = enumerable.Cast<object>().FirstOrDefault();
            return new SchemaType
            {
                DataType = SchemaDataType.Array,
                ItemType = firstItem != null ? InferType(firstItem) : null
            };
        }

        // Object
        var properties = new Dictionary<string, ISchemaProperty>();
        foreach (var prop in type.GetProperties())
        {
            properties[prop.Name] = new SchemaProperty
            {
                Name = prop.Name,
                Type = InferType(prop.GetValue(value)),
                Required = !IsNullable(prop.PropertyType)
            };
        }

        return new SchemaType
        {
            Name = type.Name,
            DataType = SchemaDataType.Object,
            Properties = properties
        };
    }
}
```

---

## Use Cases

### UC-1: UI Generation from Schema

```csharp
// Service requests schema
var schema = await _schemaDiscovery.GetSchemaAsync(container, "Customer");

// Generate form fields from schema
foreach (var prop in schema.RootType.Properties.Values)
{
    var field = new FormField
    {
        Name = prop.Name,
        Label = prop.Description ?? prop.Name,
        Type = MapToInputType(prop.Type.DataType),
        Required = prop.Required,
        Validation = MapConstraints(prop.Type.Constraints)
    };

    form.AddField(field);
}
```

---

### UC-2: API Documentation

```csharp
// Generate OpenAPI schema from data container
var container = DataContainerFactory.Create();
container.RegisterProvider("Customer", customerProvider);

// Get JSON Schema for OpenAPI
var jsonSchema = await _schemaDiscovery.GetSchemaAsAsync(container, "jsonschema");

// Add to OpenAPI spec
openApiSpec.Components.Schemas["Customer"] = JsonSerializer.Deserialize<JsonElement>(jsonSchema);
```

---

### UC-3: Data Validation

```csharp
// Get schema
var schema = await _schemaDiscovery.GetSchemaAsync(container, "Order");

// Validate data against schema
var jsonSchemaProvider = _schemaTranslation.GetProvider("jsonschema");
var validationResult = jsonSchemaProvider.Validate(orderData, schema);

if (!validationResult.IsValid)
{
    foreach (var error in validationResult.Errors)
    {
        Console.WriteLine($"{error.PropertyName}: {error.ErrorMessage}");
    }
}
```

---

### UC-4: Schema Translation

```csharp
// Have XSD, need JSON Schema
var xsdContent = File.ReadAllText("customer.xsd");

// Translate XSD → JSON Schema
var jsonSchemaContent = _schemaTranslation.Translate(xsdContent, "xsd", "jsonschema");

// Use JSON Schema with API
File.WriteAllText("customer.json-schema", jsonSchemaContent);
```

---

### UC-5: Dynamic Schema from Providers

```csharp
// Register providers with schema metadata
var customerProvider = new CustomerDatabaseProvider(_repo);
customerProvider.Schema = new CanonicalSchema
{
    Title = "Customer",
    RootType = new SchemaType
    {
        DataType = SchemaDataType.Object,
        Properties = new Dictionary<string, ISchemaProperty>
        {
            ["FirstName"] = new SchemaProperty { Name = "FirstName", Type = StringType, Required = true },
            ["LastName"] = new SchemaProperty { Name = "LastName", Type = StringType, Required = true }
        }
    }
};

container.RegisterProvider("Customer", customerProvider);

// Discovery service combines schemas from all providers
var fullSchema = await _schemaDiscovery.GetSchemaAsync(container);
// fullSchema includes Customer schema + all other registered provider schemas
```

---

## Non-Functional Requirements

### NFR-1: Performance
- Schema inference: < 50ms for typical objects
- Schema translation: < 100ms per schema
- Schema caching for repeated requests

### NFR-2: Compatibility
- XSD 1.1 support
- JSON Schema Draft-07 or later
- YAML 1.2 support
- OpenAPI 3.0+ compatibility

### NFR-3: Extensibility
- Custom schema formats via ISchemaProvider
- Custom constraints
- Custom metadata/extensions

---

## Constraints

### C-1: Translation Limitations
- Some XSD features may not map to JSON Schema (e.g., xs:choice)
- Some JSON Schema features may not map to XSD (e.g., oneOf, anyOf)
- Translation may be lossy for complex schemas

### C-2: Inference Limitations
- Inferred schemas are best-effort (may not capture all constraints)
- Nullable/optional detection based on .NET type system
- Enum detection requires inspection of actual values

---

## Success Criteria

- ✅ Schema introspection for data containers
- ✅ Three schema providers: XSD, JSON Schema, YAML Schema
- ✅ Bidirectional translation between formats
- ✅ Runtime schema inference from data
- ✅ Schema validation integration
- ✅ 80%+ test coverage

---

## Out of Scope

- ❌ Full XSD 1.1 spec (focus on common subset)
- ❌ Full JSON Schema spec (focus on common features)
- ❌ Schema evolution/versioning
- ❌ Schema registry service

---

## Dependencies

### Internal
- Core Container & Navigation (Epic 11)
- Injectable Validation (Revision 13)

### External
- .NET 10.0 BCL
- System.Xml.Schema (for XSD)
- Json.NET or System.Text.Json.Nodes (for JSON Schema)
- YamlDotNet (for YAML Schema)

---

## Integration with Other Features

### Data Container Integration
```csharp
public interface IDataContainer
{
    // Existing members...

    /// <summary>
    /// Gets schema for this container.
    /// </summary>
    Task<ICanonicalSchema> GetSchemaAsync();

    /// <summary>
    /// Gets schema for specific path.
    /// </summary>
    Task<ICanonicalSchema> GetSchemaAsync(string path);

    /// <summary>
    /// Gets schema in specific format.
    /// </summary>
    Task<string> GetSchemaAsAsync(string format);
}
```

### Data Provider Integration
```csharp
public interface IDataProvider
{
    // Existing members...

    /// <summary>
    /// Gets schema for data provided by this provider (optional).
    /// </summary>
    ICanonicalSchema? Schema { get; }
}
```

---

## Related Documents

- [Architecture](./architecture.md)
- [API Design](./api-design.md)
- [Testing Strategy](./testing-strategy.md)
- [Core Container Feature](../CoreContainer/requirements.md)
- [Injectable Validation](../../REVISIONS_SUMMARY.md#revision-13-injectable-validation-with-provider-pattern)
