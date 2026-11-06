# OoBDev.Handlebars

Handlebars templates for the framework template engine (`OoBDev.System.Text.Templating`), on Handlebars.Net.

## Description

`HandlebarsTemplateProvider` is an `ITemplateProvider` for the content type `text/x-handlebars-template` (files ending `.hbs`). For each `ApplyAsync` it creates a Handlebars environment (`NoEscape` from `HandlebarsOptions`, default true; JSON support from `Handlebars.Net.Extension.Json`), registers every helper found in the container, compiles the template that `context.OpenTemplate` returns and writes the result to the target stream. The data object is the Handlebars context; the `ITemplateContext` is passed as the Handlebars `data`.

`TryAddHandlebarServices()` registers the provider (plain and keyed `handlebars`, `HandlebarsGlobals.ProviderKey`), the `.hbs` file type with `IsTemplateType = true`, and the helpers below. Helper sets from `Handlebars.Net.Helpers` (`IHelpersRegistry`) and any extra `IHelperDescriptor<HelperOptions>` or block descriptor in the container are registered too, so an application adds a helper by registering a descriptor.

| Helper | Arguments | Output |
|--------|-----------|--------|
| `date_now` | optional .NET format string | the local time from `TimeProvider`, formatted with the invariant culture |
| `guid_new` | none | a new GUID from `IGuidProvider` |
| `hash` | one value | the hash of its text from the registered `IHash` |
| `set` | key, value | stores the value for the rest of the render (async-local store), writes nothing |
| `get` | key | writes the stored value |
| `str-replace` | input, find, replacement | the input with every `find` replaced; writes nothing when the input or `find` is blank |

`set` and `get` share one store per async flow, not per render: concurrent renders on separate async flows are isolated, but two renders in one flow share keys.

```csharp
services.TryAddHandlebarServices();
// content type and extension routing come from the template engine registration
var text = await engine.ApplyAsync("welcome.hbs", new { Name = "Ada" });
```

## Verified

`OoBDev.Handlebars.Tests` has 9 `Unit` tests for the provider, helpers and escaping.

## Not built yet

The design set in `Features/Proposals/10-TextTemplating/HandlebarsProvider` goes further. Not in the code: a compiled-template cache (every call recompiles and re-registers helpers), the formatting helpers `formatDate`, `formatNumber`, `currency`, `uppercase`, `lowercase` and `json`, the `IDataContainer` adapter (needs Epic 11), and the database template source. They are tracked in `BACKLOG.md`.

## Notes and references

* https://github.com/Handlebars-Net/Handlebars.Net
* https://github.com/Handlebars-Net/Handlebars.Net.Extension.Json
* https://github.com/Handlebars-Net/Handlebars.Net.Helpers
