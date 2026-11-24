# OoBDev.AspNetCore.Mvc

ASP.NET Core building blocks for the framework's REST APIs: authenticated-by-default authorization, application rights, OpenAPI documents with Scalar, the HTTP `QUERY` verb, the shared search query, correlation and culture middleware, health checks and the AsyncAPI endpoints. JWT validation is in [OoBDev.AspNetCore.JwtAuthentication](../OoBDev.AspNetCore.JwtAuthentication/README.AspNetCore.JwtAuthentication.md); [Common.AspNetCore](../../Common/OoBDev.Common.AspNetCore/README.Common.AspNetCore.md) wires both.

## Use

```csharp
services.TryAddAspNetCoreExtensions(new AspNetCoreExtensionBuilder
{
    RequireAuthenticatedByDefault = true,
    ApplicationUserIdClaim = CommonClaims.UserId, // null: the application has no user id
    AuthorizationPolicyBuilder = policy => policy.RequireClaim("scope", "api1"),
});
services.TryAddRoleRightMapping(configuration);                    // optional, see below

app.UseAspNetCoreExtensionMiddleware(healthCheckPath: "/health");
app.UseHttpQueryVerb();                                            // optional: HTTP QUERY
app.MapControllers();
app.MapApiReference();                                             // /openapi/{document}.json and /scalar/{document}
app.MapAsyncApi();                                                 // /asyncapi/{document}.json and a viewer
```

In Debug builds the builder and `healthCheckPath` parameters are required, in Release they default (the `#if DEBUG` pattern).

**Table 1 — Entry points**

| Method | Does |
|--------|------|
| `TryAddAspNetCoreExtensions(builder)` | Health checks, OpenAPI, search query, the CultureInfo accessor, `HttpContext` based `IPrincipal`, `IIdentity`, `ClaimsPrincipal` and `ICurrentUserAccessor` (keyed `http`; the environment accessor stays as `environment`), the correlation header feature, and `AddRequireAuthenticatedUser` when `RequireAuthenticatedByDefault` is true |
| `AddRequireAuthenticatedUser(applicationUserIdClaim, policy)` | Default and controller filter policy: authenticated user plus `UserAuthorizationRequirement`, then your policy callback |
| `TryAddCommonOpenApiExtensions()` or `(controllerAssemblies)` | One OpenAPI document per controller assembly (`/openapi/{AssemblyName}.json`) and an `all` document, with the XML documentation, search query, permission and health check transformers |
| `TryAddAspNetCoreSearchQuery()` | Search query filter, `ISearchQuery` accessor, `ISearchModelMapper` and `ISearchModelBuilder` |

The `OOBDEV_SHOW_PII=true` environment variable turns on identity model personal data in logs outside Production; in Production it is ignored with a logged warning.
| `TryAddRoleRightMapping(configuration)` | Role to application right mapping, below |
| `TryAddCultureNegotiation(configuration)` | `OoBDev:Culture` options: `DefaultCulture` and `SupportedCultures`; `Accept-Language` is matched by quality, an unsupported culture gets the default, and `Content-Language` returns the result |
| `TryAddProblemDetailsExceptionHandler(configuration)` | `ProblemDetailsExceptionHandler` (an `IExceptionHandler`) and `OoBDev:ProblemDetails` options. `UseProblemDetailsExceptionHandler()` (call first) turns an unhandled exception into an RFC 9457 500 with `traceId`, `correlationId`, `requestId` and `instance`, never the exception message; `IncludeErrorLocation` (default false) adds `errorLocation` (exception type, method, file, line) for development and test |
| `UseHttpUsageMetrics()` | Opt-in `HttpUsageMetricsMiddleware`: counter `oobdev.http.requests` (route template, method, status class), counter `oobdev.http.timeouts` (408, 504 or a `TimeoutException`) and histogram `oobdev.http.request.duration` on the meter `OoBDev.AspNetCore.Mvc`; the route is the template, never the URL with identifiers |
| `MapScreenViews(screens, path)` | `POST /api/v1/analytics/screen-views` with `{ "screen": "name" }`: counts `oobdev.ui.screen_views` (meter `OoBDev.AspNetCore.Mvc.Ui`, tags screen and authenticated); a name outside the known list gets problem details 400, so tag values cannot be inflated. Chain `RequireAuthorization()` and a rate limit as the host needs |
| `UseAspNetCoreExtensionMiddleware(path)` | Health check endpoint (default `/health`; call after `UseAuthentication`), correlation info, culture and search query middleware. The health body depends on the caller: anonymous gets `status` only, authenticated gets the check list, and the right `health.details` adds descriptions, data and errors. The OpenAPI document shows the mapped path |
| `UseHttpQueryVerb()` | `QueryVerbMiddleware` for HTTP `QUERY` |
| `MapApiReference(configure)` | OpenAPI endpoints plus Scalar listing every document; each `IApiReferenceConfigurator` (for example the OAuth2 one) and then `configure` shape the options |
| `MapAsyncApi()` | `/asyncapi/{document}.json` and an HTML viewer at `/asyncapi/{document}` loading the web component from `AsyncApi:ViewerScriptUrl` (default a pinned unpkg script); 404 for an unknown document |

## Application rights

`TryAddRoleRightMapping(configuration)` maps role claims to `app__application_right` claims, so code checks rights (`[ApplicationRight("orders.read")]`) and never role names. The mapping is configuration:

```json
{
  "OoBDev": {
    "Authorization": {
      "RoleClaimTypes": [ "roles" ],
      "Roles": { "order-manager": [ "orders.read", "orders.approve" ] }
    }
  }
}
```

Unmapped roles grant nothing. Granted rights are logged at Debug as counts only. Details: [ApplicationRights](../../../docs/design/ApplicationRights/README.md).

## Paging links

When a search action returns a paged result with more than one page, `SearchQueryResultFilter` adds an RFC 8288 `Link` header with `first`, `prev`, `next` and `last` (`PagingLinkHeader`). The path and the other query parameters are kept; only `currentPage` changes.
