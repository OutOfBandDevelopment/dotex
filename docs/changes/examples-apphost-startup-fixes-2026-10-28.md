# Example AppHost startup fixes

**Date:** 2026-10-28 | **Status:** Complete (builds and CGI/OpenAPI unit tests pass; the full AppHost start was not rerun after the last change)

## Changes

- The vector dacpac publish is skipped when `[embedding].[HashPlanes]` already exists. DacFx rebuilt the table on every upgrade and scripted the persisted computed column over the CLR vector type as NVARCHAR (error 4936). Reset the `oobd-aspire-sqlserver` volume to take schema changes.
- `example-api` gets `ASPNETCORE_URLS` set to the port Aspire assigns. Under Visual Studio the child inherited `ASPNETCORE_URLS` (5000/5001), which outranks `ASPNETCORE_HTTP_PORTS`, so the host proxy and the web site's `/scalar`, `/openapi` and `/api` calls hung.
- The vector queue reader gets `ConnectionStrings__EmbeddingSentenceTransformer` from the Aspire-hosted `VectorDb` instead of the `127.0.0.1:1433` value in `appsettings.json`.
- The CGI example (`greet.exe`, `OoBDev.AspNetCore.Cgi.OpenApi`) and the device-decoding plan were merged into `main` with these.

## Verification

- Solution build: no compile errors (only the lock on the running `OoBDev.Example.WebApi.exe`).
- `OoBDev.AspNetCore.Cgi.OpenApi.Tests` 5 passed, `OoBDev.AspNetCore.Cgi.Tests` 10 passed.
- The API built from this tree lists `/cgi-bin/greet.exe` in `/openapi/all.json` and serves the greet form.
