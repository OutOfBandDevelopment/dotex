# OoBDev.AspNetCore.Cgi.OpenApi

Lists CGI programs in the host application's OpenAPI documents (opt-in companion of `OoBDev.AspNetCore.Cgi`).

## Description

A CGI program describes itself with a `{program}.openapi.json` file in the script root (`greet.exe` and `greet.cgi` share `greet.openapi.json`). `CgiOpenApiDocumentTransformer` reads those files and publishes their paths under the CGI prefix. Design: `docs/design/CgiMiddleware` (OpenAPI listing).

## Usage

```csharp
builder.Services.TryAddCgiServices(builder.Configuration, "OoBDev:Cgi");
builder.Services.TryAddCgiOpenApi(builder.Configuration, "OoBDev:Cgi:OpenApi");
```

```json
{ "OoBDev": { "Cgi": { "OpenApi": { "FileSuffix": ".openapi.json", "Documents": [ "all" ] } } } }
```

- Paths in the file are relative to the program: `/` is the program, `/{id}` adds `PATH_INFO`. They appear as `/cgi-bin/greet.exe` and `/cgi-bin/greet.exe/{id}`.
- Operations without tags get the program name as a tag.
- A description with no program next to it, or one that is not valid OpenAPI, is skipped with a warning.
- A path already in the document is kept; the clash is logged.

A program can generate its own file at build time; the Go example in `OoBDev.Example.WebApi` runs `greet -openapi`.
