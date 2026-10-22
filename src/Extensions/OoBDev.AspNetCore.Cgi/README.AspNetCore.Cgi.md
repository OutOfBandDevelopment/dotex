# OoBDev.AspNetCore.Cgi

Opt-in ASP.NET Core middleware that runs scripts as CGI/1.1 programs (RFC 3875).

## Description

Requests under a prefix (default `/cgi-bin`) map to scripts in a directory. Each request starts one child process with a cleared environment, the CGI meta-variables and the request body on stdin. The program's header block and body become the HTTP response. Design: `docs/design/CgiMiddleware`.

## Usage

```csharp
builder.Services.TryAddCgiServices(builder.Configuration);
// ...
app.UseAuthentication();
app.UseAuthorization();   // authorization is the host's job: put it ahead of UseCgi
app.UseCgi();
```

```json
{
  "OoBDev": {
    "Cgi": {
      "Prefix": "/cgi-bin",
      "ScriptRoot": "C:/cgi",
      "Handlers": { ".py": "python3", ".sh": "/bin/sh" }
    }
  }
}
```

## Safe defaults

- Only extensions listed in `Handlers` run (403 otherwise); `.cmd` and `.bat` also need `AllowBatchFiles`.
- No shell: the script path is a single argument; request data reaches the program only through the environment and stdin.
- Names with `..`, separators or links leaving `ScriptRoot` are refused; `ScriptRoot` must exist and not be under the static file root (startup fails otherwise).
- Request bodies over `MaxRequestBodyBytes` (10 MB) get 413; runs past `Timeout` (30 s) are killed with their child processes (504).
- `MaxConcurrentProcesses` (2 x CPU) caps parallel processes; extra requests get 503 with `Retry-After`.
- Headers containing CR or NUL, a missing blank line, or no `Content-Type`/`Location` give 502; a non-zero exit with no output gives 500. Self-produced errors are RFC 9457 problem details.
- `Proxy` and `Authorization` request headers are not passed on.

## Not included

Batch files re-parse the environment with cmd.exe rules, so a value containing `&` can run commands; that is why `AllowBatchFiles` is off by default.

Non-parsed-header (NPH) scripts, running as another user (use the deployment's service account) and authorization.
