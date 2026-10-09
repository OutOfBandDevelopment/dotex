# Architecture § 7 — Cross-Cutting Building Blocks (all in `OoBDev.System*`)

<!-- nav -->
[↑ 01 — Architecture: How the Solution Is Put Together](./README.md) · [← Architecture § 6 — Runtime Composition Model (How a Call Finds Its Implementation)](./06-runtime-composition-model.md) · [Architecture § 8 — Hosting Model →](./08-hosting-model.md)
<!-- nav -->

`OoBDev.System.Abstractions` + `OoBDev.System` are the "BCL extensions" foundation that everything else references. They deliberately hold the *shared vocabulary*:

**Table 4 — Cross-cutting building blocks**

| Concern | Abstractions | Purpose |
|---------|--------------|---------|
| Time / identity of things | `TimeProvider`, `IGuidProvider`, `ITempFileFactory` | Make non-determinism injectable/testable. |
| Serialization | `ISerializer`, `IJsonSerializer`, `IBsonSerializer`, `IXmlSerializer`, `IObjectConverter` | Keyed by `SerializerTypes`; JSON default. |
| Hashing / HMAC | `IHash`, `IHMACCalculator` | Keyed by name; MD5 default (see alternatives doc). |
| Current user | `ICurrentUserAccessor` | Environment implementation by default, replaced by HTTP implementation in ASP.NET. |
| Result envelope | `IResult`, `IModelResult<T>`, `IQueryResult<T>`, `IPagedQueryResult<T>`, `ResultMessage` | Uniform API response shape with severity-levelled messages. |
| Search | `ISearchQuery`, `IFilterQuery`, `ISortQuery`, `IPageQuery`, attributes (`[Searchable]`, `[NotSortable]`…) | Attribute-driven query model reused by ASP.NET filters and OpenAPI. |
| Templating | `ITemplateEngine`, `ITemplateProvider`, `ITemplateSource`, `IFileType` | Engine → providers (Handlebars/XSLT/…) → sources (files). |
| I/O devices | `IDevice`, `IDeviceFactory`, segmenters, `IPipelineBuildDefinition` | Serial/USB/HID and binary framing (heritage of BinaryDataDecoders). |

---

<!-- nav -->
[↑ 01 — Architecture: How the Solution Is Put Together](./README.md) · [← Architecture § 6 — Runtime Composition Model (How a Call Finds Its Implementation)](./06-runtime-composition-model.md) · [Architecture § 8 — Hosting Model →](./08-hosting-model.md)
<!-- nav -->
