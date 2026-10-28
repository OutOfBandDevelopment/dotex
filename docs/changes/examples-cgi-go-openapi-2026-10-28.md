# CGI example in Go and the OpenAPI listing

**Date:** 2026-10-28 · **Branch:** `dev/cgi-example` · **Design:** [CGI middleware](../design/CgiMiddleware/README.md)

## What changed

- `OoBDev.AspNetCore.Cgi.OpenApi` (new, opt-in): `CgiOpenApiDocumentTransformer` lists CGI programs that ship a `{program}.openapi.json` in the OpenAPI documents under the CGI prefix. `TryAddCgiOpenApi` registers it for the documents named in `CgiOpenApiOptions.Documents` (default `all`).
- `OoBDev.Example.WebApi` uses `OoBDev.AspNetCore.Cgi` (`TryAddCgiServices`, `UseCgi`, an `OoBDev:Cgi` section) and serves the Go program `cgi-src/greet`: a form that sends the submitted name back (HTML-escaped).
- `greet -openapi` prints the program's own OpenAPI description; `cgi-src/build.ps1` runs the Go tests, builds `greet.exe` into `cgi-bin/` and writes `cgi-bin/greet.openapi.json` from that output.
- Feature request intake for others: `docs/features/request/` (template and triage process), linked from the root `README.md` and the `work-tracking` rule.

## Verified

- `dotnet test src/Extensions/OoBDev.AspNetCore.Cgi.OpenApi.Tests`: 5 passed (paths under the prefix, tags, description without a program, invalid description, existing path kept).
- `go test` for `greet`: passed (form, round trip, escaping, OpenAPI shape).
- Example web API built and started on Windows (Kestrel, port 5277): `GET /cgi-bin/greet.exe` returned the form, `POST name=Ada <b>` returned `Hello, Ada &lt;b&gt;!`, and `/openapi/all.json` listed `/cgi-bin/greet.exe` with `get` and `post` (request body requires `name`).
- Docs validation for the changed folders: 4 of 4 passed.

## Not done

- The Linux build (`greet.cgi`) was not run. `build.ps1` and the Go tests run on the host, not in a container.
- No automated test starts the example application; the check above was by hand.
