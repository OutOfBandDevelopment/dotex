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
Provides template processing using XSLT.

## Xml.Serialization

### DefaultXmlSerializer
Default XmlSerializer.

