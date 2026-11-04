# DevTerm web host API guide

**Date:** 2026-11-04

The REST and streaming guides are generated, not written: `ApiGuideTests` (`OoBDev.DevTerm.Web.Tests`, `Unit`) starts the web host, reads `/openapi/v1.json` and `/asyncapi.json`, and `ApiGuideRenderer` (`OoBDev.DevTerm.TestUtilities`) writes `docs/applications/devterm/api-guide/rest.md` and `streaming.md`. Output is ordered and port-free, and `WriteIfChanged` leaves the files alone when nothing changed, so reruns leave no churn.

Verified: the test passes and the guides list every operation, channel and message. Not covered: the ksy binary-format guide and the `src/Tools` guides (still in `BACKLOG.md`).
