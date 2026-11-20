# Event schema registry

**Status:** Built 2026-11-20 (first slice) · **Feature:** [CQRS and event sourcing](README.md) · **Requirement:** R7 (schema changes are handled by upcasting on read)

Event payloads are versioned contracts. The registry derives a JSON Schema from every registered event class, checks stored or incoming payloads against it, and compares the current schemas with a committed baseline so a breaking change cannot ship without a version bump.

## Decisions

- The class is the source of truth; no schema is hand-written. `System.Text.Json.Schema.JsonSchemaExporter` produces the schema, with no third-party package.
- `[EventType("name", Version = 2)]` carries the contract version (default 1). The stored event type name does not change with the version; upcasters convert old payloads on read.
- Validation is strict deserialization (unmapped members refused, required members and nullability respected), so the check and the reader agree.
- Compatibility is judged per property: an added optional property is compatible; a removed property, a type change, a newly required property or a property that became required are breaking.
- A baseline file (`event-schemas.json`, produced by `EventSchemaRegistry.ToSnapshot()`) is committed by the consumer. `EventSchemaBaseline.Verify` returns a problem for a breaking change without a higher version, a lowered version, and a baseline type that is no longer registered.

## Not built

Persisting the registry in the store, per-type security claims and property masking from the source design, and linking an upcaster to a version pair (upcasters stay keyed by type name).

## Tests

Unit: schema exported per type and version, strict validation accepts and refuses payloads, each change kind classified, baseline verify (breaking without bump fails, with bump passes, removed type fails, version lowered fails).
