# Build warnings clean (2026-10-29)

- `NU1902` (MessagePack 2.5.302, advisory GHSA-qhrr-8q5h-9q3h) arrives through Aspire.Hosting in `OoBDev.Example.AppHost`; 3.x would be a major jump under Aspire's RPC layer, so the advisory is suppressed in that project (`NuGetAuditSuppress`) with the reason: a local development tool, not shipped, no untrusted input. Remove when Aspire updates MessagePack.
- `CA1852` on `MongoDispatchProxy` (unsealed so `DispatchProxy.Create` can derive from it) is suppressed in the file with the reason.
- The `#warning` markers are gone (MailKit hosting works in all builds; the queue-test notes were removed earlier).

Verified: clean rebuild of `OoBDev.Example.AppHost` and `OoBDev.MongoDB.Tests` with 0 warnings.
