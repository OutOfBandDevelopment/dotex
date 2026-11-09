# OoBDev.Handlebars

Handlebars templates for the framework template engine (`OoBDev.System.Text.Templating`), on Handlebars.Net.

## Description

`HandlebarsTemplateProvider` is an `ITemplateProvider` for the content type `text/x-handlebars-template` (files ending `.hbs`). It builds one Handlebars environment per provider instance (`NoEscape` from `HandlebarsOptions`, default true; JSON support from `Handlebars.Net.Extension.Json`), registers every helper found in the container, and for each `ApplyAsync` compiles (or reuses, see Compilation cache) the template that `context.OpenTemplate` returns and writes the result to the target stream. The data object is the Handlebars context; the `ITemplateContext` is passed as the Handlebars `data`.

`TryAddHandlebarServices()` registers the provider (plain and keyed `handlebars`, `HandlebarsGlobals.ProviderKey`), the `.hbs` file type with `IsTemplateType = true`, and the helpers below. Helper sets from `Handlebars.Net.Helpers` (`IHelpersRegistry`) and any extra `IHelperDescriptor<HelperOptions>` or block descriptor in the container are registered too, so an application adds a helper by registering a descriptor.

| Helper | Arguments | Output |
|--------|-----------|--------|
| `date_now` | optional .NET format string | the local time from `TimeProvider`, formatted with the invariant culture |
| `format_date` | value, optional .NET format (default `yyyy-MM-dd`) | `DateTime`, `DateTimeOffset` or a parsable string, invariant culture; nothing for other values |
| `format_number` | value, optional format (default `N2`), optional culture name | falls back to the value text when it is not a number |
| `currency` | value, optional format (default `C`), optional culture name | same helper as `format_number` with a currency default |
| `json` | value, optional `true` to indent | `System.Text.Json` |
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

The design set in `docs/design/TextTemplating/HandlebarsProvider` goes further. Not in the code: the `IDataContainer` adapter (needs Epic 11), and the database template source. They are tracked in `BACKLOG.md`.

## Notes and references

* https://github.com/Handlebars-Net/Handlebars.Net
* https://github.com/Handlebars-Net/Handlebars.Net.Extension.Json
* https://github.com/Handlebars-Net/Handlebars.Net.Helpers

The formatting helpers use snake_case names like the other helpers (the design says camelCase); `uppercase` and `lowercase` come from `Handlebars.Net.Helpers`.

## Compilation cache

The helpers are registered once per provider instance, and compiled templates are cached by the SHA-256 of the template text, so an edited template compiles again and nothing needs invalidating. `HandlebarsOptions.EnableCompilationCache` (default on) and `MaxCachedTemplates` (default 100; the cache is cleared when full) control it.
