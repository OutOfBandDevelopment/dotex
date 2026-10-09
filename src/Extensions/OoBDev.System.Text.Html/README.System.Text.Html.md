# OoBDev.System.Text.Html

## Summary

HTML support for the OoBDev text templating framework, built on HtmlAgilityPack. It turns HTML files and template text into `XPathNavigator` sources so the same template engine that handles XML and other formats can process HTML.

## Contents

| Type | Purpose |
|------|---------|
| `HtmlNavigator` | `IToXPathNavigable` for `text/html` and the `.html` and `.htm` file extensions; loads a file into an XPath navigator with nested-tag fixing and auto-close enabled |
| `HtmlTemplateTransform` | `ITemplateTransform` for HTML content (`[TemplateTransform(MediaTypes.Html)]`); parses template text and walks it with an `IHtmlDocumentVistor` |
| `IHtmlDocumentVistor` | Extension point that visits an `HtmlNode` with the root, current and scoped data path resolvers, so binding elements can be replaced with data |

## Notes

- Server-side code in the source HTML is disabled when parsing.
- The interface name `IHtmlDocumentVistor` is misspelled (Visitor); renaming it is a breaking change and is tracked with the other naming items in the repository `TODO.md`.
- The file ends with a commented-out earlier implementation of the binding visitor (`value-of`, `repeater`, `condition`, `value-attr`, `data-binding` elements); it documents the binding syntax the visitor is meant to support.

## See Also

- [Patterns and practices](../../../docs/patterns-discovery/README.md)
- Tests: `OoBDev.System.Text.Html.Tests`
