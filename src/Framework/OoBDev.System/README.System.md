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

### TemplateRenderHarness

`TemplateRenderHarness.CheckAsync(engine, cases)` renders each `TemplateRenderCase` (template name, sample data, approved output) and returns a `TemplateRenderFailure` per case that is missing, renders nothing, throws or differs; the reason names the first differing line. Line endings and trailing blank lines are ignored. Use it in a project test so a template or engine change that alters a message fails the build (`TemplateRenderHarnessTests`).

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

### Applesoft BASIC
`Detokenizer` reads a tokenized program; `ApplesoftParser` reads a listing into lines, statements and keyword or literal elements (interpreter-level lexing, bounded, `FormatException` on bad input); `ApplesoftTokenizer` writes the binary form back. See the 2026-11-12 change document for the REM and DATA spacing caveat.

### Dos33
`DiskImageCommands` reads a DOS 3.3 image (catalog, track/sector lists, data). `DiskImageWriter` (`IDiskImageWriter`) adds, replaces and deletes files in DOS-order 256-byte-sector images: it works on an in-memory copy and writes back only when the file fits, so a full disk, a full catalog or a duplicate name leaves the stream unchanged (`InvalidOperationException`). Sectors are taken outward from track 17; a delete frees the bitmap and marks the entry `0xFF` with the original track kept, as DOS does. `GetFileContent` returns a file with its real length (binary 4-byte header, BASIC 2-byte prefix, text up to the first zero) where `GetDataFileEntry` returns whole sectors. `DiskImageOrder.IsProDosOrder` guesses from the length of the catalog chain whether a `.po` image holds its sectors in ProDOS order, and `ConvertSectorOrder` swaps between the orders (sectors 1 to 14 mirror); convert before using the reader or writer. The caller supplies Applesoft and binary files with DOS's own length prefix.

## IO.PathEx globbing

`PathEx.EnumerateFiles(pattern)` takes the literal leading segments as the root and matches the rest with `Microsoft.Extensions.FileSystemGlobbing` (`*` and `**`; `?` is narrowed by a regular expression because the matcher lacks it). `PathEx.EnumerateFiles(root, patterns)` matches under a root only and treats a pattern starting with `!` as an exclusion, for example `!**/obj/**`.

## IO.Fat

### FatVolume
Read-only FAT12, FAT16 and FAT32 reader (`FatVolume.Open(stream)`). The type comes from the cluster count, as the specification requires. `GetEntries(directory?)` lists a directory with long names (checksum-verified; deleted entries, `.`/`..` and the label entry are skipped), `Find(path)` walks `/` or `\` paths without regard to case, `OpenRead(entry)` returns the file content, `Label` is the root label entry or the boot label. Short names are Latin-1 (OEM code pages are not applied). The image is untrusted: sizes, table length, cluster numbers and chains are bounded, and corrupt or looping structures raise `InvalidDataException`. Write support is not part of it.

## Time

### HybridLogicalClock and HybridLogicalClockGenerator

Orders events across sites whose clocks are skewed. `HybridLogicalClock` is wall-clock ticks plus a logical counter, compared physical first. `HybridLogicalClockGenerator` (register as a singleton, thread safe, takes a `TimeProvider`) offers `Next()` for local events and `Merge(received)` for events received from another site; both return values strictly greater than anything seen before. An optional `maxDrift` rejects a received clock too far ahead of local time, and counter exhaustion throws `OverflowException` instead of wrapping. Pattern from the sync-mesh reference (hybrid logical clock paper); see [CQRS and event sourcing](../../../docs/design/CqrsEventSourcing/README.md).

## IO

### CsvWriter

- **WriteAsCsvTo(source, stream, includeHeader)** and **WriteAsCsvToAsync(...)**: write a sequence of objects as CSV, one column per readable public property. Every field is quoted, embedded quotes are doubled, underscores in header names become spaces, values use the invariant culture, null items are skipped, and the stream stays open.

### IniFile

- **Read(section, key, default)**, **Write(section, key, value)** and indexers: a managed INI reader and writer with no Win32 dependency. Names are case-insensitive, comments (`;` or `#`) and unrelated lines survive a write, and a missing file, section or key reads as the default.

## ExpressionCalculator optimizers

`ExpressionOptimizationProvider<T>` applies inner-expression, unary, identity (`B^1`, `B*1`, `B+0`, `B/-1` and so on), constant folding and commutative-variable shifting until the expression stops changing. The four "bonus round" reductions of the source (`B^-1` to `1/B`, `B/N` to `B*(1/N)`, `B*B*...` to `B^T`, `B+B+...` to `B*T`) are declined (2026-11-11): the first two change results for integer `T` (integer division) and for floating point (rounding), and the last two save no real work for a tree evaluated once. Add one only with a test that proves it is exact for every supported `T`.
