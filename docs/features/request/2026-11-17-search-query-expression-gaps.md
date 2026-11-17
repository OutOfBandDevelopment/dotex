# Feature request: search and filter query expression gaps

- **Status:** triaged
- **Requested by:** owner (source `TODO` comments, collected 2026-11-17)
- **Date:** 2026-11-17
- **Area:** `OoBDev.System.Linq` (`ExpressionTreeBuilder`), `OoBDev.AspNetCore.Mvc` (search query binding and OpenAPI), `OoBDev.System` (expression parser)

## Need

The search query to expression tree builder has known gaps that surface as wrong or unsupported queries: no `like` for strings, no `IEnumerable<>` arguments (arrays only), no type casting, no null-safe member access, and untyped JSON values. The query model binder cannot bind `Dictionary<string, object>` from forms or query strings, and the OpenAPI operation ignores filters.

## Proposed behavior

Close each gap or decline it with a reason: string `like`; `IEnumerable<>` in addition to arrays; casts through a type converter on `FilterParameter`; null-safe member chains; enumerate the requested type instead of guessing from the JSON type; unroll `searchOption`; dictionary binding; filters described in OpenAPI; the expression parser rejects `"1e"`.

## Acceptance criteria

- Given a string filter with `like`, then the expression uses a contains/like comparison.
- Given an `IEnumerable<T>` argument, then it works like an array.
- Given a null in a member chain, then the filter does not throw.
- Given the input `1e`, then the expression parser raises a parse error.
- Given a filter parameter, then the OpenAPI operation documents it.

## Existing code checked

`docs/design/SearchMerge`, `HybridSearch`, `ODataQuery`, `HttpQueryVerb`: they define the query surface but none lists these builder gaps.

Source comments (replaced by a reference to this request):

- `src/Framework/OoBDev.System.Linq/Expressions/ExpressionTreeBuilder.cs` (8): "add support for these operations"; 'should be "like"'; "needs to be a bit more creative. type casting not supported"; "should support IEnumerable<> as well"; "make null safe"; "consider this being a type converter on FilterParameter instead"; "logic to enumerate the requested type instead of assuming the type based on the json type"; "should I add support to unroll the searchOption?".
- `src/Framework/OoBDev.AspNetCore.Mvc/Providers/SearchQuery/SearchModelMapper.cs`: "dictionary <string, object> does not map for forms and query string".
- `src/Framework/OoBDev.AspNetCore.Mvc/OpenApi/SearchQueryOperationTransformer.cs`: "ignore filter support for now".
- `src/Framework/OoBDev.System.Tests/ExpressionCalculator/Parser/ExpressionParserTests.cs`: `"1e"` should throw a parse error (test case commented out).
- `src/Framework/OoBDev.System.Tests/Linq/QueryableExtensionsTests.cs`: equality on a double index `"0.0"` not supported (test case commented out).

## Security and operations

Query input is untrusted: every added operation needs the same limits on depth and size as the existing ones. None otherwise.

## Priority

- **Impact:** some valid filters fail or return wrong results.
- **Needed by:** no date.
- **Workaround today:** use supported operators.

## Open questions

Which gaps to build first? Recommendation: `like`, null safety, `IEnumerable<>`.

## Triage notes

Triaged 2026-11-17 into [BACKLOG.md](../../../BACKLOG.md) section 5 (source TODO comments). Design document to follow when built.
