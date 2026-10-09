# OpenApiScalar — API Design

[← Architecture](architecture.md) · [Testing strategy →](testing-strategy.md)

## Registration

```csharp
// services
services.TryAddAspNetCoreExtensions(configuration);      // includes TryAddCommonOpenApiExtensions()
services.TryAddJwtBearerServices(configuration);         // adds the oauth2 scheme and Scalar sign-in

// pipeline
app.MapControllers();
app.MapApiReference();                                    // /openapi/{name}.json and /scalar/{name}
```

`TryAddCommonOpenApiExtensions(IEnumerable<Assembly>)` takes the controller assemblies explicitly (used by the tests); the parameterless overload discovers them.

## Public types

**Table 1 — Namespace `OoBDev.AspNetCore.Mvc.OpenApi`**

| Type | Role |
|------|------|
| `OpenApiDocumentCatalog` | Document names (`all` + assemblies); `DiscoverControllerAssemblies()`, `From(...)` |
| `ConfigureOpenApiOptions` | `IConfigureNamedOptions<OpenApiOptions>`: filter, schema ids and transformer registration |
| `IApiReferenceConfigurator` | `Configure(ScalarOptions)`; implemented by libraries that need to change the reference |
| `ApiInfoDocumentTransformer`, `HealthChecksDocumentTransformer` | Document transformers |
| `ApplicationPermissionsOperationTransformer`, `SearchQueryOperationTransformer`, `XmlDocumentationOperationTransformer` | Operation transformers |
| `SearchQuerySchemaTransformer`, `XmlDocumentationSchemaTransformer` | Schema transformers |
| `XmlDocumentationProvider` | Reads member comments from the XML files beside the application |
| `AddMvcFilterOptions<T>`, `ApiNamespaceControllerModelConvention` | MVC filter registration and API group per assembly |

`EndpointRouteBuilderExtensions.MapApiReference(Action<ScalarOptions>?)` maps the documents and the reference.

**Table 2 — Namespace `OoBDev.AspNetCore.JwtAuthentication.OpenApi`**

| Type | Role |
|------|------|
| `OAuth2OpenApiOptions` | `UserReadApiClaim`, `AuthorizationUrl`, `TokenUrl` |
| `OAuthDocumentTransformer` | Declares the `oauth2` authorization code scheme |
| `OAuthApiReferenceConfigurator` | Scalar authorization code flow with PKCE |
| `ConfigureOAuthOpenApiOptions` | Adds the document transformer |

## Configuration

```json
"OAuth2OpenApiOptions": {
  "UserReadApiClaim": "api_access",
  "AuthorizationUrl": "https://sts.example.com/auth",
  "TokenUrl": "https://sts.example.com/token"
}
```

The JwtBearer `Audience` is used as the client id. Keycloak clients need the `/scalar/{name}` redirect URIs (see `containers/keycloak/local-dev-realm.json`).

[← Architecture](architecture.md) · [Testing strategy →](testing-strategy.md)
