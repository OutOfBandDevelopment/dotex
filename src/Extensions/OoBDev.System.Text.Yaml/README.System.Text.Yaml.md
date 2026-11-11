# OoBDev.System.Text.Yaml

YAML to XPath navigation provider for configuration and data file processing.

## Description

This package provides YAML file parsing with XPath navigation capabilities, allowing you to query YAML documents using familiar XPath syntax. It extends the YamlDotNet library with XPath navigation support.

## Key Features

- YAML file parsing and navigation
- XPath query support for YAML documents
- Multiple YAML media type support
- Stream and file-based input
- Integration with OoBDev.System metadata framework

## Installation

```xml
<PackageReference Include="OoBDev.System.Text.Yaml" Version="*" />
```

## Basic Usage

A mapping key becomes an element name and a scalar becomes its text, so `server: { port: 8080 }` is `//server/port`. Keys that are not valid XML names (`my key`, `2nd`) do not throw; query the keys that are. Only the first document of a multi-document stream is read.

```csharp
using OoBDev.System.Text.Yaml;

var navigator = new YamlNavigator();
var navigable = navigator.ToNavigable("config.yaml");   // ".yaml" and ".yml" files

var xpath = navigable!.CreateNavigator();
var port = xpath.SelectSingleNode("//server/port")?.Value;
```

`ToNavigable(Stream)` reads a stream and leaves it open; the caller owns it. The navigator is registered for the media types `text/yaml`, `text/vnd.yaml`, `text/x-yaml`, `application/yaml`, `application/vnd.yaml` and `application/x-yaml`.

## License

See repository license file for details.
