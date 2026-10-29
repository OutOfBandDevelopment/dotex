# CGI example: Linux build and an automated form test

**Date:** 2026-10-28
**Status:** Complete

Closes the "Not done" items of [examples-cgi-go-openapi-2026-10-28.md](examples-cgi-go-openapi-2026-10-28.md).

- `go test ./...` and `go build` for `cgi-src/greet` ran in the `golang:1.24` container: tests pass and `greet.cgi` builds and prints its OpenAPI description.
- `GreetExampleTests.Post_NameWithMarkup_ReturnsEscapedGreetingAsync` (Simulate, in `OoBDev.AspNetCore.Cgi.Tests`) serves the example `cgi-bin` through the CGI middleware, posts the form and checks the escaped greeting. It ends `Inconclusive` when the program is not built (`greet.exe` is committed; `greet.cgi` is git-ignored and built by `build.ps1`).

The test serves the program through the middleware with the example's settings; it does not start the whole example web API (that needs its database and identity setup).

Verified: passes on Windows (`greet.exe`) and in the Linux container (`greet.cgi`, 43 passed, 1 skipped).
