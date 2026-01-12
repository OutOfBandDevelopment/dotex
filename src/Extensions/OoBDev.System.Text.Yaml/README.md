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

```csharp
using OoBDev.System.Text.Yaml;

var navigator = new YamlNavigator();
var navigable = navigator.ToNavigable("config.yaml");

// Use XPath to query the YAML document
var xpath = navigable.CreateNavigator();
var value = xpath.SelectSingleNode("//setting[@name='value']");
```

## License

See repository license file for details.

## Repository

[OoBDev Repository](https://github.com/yourusername/oobdev)
