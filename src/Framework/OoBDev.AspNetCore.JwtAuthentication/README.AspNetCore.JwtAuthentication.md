# OoBDev.AspNetCore.JwtAuthentication

JWT bearer authentication for ASP.NET Core, bound from configuration, plus the OAuth2 sign-in for the OpenAPI documents and the Scalar API reference. Services understand only JWT (owner decision); the STS or an edge adapter converts any other credential.

## Use

```csharp
services.TryAddJwtBearerServices(configuration);                       // namespace OoBDev.AspNetCore.JwtAuthentication
services.TryAddJwtBearerServices(configuration, new JwtExtensionBuilder { DefaultSchema = "Bearer" });
```

In Debug builds the builder is a required parameter, in Release it defaults to null (the `#if DEBUG` pattern). `TryAddJwtBearerServices` calls the two parts, which can also be used alone:

**Table 1 — Registrations**

| Method | Does |
|--------|------|
| `TryAddJwtBearerAuthentication(configuration, defaultScheme, section)` | Binds `JwtBearerOptions` from the section (default `JwtBearerOptions`) and adds authentication with the JWT bearer handler as the default scheme (`Bearer`) |
| `TryAddJwtBearerOpenApi(configuration, section)` | Binds `OAuth2OpenApiOptions` from the section (default `OAuth2OpenApiOptions`) and adds `ConfigureOAuthOpenApiOptions` (registers `OAuthDocumentTransformer`) and `OAuthApiReferenceConfigurator` |

`JwtExtensionBuilder` holds `DefaultSchema`, `JwtBearerConfigurationSection` and `OAuth2OpenApiConfigurationSection`.

## Configuration

```json
{
  "JwtBearerOptions": { "Authority": "https://sts.example/realms/app", "Audience": "my-api" },
  "OAuth2OpenApiOptions": {
    "UserReadApiClaim": "api.read",
    "AuthorizationUrl": "https://sts.example/realms/app/protocol/openid-connect/auth",
    "TokenUrl": "https://sts.example/realms/app/protocol/openid-connect/token",
    "ClientId": "my-api-docs"
  }
}
```

`UserReadApiClaim`, `AuthorizationUrl` and `TokenUrl` are required; `ClientId` is optional.

## OpenAPI and Scalar

- `OAuthDocumentTransformer` declares the `oauth2` authorization code security scheme, with the scope from `UserReadApiClaim`, in every OpenAPI document.
- `OAuthApiReferenceConfigurator` configures the Scalar reference to sign in with the authorization code flow and PKCE (SHA-256). The client id is `ClientId`, else the JWT `Audience`; when neither is set it logs a warning (event 1) and leaves Scalar unconfigured.

The documents and Scalar are mapped by `MapApiReference()` from [OoBDev.AspNetCore.Mvc](../OoBDev.AspNetCore.Mvc/README.AspNetCore.Mvc.md): `/openapi/{AssemblyName}.json`, the `all` document and `/scalar/{document}`.
