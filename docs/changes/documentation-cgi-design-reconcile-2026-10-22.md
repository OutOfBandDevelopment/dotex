# CGI middleware design reconciled with the code

**Date:** 2026-10-22 · **Compared:** `docs/design/CgiMiddleware/*` and `README.AspNetCore.Cgi.md` against `src/Extensions/OoBDev.AspNetCore.Cgi` at the commit that added this file.

## Design updated to match the code

- Overview: the project exists; architecture and API design are marked Implemented.
- API design: `UseCgi` lives in `ServiceCollectionExtensions` (there is no `ApplicationBuilderExtensions`); `DefaultSection`, the section-name parameter, `CgiOptionsValidator`, `InheritedEnvironmentVariables`, the `ScriptRights` default, the real resolver and runner signatures (`Resolve`, `StartAsync`), the `CgiException` subtypes, and 401/404/500 in the error model.
- Architecture: `StartAsync` in the sequence, `REQUEST_SCHEME`, the headers that are not passed on (`Authorization`, `Content-Type`, `Content-Length`), the authorization rows in the failure table, the new options in the configuration list.
- Requirements: the Linux fixture decision is done.

## Design wanted, code lacks

`PATH_TRANSLATED` is not set; it is marked Planned in the meta-variable table and added to `BACKLOG.md`.

## Verification

`OoBDev.Docs.Tests` run on `docs/design/CgiMiddleware` (see the commit).
