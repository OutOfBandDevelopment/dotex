# OoBDev.Spike.GraphQl

## Summary

Spike: can HotChocolate expose search through the existing `ISearchProvider`, `SearchTextParser` and `IApplicationRightChecker`? Result and verdict: [the spike report](../../../docs/patterns-discovery/08-spikes/04-graphql.md). Throwaway evidence, not packaged, and not part of `OoBDev.slnx`.

## Contents

| File | Purpose |
|------|---------|
| `GraphQlApplicationRightAttribute.cs` | Field attribute running `IApplicationRightChecker` as middleware |
| `SearchQuery.cs` | `search` (text syntax) and `searchTyped` (typed filter) with `first`/`after` paging |
| `GraphQlSpikeTests.cs` | Five unit tests: text syntax, typed filter, cursor, forbidden, unauthenticated |

## Running

```bash
cd src/Framework
dotnet test ../OoBDev.Spikes.slnx
```
