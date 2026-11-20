# User analytics (screen usage, URLs, timeouts)

**Status:** proposed, not built. **Last Updated:** 2026-11-20. Origin: owner wish list (`BACKLOG.md` section 8). Waiting on [Q28](../../../OPEN-QUESTIONS.md).

## Requirements

- Count screen views, API calls by route template, and timeouts (request, query and hub timeouts) so the owner can see what is used and what is slow.
- Aggregate only: route template (`/api/things/{id}`), not the URL with identifiers; a user is counted, never named in a metric tag (rule "Keep cardinality low" in [observability practices](../../patterns-discovery/03-practices-and-conventions/13-observability-practices.md)).
- Built on the platform primitives already chosen: `Meter` and `IMeterFactory`, exported with OpenTelemetry (`OoBDev.Telemetry`); no third-party analytics SDK.
- Off unless configured; no cost without a listener.

## Architecture

| Signal | Source | Instrument |
|--------|--------|------------|
| API calls | an ASP.NET middleware in `OoBDev.AspNetCore.Mvc`, after routing | counter `oobdev.http.requests` tagged route template, method, status class; histogram of duration |
| Timeouts | `OperationCanceledException` from a timeout token and the 408 and 504 results | counter `oobdev.http.timeouts` tagged route template |
| Screen views | the web front end sends one `POST /api/analytics/screen` per route change with the screen name | counter `oobdev.ui.screen_views` tagged screen name, authenticated or not |

The screen endpoint takes only a screen name from a known list (the router table), is rate limited and needs no right beyond authentication, so it cannot be used to inflate arbitrary tag values.

## Testing

Unit: a `MeterListener` observes the counters for a fake request and for a timeout; the screen endpoint rejects an unknown name. No Integration test is needed until the OTLP export is asserted (the Aspire dashboard exists for that).

## Open

See Q28 in `OPEN-QUESTIONS.md`: whether per-user usage (who used what) is wanted, which carries privacy duties (consent, erasure).
