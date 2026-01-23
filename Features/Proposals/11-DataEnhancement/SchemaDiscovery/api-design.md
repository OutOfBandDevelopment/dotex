# Schema Discovery & Translation - API Design

**Epic:** 11 - Data Enhancement Pipeline
**Feature:** Schema Discovery & Translation
**Priority:** HIGH (Foundation)
**Complexity:** MEDIUM-HIGH

---

## Overview

Complete API surface for schema discovery, translation, and validation. Provides format-agnostic schema introspection using provider pattern similar to path translation.

---

## Core Interfaces

### ISchemaProvider

```csharp
namespace OoBDev.Framework.Data.Schema
{
    /// <summary>
    /// Schema provider abstraction for different schema formats.
    /// </summary>
    public interface ISchemaProvider
    {
        /// <summary>
        /// Schema format identifier (e.g., "xsd", "jsonschema", "yamlschema", "openapi").
        /// </summary>
        string SchemaFormat { get; }

        /// <summary>
        /// Parses schema document into canonical representation.
        /// </summary>
        /// <param name="schemaContent">Schema document content</param>
        /// <returns>Canonical schema representation</returns>
        /// <exception cref="SchemaParseException">Schema is invalid</exception>
        ICanonicalSchema Parse(string schemaContent);

        /// <summary>
        /// Formats canonical schema back to this format.
        /// </summary>
        /// <param name="schema">Canonical schema</param>
        /// <returns>Schema document in this format</returns>
        string Format(ICanonicalSchema schema);

        /// <summary>
        /// Validates data against canonical schema.
        /// </summary>
        /// <param name="data">Data to validate</param>
        /// <param name="schema">Schema to validate against</param>
        /// <returns>Validation result</returns>
        ValidationResult Validate(object data, ICanonicalSchema schema);

        /// <summary>
        /// Checks if content is parseable by this provider.
        /// </summary>
        /// <param name="content">Schema content</param>
        /// <returns>True if this provider can parse the content</returns>
        bool CanParse(string content);
    }
}
```

---

### ICanonicalSchema

```csharp
namespace OoBDev.Framework.Data.Schema
{
    /// <summary>
    /// Canonical internal schema representation (format-agnostic).
    /// </summary>
    public interface ICanonicalSchema
    {
        /// <summary>
        /// Schema title/name (optional).
        /// </summary>
        string? Title { get; }

        /// <summary>
        /// Schema description (optional).
        /// </summary>
        string? Description { get; }

        /// <summary>
        /// Root type definition.
        /// </summary>
        ISchemaType RootType { get; }

        /// <summary>
        /// Named type definitions (for $ref, complexType, etc.).
        /// Key: definition name, Value: type definition
        /// </summary>
        IReadOnlyDictionary<string, ISchemaType> Definitions { get; }

        /// <summary>
        /// Additional metadata (version, id, schema URI, etc.).
        /// </summary>
        IReadOnlyDictionary<string, object> Metadata { get; }

        /// <summary>
        /// Resolves a type reference (handles $ref, type names).
        /// </summary>
        /// <param name="reference">Reference path (e.g., "#/definitions/Customer")</param>
        /// <returns>Resolved schema type, or null if not found</returns>
        ISchemaType? ResolveReference(string reference);

        /// <summary>
        /// Creates a deep copy of this schema.
        /// </summary>
        ICanonicalSchema Clone();
    }
}
```

---

### ISchemaType Hierarchy

```csharp
namespace OoBDev.Framework.Data.Schema
{
    /// <summary>
    /// Base schema type interface.
    /// </summary>
    public interface ISchemaType
    {
        /// <summary>
        /// Type category.
        /// </summary>
        SchemaTypeKind Kind { get; }

        /// <summary>
        /// Type title (optional).
        /// </summary>
        string? Title { get; }

        /// <summary>
        /// Type description (optional).
        /// </summary>
        string? Description { get; }

        /// <summary>
        /// Is this type nullable?
        /// </summary>
        bool IsNullable { get; }

        /// <summary>
        /// Default value (optional).
        /// </summary>
        object? DefaultValue { get; }

        /// <summary>
        /// Example values.
        /// </summary>
        IReadOnlyList<object> Examples { get; }

        /// <summary>
        /// Additional metadata.
        /// </summary>
        IReadOnlyDictionary<string, object> Metadata { get; }

        /// <summary>
        /// Validates value against this type.
        /// </summary>
        ValidationResult Validate(object? value);

        /// <summary>
        /// Creates a deep copy of this type.
        /// </summary>
        ISchemaType Clone();
    }

    public enum SchemaTypeKind
    {
        Object,
        Array,
        String,
        Number,
        Integer,
        Boolean,
        Null,
        Reference,
        OneOf,
        AllOf,
        AnyOf
    }

    /// <summary>
    /// Object schema type.
    /// </summary>
    public interface IObjectSchemaType : ISchemaType
    {
        /// <summary>
        /// Property definitions.
        /// Key: property name, Value: property type
        /// </summary>
        IReadOnlyDictionary<string, ISchemaType> Properties { get; }

        /// <summary>
        /// Required property names.
        /// </summary>
        IReadOnlyList<string> Required { get; }

        /// <summary>
        /// Minimum properties.
        /// </summary>
        int? MinProperties { get; }

        /// <summary>
        /// Maximum properties.
        /// </summary>
        int? MaxProperties { get; }

        /// <summary>
        /// Additional properties allowed?
        /// </summary>
        bool AdditionalPropertiesAllowed { get; }

        /// <summary>
        /// Schema for additional properties (if allowed).
        /// </summary>
        ISchemaType? AdditionalPropertiesSchema { get; }

        /// <summary>
        /// Pattern properties (regex-based property schemas).
        /// Key: regex pattern, Value: property type
        /// </summary>
        IReadOnlyDictionary<string, ISchemaType> PatternProperties { get; }
    }

    /// <summary>
    /// Array schema type.
    /// </summary>
    public interface IArraySchemaType : ISchemaType
    {
        /// <summary>
        /// Item schema.
        /// </summary>
        ISchemaType ItemsSchema { get; }

        /// <summary>
        /// Minimum items.
        /// </summary>
        int? MinItems { get; }

        /// <summary>
        /// Maximum items.
        /// </summary>
        int? MaxItems { get; }

        /// <summary>
        /// Unique items required?
        /// </summary>
        bool UniqueItems { get; }

        /// <summary>
        /// Contains constraint (at least one item must match this schema).
        /// </summary>
        ISchemaType? ContainsSchema { get; }
    }

    /// <summary>
    /// String schema type.
    /// </summary>
    public interface IStringSchemaType : ISchemaType
    {
        /// <summary>
        /// Minimum length.
        /// </summary>
        int? MinLength { get; }

        /// <summary>
        /// Maximum length.
        /// </summary>
        int? MaxLength { get; }

        /// <summary>
        /// Regular expression pattern.
        /// </summary>
        string? Pattern { get; }

        /// <summary>
        /// Format hint (date-time, email, uri, uuid, etc.).
        /// </summary>
        string? Format { get; }

        /// <summary>
        /// Allowed enum values.
        /// </summary>
        IReadOnlyList<string>? EnumValues { get; }

        /// <summary>
        /// Content encoding (base64, etc.).
        /// </summary>
        string? ContentEncoding { get; }

        /// <summary>
        /// Content media type (application/json, etc.).
        /// </summary>
        string? ContentMediaType { get; }
    }

    /// <summary>
    /// Number schema type (includes integers and decimals).
    /// </summary>
    public interface INumberSchemaType : ISchemaType
    {
        /// <summary>
        /// Minimum value.
        /// </summary>
        decimal? Minimum { get; }

        /// <summary>
        /// Maximum value.
        /// </summary>
        decimal? Maximum { get; }

        /// <summary>
        /// Exclusive minimum?
        /// </summary>
        bool ExclusiveMinimum { get; }

        /// <summary>
        /// Exclusive maximum?
        /// </summary>
        bool ExclusiveMaximum { get; }

        /// <summary>
        /// Multiple of (for validation).
        /// </summary>
        decimal? MultipleOf { get; }

        /// <summary>
        /// Allowed enum values.
        /// </summary>
        IReadOnlyList<decimal>? EnumValues { get; }
    }

    /// <summary>
    /// Integer schema type.
    /// </summary>
    public interface IIntegerSchemaType : INumberSchemaType
    {
        // Inherits all number constraints
    }

    /// <summary>
    /// Boolean schema type.
    /// </summary>
    public interface IBooleanSchemaType : ISchemaType
    {
        // No additional constraints beyond base type
    }

    /// <summary>
    /// Null schema type.
    /// </summary>
    public interface INullSchemaType : ISchemaType
    {
        // No additional constraints
    }

    /// <summary>
    /// Reference schema type (for $ref, type references).
    /// </summary>
    public interface IReferenceSchemaType : ISchemaType
    {
        /// <summary>
        /// Reference path ($ref, type name, etc.).
        /// </summary>
        string Reference { get; }

        /// <summary>
        /// Resolved type (lazy loaded).
        /// </summary>
        ISchemaType? ResolvedType { get; }
    }

    /// <summary>
    /// OneOf schema type (exactly one schema must match).
    /// </summary>
    public interface IOneOfSchemaType : ISchemaType
    {
        /// <summary>
        /// Possible schemas (exactly one must match).
        /// </summary>
        IReadOnlyList<ISchemaType> Schemas { get; }
    }

    /// <summary>
    /// AllOf schema type (all schemas must match).
    /// </summary>
    public interface IAllOfSchemaType : ISchemaType
    {
        /// <summary>
        /// Required schemas (all must match).
        /// </summary>
        IReadOnlyList<ISchemaType> Schemas { get; }
    }

    /// <summary>
    /// AnyOf schema type (at least one schema must match).
    /// </summary>
    public interface IAnyOfSchemaType : ISchemaType
    {
        /// <summary>
        /// Possible schemas (at least one must match).
        /// </summary>
        IReadOnlyList<ISchemaType> Schemas { get; }
    }
}
```

---

### ISchemaDiscoveryService

```csharp
namespace OoBDev.Framework.Data.Schema
{
    /// <summary>
    /// Schema discovery and translation service.
    /// </summary>
    public interface ISchemaDiscoveryService
    {
        /// <summary>
        /// Registers schema provider.
        /// </summary>
        /// <param name="provider">Schema provider</param>
        void RegisterProvider(ISchemaProvider provider);

        /// <summary>
        /// Gets schema for entire data container.
        /// </summary>
        /// <param name="container">Data container</param>
        /// <returns>Canonical schema</returns>
        Task<ICanonicalSchema> GetSchemaAsync(IDataContainer container);

        /// <summary>
        /// Gets schema for specific path in container.
        /// </summary>
        /// <param name="container">Data container</param>
        /// <param name="path">Path to data</param>
        /// <returns>Canonical schema</returns>
        Task<ICanonicalSchema> GetSchemaAsync(IDataContainer container, string path);

        /// <summary>
        /// Infers schema from runtime data.
        /// </summary>
        /// <param name="data">Data instance</param>
        /// <returns>Canonical schema</returns>
        ICanonicalSchema InferSchema(object data);

        /// <summary>
        /// Infers schema from CLR type.
        /// </summary>
        /// <param name="type">CLR type</param>
        /// <returns>Canonical schema</returns>
        ICanonicalSchema InferSchemaFromType(Type type);

        /// <summary>
        /// Translates schema between formats.
        /// </summary>
        /// <param name="schemaContent">Source schema content</param>
        /// <param name="sourceFormat">Source format (e.g., "xsd")</param>
        /// <param name="targetFormat">Target format (e.g., "jsonschema")</param>
        /// <returns>Translated schema</returns>
        string TranslateSchema(string schemaContent, string sourceFormat, string targetFormat);

        /// <summary>
        /// Gets schema in specific format.
        /// </summary>
        /// <param name="container">Data container</param>
        /// <param name="format">Target format</param>
        /// <returns>Schema in target format</returns>
        Task<string> GetSchemaAsAsync(IDataContainer container, string format);

        /// <summary>
        /// Validates data against schema.
        /// </summary>
        /// <param name="data">Data to validate</param>
        /// <param name="schema">Schema to validate against</param>
        /// <returns>Validation result</returns>
        ValidationResult ValidateAgainstSchema(object data, ICanonicalSchema schema);

        /// <summary>
        /// Gets provider by format.
        /// </summary>
        /// <param name="format">Schema format</param>
        /// <returns>Schema provider, or null if not found</returns>
        ISchemaProvider? GetProvider(string format);

        /// <summary>
        /// Parses schema using any registered provider (auto-detect format).
        /// </summary>
        /// <param name="schemaContent">Schema content</param>
        /// <returns>Canonical schema</returns>
        ICanonicalSchema ParseAny(string schemaContent);
    }
}
```

---

### Validation Types

```csharp
namespace OoBDev.Framework.Data.Schema
{
    /// <summary>
    /// Schema validation result.
    /// </summary>
    public class ValidationResult
    {
        /// <summary>
        /// Is data valid?
        /// </summary>
        public bool IsValid => Errors.Count == 0;

        /// <summary>
        /// Validation errors.
        /// </summary>
        public IList<ValidationError> Errors { get; } = new List<ValidationError>();

        /// <summary>
        /// Validation warnings (non-fatal).
        /// </summary>
        public IList<ValidationWarning> Warnings { get; } = new List<ValidationWarning>();

        /// <summary>
        /// Merges another validation result into this one.
        /// </summary>
        public void Merge(ValidationResult other)
        {
            foreach (var error in other.Errors)
                Errors.Add(error);
            foreach (var warning in other.Warnings)
                Warnings.Add(warning);
        }
    }

    /// <summary>
    /// Schema validation error.
    /// </summary>
    public class ValidationError
    {
        /// <summary>
        /// Path to invalid data (e.g., "/customer/address/city").
        /// </summary>
        public string Path { get; set; } = "";

        /// <summary>
        /// Error message.
        /// </summary>
        public string Message { get; set; } = "";

        /// <summary>
        /// Error code (e.g., "type_mismatch", "required_missing").
        /// </summary>
        public string ErrorCode { get; set; } = "";

        /// <summary>
        /// Actual value that failed validation.
        /// </summary>
        public object? Value { get; set; }

        /// <summary>
        /// Expected schema type.
        /// </summary>
        public ISchemaType? ExpectedType { get; set; }

        public override string ToString() => $"[{ErrorCode}] {Path}: {Message}";
    }

    /// <summary>
    /// Schema validation warning.
    /// </summary>
    public class ValidationWarning
    {
        public string Path { get; set; } = "";
        public string Message { get; set; } = "";
        public string WarningCode { get; set; } = "";

        public override string ToString() => $"[{WarningCode}] {Path}: {Message}";
    }
}
```

---

### Exception Types

```csharp
namespace OoBDev.Framework.Data.Schema
{
    /// <summary>
    /// Schema parsing exception.
    /// </summary>
    public class SchemaParseException : Exception
    {
        public string SchemaFormat { get; }
        public int? LineNumber { get; }
        public int? ColumnNumber { get; }

        public SchemaParseException(
            string message,
            string format,
            int? line = null,
            int? column = null,
            Exception? innerException = null)
            : base(message, innerException)
        {
            SchemaFormat = format;
            LineNumber = line;
            ColumnNumber = column;
        }
    }

    /// <summary>
    /// Schema provider not found exception.
    /// </summary>
    public class SchemaProviderNotFoundException : Exception
    {
        public string SchemaFormat { get; }

        public SchemaProviderNotFoundException(string format)
            : base($"No schema provider registered for format: {format}")
        {
            SchemaFormat = format;
        }
    }

    /// <summary>
    /// Schema translation exception.
    /// </summary>
    public class SchemaTranslationException : Exception
    {
        public string SourceFormat { get; }
        public string TargetFormat { get; }

        public SchemaTranslationException(
            string message,
            string sourceFormat,
            string targetFormat,
            Exception? innerException = null)
            : base(message, innerException)
        {
            SourceFormat = sourceFormat;
            TargetFormat = targetFormat;
        }
    }
}
```

---

## Implementation Classes

### CanonicalSchema

```csharp
namespace OoBDev.Framework.Data.Schema
{
    /// <summary>
    /// Default canonical schema implementation.
    /// </summary>
    public class CanonicalSchema : ICanonicalSchema
    {
        public string? Title { get; init; }
        public string? Description { get; init; }
        public ISchemaType RootType { get; init; } = null!;
        public IReadOnlyDictionary<string, ISchemaType> Definitions { get; init; }
            = new Dictionary<string, ISchemaType>();
        public IReadOnlyDictionary<string, object> Metadata { get; init; }
            = new Dictionary<string, object>();

        public ISchemaType? ResolveReference(string reference)
        {
            // Handle JSON Schema style references: #/definitions/Customer
            if (reference.StartsWith("#/definitions/"))
            {
                var defName = reference.Substring("#/definitions/".Length);
                return Definitions.TryGetValue(defName, out var type) ? type : null;
            }

            // Handle simple name references
            return Definitions.TryGetValue(reference, out var simpleType) ? simpleType : null;
        }

        public ICanonicalSchema Clone()
        {
            return new CanonicalSchema
            {
                Title = Title,
                Description = Description,
                RootType = RootType.Clone(),
                Definitions = new Dictionary<string, ISchemaType>(
                    Definitions.Select(kvp => new KeyValuePair<string, ISchemaType>(kvp.Key, kvp.Value.Clone()))),
                Metadata = new Dictionary<string, object>(Metadata)
            };
        }
    }
}
```

### SchemaType Implementations

```csharp
namespace OoBDev.Framework.Data.Schema
{
    public class ObjectSchemaType : IObjectSchemaType
    {
        public SchemaTypeKind Kind => SchemaTypeKind.Object;
        public string? Title { get; init; }
        public string? Description { get; init; }
        public bool IsNullable { get; init; }
        public object? DefaultValue { get; init; }
        public IReadOnlyList<object> Examples { get; init; } = Array.Empty<object>();
        public IReadOnlyDictionary<string, object> Metadata { get; init; }
            = new Dictionary<string, object>();

        public IReadOnlyDictionary<string, ISchemaType> Properties { get; init; }
            = new Dictionary<string, ISchemaType>();
        public IReadOnlyList<string> Required { get; init; } = Array.Empty<string>();
        public int? MinProperties { get; init; }
        public int? MaxProperties { get; init; }
        public bool AdditionalPropertiesAllowed { get; init; } = true;
        public ISchemaType? AdditionalPropertiesSchema { get; init; }
        public IReadOnlyDictionary<string, ISchemaType> PatternProperties { get; init; }
            = new Dictionary<string, ISchemaType>();

        public ValidationResult Validate(object? value)
        {
            var result = new ValidationResult();

            if (value == null)
            {
                if (!IsNullable)
                    result.Errors.Add(new ValidationError
                    {
                        Path = "/",
                        Message = "Value cannot be null",
                        ErrorCode = "null_not_allowed",
                        Value = null,
                        ExpectedType = this
                    });
                return result;
            }

            // Validate properties
            var objDict = ObjectToDictionary(value);

            // Check required properties
            foreach (var requiredProp in Required)
            {
                if (!objDict.ContainsKey(requiredProp))
                    result.Errors.Add(new ValidationError
                    {
                        Path = $"/{requiredProp}",
                        Message = $"Required property '{requiredProp}' is missing",
                        ErrorCode = "required_missing",
                        ExpectedType = Properties.GetValueOrDefault(requiredProp)
                    });
            }

            // Validate each property
            foreach (var prop in objDict)
            {
                if (Properties.TryGetValue(prop.Key, out var propType))
                {
                    var propResult = propType.Validate(prop.Value);
                    foreach (var error in propResult.Errors)
                    {
                        error.Path = $"/{prop.Key}{error.Path}";
                        result.Errors.Add(error);
                    }
                }
                else if (!AdditionalPropertiesAllowed)
                {
                    result.Errors.Add(new ValidationError
                    {
                        Path = $"/{prop.Key}",
                        Message = $"Additional property '{prop.Key}' not allowed",
                        ErrorCode = "additional_property_not_allowed",
                        Value = prop.Value
                    });
                }
            }

            // Property count constraints
            if (MinProperties.HasValue && objDict.Count < MinProperties.Value)
                result.Errors.Add(new ValidationError
                {
                    Path = "/",
                    Message = $"Object has {objDict.Count} properties, minimum is {MinProperties.Value}",
                    ErrorCode = "min_properties_violated"
                });

            if (MaxProperties.HasValue && objDict.Count > MaxProperties.Value)
                result.Errors.Add(new ValidationError
                {
                    Path = "/",
                    Message = $"Object has {objDict.Count} properties, maximum is {MaxProperties.Value}",
                    ErrorCode = "max_properties_violated"
                });

            return result;
        }

        public ISchemaType Clone()
        {
            return new ObjectSchemaType
            {
                Title = Title,
                Description = Description,
                IsNullable = IsNullable,
                DefaultValue = DefaultValue,
                Examples = Examples,
                Metadata = new Dictionary<string, object>(Metadata),
                Properties = new Dictionary<string, ISchemaType>(
                    Properties.Select(kvp => new KeyValuePair<string, ISchemaType>(kvp.Key, kvp.Value.Clone()))),
                Required = Required,
                MinProperties = MinProperties,
                MaxProperties = MaxProperties,
                AdditionalPropertiesAllowed = AdditionalPropertiesAllowed,
                AdditionalPropertiesSchema = AdditionalPropertiesSchema?.Clone(),
                PatternProperties = new Dictionary<string, ISchemaType>(
                    PatternProperties.Select(kvp => new KeyValuePair<string, ISchemaType>(kvp.Key, kvp.Value.Clone())))
            };
        }

        private Dictionary<string, object?> ObjectToDictionary(object obj)
        {
            // Convert object to dictionary for validation
            var dict = new Dictionary<string, object?>();
            var props = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var prop in props)
                dict[prop.Name] = prop.GetValue(obj);
            return dict;
        }
    }

    public class StringSchemaType : IStringSchemaType
    {
        public SchemaTypeKind Kind => SchemaTypeKind.String;
        public string? Title { get; init; }
        public string? Description { get; init; }
        public bool IsNullable { get; init; }
        public object? DefaultValue { get; init; }
        public IReadOnlyList<object> Examples { get; init; } = Array.Empty<object>();
        public IReadOnlyDictionary<string, object> Metadata { get; init; }
            = new Dictionary<string, object>();

        public int? MinLength { get; init; }
        public int? MaxLength { get; init; }
        public string? Pattern { get; init; }
        public string? Format { get; init; }
        public IReadOnlyList<string>? EnumValues { get; init; }
        public string? ContentEncoding { get; init; }
        public string? ContentMediaType { get; init; }

        public ValidationResult Validate(object? value)
        {
            var result = new ValidationResult();

            if (value == null)
            {
                if (!IsNullable)
                    result.Errors.Add(new ValidationError
                    {
                        Message = "Value cannot be null",
                        ErrorCode = "null_not_allowed",
                        ExpectedType = this
                    });
                return result;
            }

            if (value is not string str)
            {
                result.Errors.Add(new ValidationError
                {
                    Message = $"Expected string, got {value.GetType().Name}",
                    ErrorCode = "type_mismatch",
                    Value = value,
                    ExpectedType = this
                });
                return result;
            }

            // Length constraints
            if (MinLength.HasValue && str.Length < MinLength.Value)
                result.Errors.Add(new ValidationError
                {
                    Message = $"String length {str.Length} is less than minimum {MinLength.Value}",
                    ErrorCode = "min_length_violated",
                    Value = str
                });

            if (MaxLength.HasValue && str.Length > MaxLength.Value)
                result.Errors.Add(new ValidationError
                {
                    Message = $"String length {str.Length} exceeds maximum {MaxLength.Value}",
                    ErrorCode = "max_length_violated",
                    Value = str
                });

            // Pattern constraint
            if (Pattern != null && !Regex.IsMatch(str, Pattern))
                result.Errors.Add(new ValidationError
                {
                    Message = $"String does not match pattern: {Pattern}",
                    ErrorCode = "pattern_mismatch",
                    Value = str
                });

            // Enum constraint
            if (EnumValues != null && !EnumValues.Contains(str))
                result.Errors.Add(new ValidationError
                {
                    Message = $"Value '{str}' is not in allowed enum values",
                    ErrorCode = "enum_violation",
                    Value = str
                });

            return result;
        }

        public ISchemaType Clone()
        {
            return new StringSchemaType
            {
                Title = Title,
                Description = Description,
                IsNullable = IsNullable,
                DefaultValue = DefaultValue,
                Examples = Examples,
                Metadata = new Dictionary<string, object>(Metadata),
                MinLength = MinLength,
                MaxLength = MaxLength,
                Pattern = Pattern,
                Format = Format,
                EnumValues = EnumValues,
                ContentEncoding = ContentEncoding,
                ContentMediaType = ContentMediaType
            };
        }
    }

    // Similar implementations for ArraySchemaType, NumberSchemaType, IntegerSchemaType, BooleanSchemaType, etc.
}
```

---

## Built-in Schema Providers

### JSON Schema Provider

```csharp
namespace OoBDev.Framework.Data.Schema.Providers
{
    /// <summary>
    /// JSON Schema (draft-07+) provider.
    /// </summary>
    public class JsonSchemaProvider : ISchemaProvider
    {
        public string SchemaFormat => "jsonschema";

        public ICanonicalSchema Parse(string schemaContent)
        {
            var jsonDoc = JsonDocument.Parse(schemaContent);
            var root = jsonDoc.RootElement;

            var title = root.TryGetProperty("title", out var titleProp)
                ? titleProp.GetString()
                : null;
            var description = root.TryGetProperty("description", out var descProp)
                ? descProp.GetString()
                : null;

            var definitions = new Dictionary<string, ISchemaType>();
            if (root.TryGetProperty("definitions", out var defsProp))
            {
                foreach (var def in defsProp.EnumerateObject())
                {
                    definitions[def.Name] = ParseType(def.Value);
                }
            }

            var rootType = ParseType(root);

            return new CanonicalSchema
            {
                Title = title,
                Description = description,
                RootType = rootType,
                Definitions = definitions
            };
        }

        private ISchemaType ParseType(JsonElement element)
        {
            if (!element.TryGetProperty("type", out var typeProp))
                throw new SchemaParseException("Missing 'type' property", "jsonschema");

            var typeStr = typeProp.GetString();
            return typeStr switch
            {
                "object" => ParseObjectType(element),
                "array" => ParseArrayType(element),
                "string" => ParseStringType(element),
                "number" => ParseNumberType(element),
                "integer" => ParseIntegerType(element),
                "boolean" => ParseBooleanType(element),
                "null" => new NullSchemaType(),
                _ => throw new SchemaParseException($"Unknown type: {typeStr}", "jsonschema")
            };
        }

        private IObjectSchemaType ParseObjectType(JsonElement element)
        {
            var properties = new Dictionary<string, ISchemaType>();
            if (element.TryGetProperty("properties", out var propsProp))
            {
                foreach (var prop in propsProp.EnumerateObject())
                {
                    properties[prop.Name] = ParseType(prop.Value);
                }
            }

            var required = new List<string>();
            if (element.TryGetProperty("required", out var reqProp))
            {
                foreach (var req in reqProp.EnumerateArray())
                    required.Add(req.GetString()!);
            }

            return new ObjectSchemaType
            {
                Title = GetStringOrNull(element, "title"),
                Description = GetStringOrNull(element, "description"),
                Properties = properties,
                Required = required,
                AdditionalPropertiesAllowed = GetBoolOrDefault(element, "additionalProperties", true)
            };
        }

        private IStringSchemaType ParseStringType(JsonElement element)
        {
            return new StringSchemaType
            {
                Title = GetStringOrNull(element, "title"),
                Description = GetStringOrNull(element, "description"),
                MinLength = GetIntOrNull(element, "minLength"),
                MaxLength = GetIntOrNull(element, "maxLength"),
                Pattern = GetStringOrNull(element, "pattern"),
                Format = GetStringOrNull(element, "format")
            };
        }

        public string Format(ICanonicalSchema schema)
        {
            var jsonObj = new Dictionary<string, object?>
            {
                ["$schema"] = "http://json-schema.org/draft-07/schema#",
                ["title"] = schema.Title,
                ["description"] = schema.Description
            };

            jsonObj = MergeTypeTo ObjectObj(jsonObj, schema.RootType);

            if (schema.Definitions.Count > 0)
            {
                var defs = new Dictionary<string, object?>();
                foreach (var def in schema.Definitions)
                {
                    defs[def.Key] = FormatType(def.Value);
                }
                jsonObj["definitions"] = defs;
            }

            return JsonSerializer.Serialize(jsonObj, new JsonSerializerOptions { WriteIndented = true });
        }

        private Dictionary<string, object?> FormatType(ISchemaType type)
        {
            return type.Kind switch
            {
                SchemaTypeKind.Object => FormatObjectType((IObjectSchemaType)type),
                SchemaTypeKind.String => FormatStringType((IStringSchemaType)type),
                SchemaTypeKind.Array => FormatArrayType((IArraySchemaType)type),
                SchemaTypeKind.Number => FormatNumberType((INumberSchemaType)type),
                // ... other types
                _ => new Dictionary<string, object?>()
            };
        }

        public ValidationResult Validate(object data, ICanonicalSchema schema)
        {
            return schema.RootType.Validate(data);
        }

        public bool CanParse(string content)
        {
            try
            {
                var doc = JsonDocument.Parse(content);
                return doc.RootElement.TryGetProperty("$schema", out var schemaProp) &&
                       schemaProp.GetString()?.Contains("json-schema") == true;
            }
            catch
            {
                return false;
            }
        }

        private string? GetStringOrNull(JsonElement element, string propertyName)
        {
            return element.TryGetProperty(propertyName, out var prop) ? prop.GetString() : null;
        }

        private int? GetIntOrNull(JsonElement element, string propertyName)
        {
            return element.TryGetProperty(propertyName, out var prop) ? prop.GetInt32() : null;
        }

        private bool GetBoolOrDefault(JsonElement element, string propertyName, bool defaultValue)
        {
            return element.TryGetProperty(propertyName, out var prop) ? prop.GetBoolean() : defaultValue;
        }
    }
}
```

---

## Usage Examples

### Example 1: Discover Schema from DataContainer

```csharp
var data = new
{
    Customer = new
    {
        FirstName = "John",
        LastName = "Doe",
        Age = 30,
        Email = "john.doe@example.com",
        Address = new
        {
            Street = "123 Main St",
            City = "Seattle",
            ZipCode = "98101"
        }
    }
};

var container = DataContainerFactory.Create(data);
var schemaService = new SchemaDiscoveryService();

// Discover schema
var schema = await schemaService.GetSchemaAsync(container);

Console.WriteLine($"Schema Title: {schema.Title}");
Console.WriteLine($"Root Type: {schema.RootType.Kind}");

if (schema.RootType is IObjectSchemaType objType)
{
    Console.WriteLine($"Properties: {objType.Properties.Count}");
    foreach (var prop in objType.Properties)
    {
        Console.WriteLine($"  - {prop.Key}: {prop.Value.Kind}");
    }
}

// Output:
// Schema Title: null
// Root Type: Object
// Properties: 1
//   - Customer: Object
```

### Example 2: Translate Schema Between Formats

```csharp
var jsonSchemaContent = @"{
  ""$schema"": ""http://json-schema.org/draft-07/schema#"",
  ""type"": ""object"",
  ""properties"": {
    ""name"": { ""type"": ""string"", ""minLength"": 1 },
    ""age"": { ""type"": ""integer"", ""minimum"": 0 },
    ""email"": { ""type"": ""string"", ""format"": ""email"" }
  },
  ""required"": [""name"", ""email""]
}";

var schemaService = new SchemaDiscoveryService();
schemaService.RegisterProvider(new JsonSchemaProvider());
schemaService.RegisterProvider(new XsdSchemaProvider());

// Translate JSON Schema → XSD
var xsdSchema = schemaService.TranslateSchema(
    jsonSchemaContent,
    "jsonschema",
    "xsd");

Console.WriteLine(xsdSchema);

// Output:
// <?xml version="1.0" encoding="UTF-8"?>
// <xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema">
//   <xs:element name="root">
//     <xs:complexType>
//       <xs:sequence>
//         <xs:element name="name" type="xs:string" minOccurs="1"/>
//         <xs:element name="age" type="xs:integer" minOccurs="0"/>
//         <xs:element name="email" type="xs:string" minOccurs="1"/>
//       </xs:sequence>
//     </xs:complexType>
//   </xs:element>
// </xs:schema>
```

### Example 3: Infer Schema from CLR Type

```csharp
public class Customer
{
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public int Age { get; set; }
    public string Email { get; set; } = "";
    public Address? BillingAddress { get; set; }
}

public class Address
{
    public string Street { get; set; } = "";
    public string City { get; set; } = "";
    public string ZipCode { get; set; } = "";
}

var schemaService = new SchemaDiscoveryService();

// Infer from type
var schema = schemaService.InferSchemaFromType(typeof(Customer));

// Get as JSON Schema
schemaService.RegisterProvider(new JsonSchemaProvider());
var jsonSchema = schemaService.GetProvider("jsonschema")!.Format(schema);

Console.WriteLine(jsonSchema);

// Output:
// {
//   "$schema": "http://json-schema.org/draft-07/schema#",
//   "title": "Customer",
//   "type": "object",
//   "properties": {
//     "FirstName": { "type": "string" },
//     "LastName": { "type": "string" },
//     "Age": { "type": "integer" },
//     "Email": { "type": "string", "format": "email" },
//     "BillingAddress": { "$ref": "#/definitions/Address" }
//   },
//   "required": ["FirstName", "LastName", "Age", "Email"],
//   "definitions": {
//     "Address": {
//       "type": "object",
//       "properties": {
//         "Street": { "type": "string" },
//         "City": { "type": "string" },
//         "ZipCode": { "type": "string" }
//       },
//       "required": ["Street", "City", "ZipCode"]
//     }
//   }
// }
```

### Example 4: Validate Data Against Schema

```csharp
var schemaContent = @"{
  ""type"": ""object"",
  ""properties"": {
    ""name"": { ""type"": ""string"", ""minLength"": 1 },
    ""age"": { ""type"": ""integer"", ""minimum"": 0, ""maximum"": 120 }
  },
  ""required"": [""name""]
}";

var schemaService = new SchemaDiscoveryService();
schemaService.RegisterProvider(new JsonSchemaProvider());

var schema = schemaService.ParseAny(schemaContent);

// Valid data
var validData = new { name = "John", age = 30 };
var result1 = schemaService.ValidateAgainstSchema(validData, schema);
Console.WriteLine($"Valid: {result1.IsValid}");  // True

// Invalid data (missing required property)
var invalidData1 = new { age = 30 };
var result2 = schemaService.ValidateAgainstSchema(invalidData1, schema);
Console.WriteLine($"Valid: {result2.IsValid}");  // False
Console.WriteLine($"Errors: {string.Join(", ", result2.Errors)}");
// Output: [required_missing] /name: Required property 'name' is missing

// Invalid data (constraint violation)
var invalidData2 = new { name = "Jane", age = 150 };
var result3 = schemaService.ValidateAgainstSchema(invalidData2, schema);
Console.WriteLine($"Valid: {result3.IsValid}");  // False
Console.WriteLine($"Errors: {string.Join(", ", result3.Errors)}");
// Output: [max_value_exceeded] /age: Value 150 exceeds maximum 120
```

### Example 5: Get Schema for Specific Path

```csharp
var data = new
{
    Customers = new[]
    {
        new { Id = 1, Name = "Alice", Orders = new[] { new { Total = 100.0 } } },
        new { Id = 2, Name = "Bob", Orders = new[] { new { Total = 200.0 } } }
    }
};

var container = DataContainerFactory.Create(data);
var schemaService = new SchemaDiscoveryService();

// Get schema for specific path
var schema = await schemaService.GetSchemaAsync(container, "Customers/0/Orders");

if (schema.RootType is IArraySchemaType arrayType)
{
    Console.WriteLine($"Array of: {arrayType.ItemsSchema.Kind}");

    if (arrayType.ItemsSchema is IObjectSchemaType objType)
    {
        foreach (var prop in objType.Properties)
        {
            Console.WriteLine($"  - {prop.Key}: {prop.Value.Kind}");
        }
    }
}

// Output:
// Array of: Object
//   - Total: Number
```

### Example 6: Custom Schema Provider

```csharp
public class OpenApiSchemaProvider : ISchemaProvider
{
    public string SchemaFormat => "openapi";

    public ICanonicalSchema Parse(string schemaContent)
    {
        var openApiDoc = OpenApiDocument.Load(schemaContent);
        var schema = openApiDoc.Components.Schemas["MyModel"];

        return ConvertOpenApiSchemaToCanonical(schema);
    }

    public string Format(ICanonicalSchema schema)
    {
        var openApiSchema = ConvertCanonicalToOpenApiSchema(schema);
        return openApiSchema.SerializeAsJson();
    }

    public ValidationResult Validate(object data, ICanonicalSchema schema)
    {
        return schema.RootType.Validate(data);
    }

    public bool CanParse(string content)
    {
        return content.Contains("openapi") && content.Contains("3.");
    }

    // Helper methods...
}

// Usage
var schemaService = new SchemaDiscoveryService();
schemaService.RegisterProvider(new OpenApiSchemaProvider());

var openApiSpec = File.ReadAllText("api-spec.yaml");
var schema = schemaService.ParseAny(openApiSpec);
```

---

## Extension Methods

```csharp
namespace OoBDev.Framework.Data.Schema
{
    public static class SchemaDiscoveryExtensions
    {
        /// <summary>
        /// Gets schema as JSON Schema.
        /// </summary>
        public static async Task<string> GetAsJsonSchemaAsync(
            this ISchemaDiscoveryService service,
            IDataContainer container)
        {
            return await service.GetSchemaAsAsync(container, "jsonschema");
        }

        /// <summary>
        /// Gets schema as XSD.
        /// </summary>
        public static async Task<string> GetAsXsdAsync(
            this ISchemaDiscoveryService service,
            IDataContainer container)
        {
            return await service.GetSchemaAsAsync(container, "xsd");
        }

        /// <summary>
        /// Validates data container against schema.
        /// </summary>
        public static async Task<ValidationResult> ValidateAsync(
            this IDataContainer container,
            ICanonicalSchema schema,
            ISchemaDiscoveryService service)
        {
            var data = await container.Navigate("/").GetValueAsync();
            return service.ValidateAgainstSchema(data!, schema);
        }

        /// <summary>
        /// Checks if schema type is object.
        /// </summary>
        public static bool IsObject(this ISchemaType type)
        {
            return type.Kind == SchemaTypeKind.Object;
        }

        /// <summary>
        /// Checks if schema type is array.
        /// </summary>
        public static bool IsArray(this ISchemaType type)
        {
            return type.Kind == SchemaTypeKind.Array;
        }

        /// <summary>
        /// Checks if schema type is primitive (string, number, integer, boolean).
        /// </summary>
        public static bool IsPrimitive(this ISchemaType type)
        {
            return type.Kind is SchemaTypeKind.String
                or SchemaTypeKind.Number
                or SchemaTypeKind.Integer
                or SchemaTypeKind.Boolean;
        }
    }
}
```

---

## Best Practices

1. **Use Auto-Detection**: Use `ParseAny()` when schema format is unknown
2. **Cache Schemas**: Inferred schemas should be cached for performance
3. **Validate Early**: Validate data as early as possible in the pipeline
4. **Preserve Metadata**: Include titles, descriptions for better documentation
5. **Handle References**: Ensure `$ref` and type references are properly resolved
6. **Graceful Degradation**: Not all schema features translate 1:1 between formats

---

## Performance Considerations

- **Schema inference** is expensive - cache results
- **Validation** can be slow for large data - consider streaming validation
- **Translation** through canonical form adds overhead - cache translated schemas
- **Reference resolution** can cause cycles - implement cycle detection

---

## Related Documents

- [Requirements](./requirements.md)
- [Architecture](./architecture.md)
- [Testing Strategy](./testing-strategy.md)
- [Core Container API](../CoreContainer/api-design.md)
