# OoBDev.System

OoBDev.System provides a comprehensive set of classes and utilities for various system-related functionalities. Here's an overview of its key components:

## Security.Cryptography

### HashTypes
Specifies different types of hash algorithms.
- **Md5**: Represents the MD5 hash algorithm.
- **Sha256**: Represents the SHA-256 hash algorithm.
- **Sha512**: Represents the SHA-512 hash algorithm.

### Md5Hash
Default hash of input value. Base64 encoded MD5 Hash.
- **GetHash(value: string)**: Computes the default hash of the input value using MD5.

### Sha256Hash
Default hash of input value. Base64 encoded SHA256 Hash.
- **GetHash(value: string)**: Computes the default hash of the input value using SHA256.

### Sha512Hash
Default hash of input value. Base64 encoded SHA512 Hash.
- **GetHash(value: string)**: Computes the default hash of the input value using SHA512.

## ServiceCollectionExtensions

### TryAddSystemExtensions
Adds all available extensions to the IOC container.
### TrySecurityExtensions
Adds support for shared security extensions.
### TrySerializerExtensions
Adds support for shared serializers.
### TryTemplatingExtensions
Adds support for shared templating.

## SystemExtensionBuilder
Builder for configuring system extensions.

## Numerics
Numbers that format and parse in a notation, migrated from BinaryDataDecoders.

| Type | Notation | Example |
|------|----------|---------|
| `ScientificNumber` | power of ten | `1.001e3` |
| `EngineeringNumber` | SI prefix, base 1000 | `110m`, `1.5k` |
| `ByteNumber` | SI prefix, base 1024, trailing `b` | `1kb`, `112.64mb` |

`FormattableNumber.Parse(text)` accepts any of the three. All types implement `INumeric` (`AsDouble` to `AsInt64`), convert implicitly from and to the built-in numeric types and to each other, and format through `ScienceNotationFormatter`, `EngineeringNotationFormatter` or `ByteNotationFormatter` (usable with `string.Format`). Parsing and formatting use the invariant culture.

## Text.Json

### JsonSerializer
Default serializer for JSON.
- **Serialize(obj: object, type: Type)**: Serializes an object to a JSON string.
- **Deserialize(stream: Stream, type: Type)**: Deserializes a JSON stream to an object of a given type.

### BsonDateTimeOffsetConverter
System.Text.Json converter to support BSON DateTimeOffset.

### BsonIdConverter
Type converter for System.Text.Json to support BSON ObjectID to JSON safe export/import.

### DictionaryStringObjectJsonConverter
Custom JSON converter for dictionaries with string keys and object values.

## Templating

### FileTemplateSource
Access template from the file system.
- **Get(templateName: string)**: Looks up templates from the file system.

### FileTemplatingOptions
Configuration settings for file templating engine.

### TemplateContext
Represents the context of a template.
- Provides information about the template and its processing.

### TemplateEngine
Generates a templating engine that tries to use the best match for source and provider.
- **ApplyAsync**: Applies the template asynchronously to the provided data.

### XsltTemplateProvider
Provides template processing using XSLT. Data may be an `IXPathNavigable`, JSON, XML or any serializable object. `XsltTemplateOptions.Extensions` lists extension objects (for example `StringExtensions`, `PathExtensions`) whose methods become XSLT functions; none are available by default.

## Xml.Serialization

### DefaultXmlSerializer
Default XmlSerializer.


## Retro.Apple2

### Apple2Encoding
Single-byte `Encoding` for Apple ][ text. Bytes with bit 7 set (normal text) decode to the ASCII character with the bit cleared; bytes with bit 7 clear (inverse and flashing text) decode to the private-use character `U+E000` plus the byte value, so all 256 byte values round-trip through `GetString` and `GetBytes`. Encoding ASCII sets bit 7; other characters become `?`. `Dos33.FileEntry` names use it.

## Time

### HybridLogicalClock and HybridLogicalClockGenerator

Orders events across sites whose clocks are skewed. `HybridLogicalClock` is wall-clock ticks plus a logical counter, compared physical first. `HybridLogicalClockGenerator` (register as a singleton, thread safe, takes a `TimeProvider`) offers `Next()` for local events and `Merge(received)` for events received from another site; both return values strictly greater than anything seen before. An optional `maxDrift` rejects a received clock too far ahead of local time, and counter exhaustion throws `OverflowException` instead of wrapping. Pattern from the sync-mesh reference (hybrid logical clock paper); see [CQRS and event sourcing](../../../docs/design/CqrsEventSourcing/README.md).

## IO

### CsvWriter

- **WriteAsCsvTo(source, stream, includeHeader)** and **WriteAsCsvToAsync(...)**: write a sequence of objects as CSV, one column per readable public property. Every field is quoted, embedded quotes are doubled, underscores in header names become spaces, values use the invariant culture, null items are skipped, and the stream stays open.

### IniFile

- **Read(section, key, default)**, **Write(section, key, value)** and indexers: a managed INI reader and writer with no Win32 dependency. Names are case-insensitive, comments (`;` or `#`) and unrelated lines survive a write, and a missing file, section or key reads as the default.
