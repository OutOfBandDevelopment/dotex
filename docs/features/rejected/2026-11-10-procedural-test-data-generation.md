# Feature request: procedural test data generation

- **Status:** declined (owner, 2026-11-19, Q19: closed as stale; test data uses the CSV utility project, reference designs are covered by the patterns blueprint; reopen a line by filing a new request)
- **Requested by:** owner (SharedFramework comparison, 2026-11-10)
- **Date:** 2026-11-10
- **Area:** `src/Framework` (test utilities and generation library)

## Need

Create realistic object graphs for tests and demos from attributes on a type or interface, with a seed so runs repeat. dotex has no generator.

## Proposed behavior

Attribute-driven generation (`GenerateString`, `GenerateInteger`, `GenerateDateTime`, `GenerateEnumeration`, collection, array, nullable, interface and abstract types, plus address, email, name, phone, words), a seeded context, a provider builder and a dispatch proxy that builds interface implementations.

## Source projects

Under `C:\repo\mine\proving-grounds\examples\SharedFramework` (read-only reference):

- `OoBDev.Generations.Abstractions` (21 files)
- `OoBDev.Generations` (25 files)
- `OoBDev.Generations.Extensions.DependencyInjection`
- `OoBDev.Generations.Tests`

## Acceptance criteria

- Given a seed and a type, then two runs produce identical data.
- Given an interface, then a proxy returns generated values for its members.
- Given the same type with a different seed, then the data differs.

## Existing code checked

Searched `src` for the generation interfaces and `GenerateObject`: nothing. A `Bogus` or `AutoFixture` mention exists only in some DevTerm tests (check how it is used); those libraries are the third-party alternative and the owner leans against new third-party dependencies.

## Security and operations

Test data only. Never generate values that look like real personal data or real credentials.

## Priority

- **Impact:** less hand-built test data; also useful in the example apps.
- **Needed by:** no date
- **Workaround today:** inline objects, or `Bogus` where already used

## Open questions

Take this over, or use `Bogus` (the owner's dependency rule applies)? Does it share the CSV test data utility request's project?

## Triage notes

Origin: SharedFramework. Compare with the built-in alternatives before accepting.

2026-11-11: triaged. Accepted as a backlog item: design first (`docs/patterns-discovery/06-design-document-standard.md`), no code until a consumer appears. Backlog: BACKLOG.md, SharedFramework requests.

2026-11-19: proposed design written: [TestDataGeneration](../../design/TestDataGeneration/README.md).
