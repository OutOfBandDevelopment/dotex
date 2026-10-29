# CGI tests: control characters in headers and child kill on timeout

**Date:** 2026-10-28
**Status:** Complete

Two Simulate tests added to `OoBDev.AspNetCore.Cgi.Tests` with shell fixtures `bad-header.sh` (REQ-008: a CR inside a header value gives 502) and `pid-sleep.sh` (REQ-010: after the 504 the child process no longer exists). Both run in the .NET SDK container (`scripts/ci/run-linux-tests.ps1`) and end `Inconclusive` on Windows.

Verified in the Linux container: 42 passed, 1 skipped (the batch-file opt-in test). The earlier `.sh` fixtures (`echo`, `location`, `no-blank-line`) ran there too. Not run on Windows.
