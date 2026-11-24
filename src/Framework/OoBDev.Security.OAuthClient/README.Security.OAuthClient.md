# OoBDev.Security.OAuthClient

Public-client OAuth 2.0 / OIDC for desktop and console applications. The library never opens a browser: the caller passes an `Action<Uri>`.

- `PublicOAuthClient` (`IOAuthClient`): authorization code with PKCE S256 on a loopback redirect (`LoopbackReceiver`), device code (RFC 8628), refresh, sign-out with revocation. Endpoints come from `{Issuer}/.well-known/openid-configuration`.
- `TokenCache` (`ITokenCache`): serves the stored token, refreshes within `Skew` of expiry, and shares one refresh between concurrent callers. `invalid_grant` clears the stored tokens and returns null (sign in again).
- `BearerTokenHandler`: adds the token to requests; after a 401 it refreshes once and retries once.
- `FileSecretStore` (`ISecretStore`): files under the user profile; replace the registration for a protected store.
- `Pkce`, `OAuthEndpoints`, `OAuthClientOptions` (section `OoBDev:OAuthClient`, validated on start).

```csharp
services.AddOAuthClient(configuration);
services.AddHttpClient("api").AddBearerToken();
```

Contracts: [Abstractions](../OoBDev.Security.OAuthClient.Abstractions/README.Security.OAuthClient.Abstractions.md). Design: [OAuthClient](../../../docs/design/OAuthClient/README.md).
