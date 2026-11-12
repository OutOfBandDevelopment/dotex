# HTML template directives

**Status:** built 2026-11-12 in `OoBDev.System.Text.Html` (owner decision 2026-11-11). **Last Updated:** 2026-11-12

Data-bound HTML templates for the cases where the template is itself HTML (mail bodies, generated pages) and Handlebars or XSLT would force a second syntax. The directives are plain elements and attributes, so a template stays valid, previewable HTML.

## Contents

- [Directives](#directives)
- [Path and rule syntax](#path-and-rule-syntax)
- [Design notes](#design-notes)
- [Types](#types)

## Directives

**Table 1 — Directives**

| Directive | Attributes | Effect |
|-----------|------------|--------|
| `<value-of binding="path" />` | `format`, `type` (`date`, `number`), `source` | Replaced by the selected value, HTML-encoded. `format` is a .NET format string; without `type` it tries a date, then a number. Without `format` and `type` the text is unchanged. |
| `<repeater binding="path">…</repeater>` | `item`, `source` | The content is repeated for each selected item (an array selected by itself repeats its elements) and the wrapper element is removed. Bindings inside are relative to the item. |
| `<condition rule="rule">…</condition>` | `item`, `source` | The content is kept (wrapper removed) when the rule holds for the current item, otherwise dropped. |
| `<value-attr item="attribute" binding="path" />` | `value`, `format`, `type`, `rule` | Sets attribute `item` on the preceding `input` or `img`, or else on the enclosing element; the value is the binding or the literal `value`; `rule` makes it conditional. The directive element is removed. |
| `data-binding="path"` on any element | | The element content is replaced by the selected values (joined by new lines) and the attribute is removed. |

`item="name"` on a repeater or condition names the item, and `source="name"` on a later directive reads from that named item instead of the current one (nested repeaters).

## Path and rule syntax

A JSONPath subset: `$` root, `@` current, `.name`, `name`, `['name']`, `[n]`, `[*]`, `[?(expression)]`. An expression is `left op right` (`== != >= <= > <`; right side a quoted string, number, `true`, `false`, `null` or a path) or a bare path that must select a truthy value. A rule holds when the expression holds for the current item or any descendant. Paths starting with `$` always use the root data, so inside a nested repeater `$` still means the whole document.

## Design notes

- Values are HTML-encoded (the reference implementation in the owner's TextTemplating project injected data as markup); data cannot add elements.
- No new dependency: `System.Text.Json.Nodes` with a small evaluator, not Newtonsoft JSONPath. Recursive descent (`..`) and script expressions are not supported.
- `IPathResolver` gained `Test(rule)`; `IHtmlDocumentVisitor` (was misspelled `IHtmlDocumentVistor`) is implemented by `HtmlDocumentVisitor`.
- `JsonInstanceFactory` is the `IInstanceFactory` for JSON-shaped data (any object is serialised with System.Text.Json); it creates path resolvers only.
- Handlebars and XSLT remain the recommendation for new non-HTML templates; this syntax exists because the owner's existing HTML templates use it.

## Types

`HtmlTemplateTransform` (`text/html`), `HtmlDocumentVisitor`, `IHtmlDocumentVisitor`, `JsonInstanceFactory`, `JsonPathResolver`; tests in `OoBDev.System.Text.Html.Tests` (`HtmlTemplateDirectivesTests`, `JsonPathQueryTests`).
