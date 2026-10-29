# CGI latency gate (2026-10-29)

`Get_TrivialScript_StaysUnderTheLatencyGate` (Simulate, `OoBDev.AspNetCore.Cgi.Tests`) applies the testing-rule gate to the CGI middleware: a request over 500 ms is printed, one at 750 ms or more fails the test. Verified on Windows and in the Linux container (`scripts/ci/run-linux-tests.ps1`: 44 passed, 1 skipped Windows-only batch test). The search endpoints still need their gate (backlog).
