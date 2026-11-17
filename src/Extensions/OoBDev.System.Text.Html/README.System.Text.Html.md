# OoBDev.System.Text.Html

## Summary

HTML support for the OoBDev text templating framework, built on HtmlAgilityPack. It turns HTML files and template text into `XPathNavigator` sources, and applies data-bound HTML templates (`value-of`, `repeater`, `condition`, `value-attr`, `data-binding`). Syntax and rules: [html-directives](../../../docs/design/TextTemplating/html-directives.md).

## Contents

| Type | Purpose |
|------|---------|
| `HtmlNavigator` | `IToXPathNavigable` for `text/html` and the `.html` and `.htm` file extensions; loads a file into an XPath navigator with nested-tag fixing and auto-close enabled |
| `HtmlTemplateTransform` | `ITemplateTransform` for HTML content (`[TemplateTransform(MediaTypes.Html)]`); `Transform` resolves the data and runs the visitor |
| `IHtmlDocumentVisitor` | Extension point that visits an `HtmlNode` with the root, current and scoped data path resolvers |
| `HtmlDocumentVisitor` | The directive implementation of `IHtmlDocumentVisitor` |
| `JsonInstanceFactory`, `JsonPathResolver` | `IInstanceFactory` and `IPathResolver` over System.Text.Json (a JSONPath subset); the factory creates path resolvers only |

## Use

```csharp
var factory = new JsonInstanceFactory();
var transform = new HtmlTemplateTransform(factory, new HtmlDocumentVisitor(factory));
string html = await transform.Transform(model, "<repeater binding=\"Items\"><value-of binding=\"Name\" />, </repeater>");
```

## Notes

- Server-side code in the source HTML is disabled when parsing.
- Values are HTML-encoded; data cannot inject markup.
- No registration extension yet: `IInstanceFactory` has no default registration in the framework (JSON-shaped data only).

## Verified

`OoBDev.System.Text.Html.Tests`: 20 Unit tests pass (2026-11-12).

## See Also

- [Patterns and practices](../../../docs/patterns-discovery/README.md)
- Tests: `OoBDev.System.Text.Html.Tests`
