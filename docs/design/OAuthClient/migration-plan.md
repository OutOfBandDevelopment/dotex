# OAuth client: migration plan (confirmed 2026-11-19)

Status: confirmed by the owner 2026-11-19 (Q1, option A); the table may be built.

**Table 1 — Proposed projects**

| Project | Layer folder | Namespace | Package | Contents | References |
|---------|--------------|-----------|---------|----------|------------|
| `OoBDev.Security.OAuthClient.Abstractions` (new) | Common | `OoBDev.Security.OAuthClient` | `OoBDev.Security.OAuthClient.Abstractions` | `IOAuthClient`, `ITokenCache`, `ISecretStore`, `OAuthTokens`, `OAuthException` | none |
| `OoBDev.Security.OAuthClient` (new) | Framework | `OoBDev.Security.OAuthClient` | `OoBDev.Security.OAuthClient` | PKCE, discovery, loopback listener, device flow, `TokenCache`, `FileSecretStore`, `BearerTokenHandler`, options, `ServiceCollectionExtensions` | the abstractions, `OoBDev.System`, `Microsoft.Extensions.Http` |
| `OoBDev.Security.OAuthClient.Tests` (new) | Framework | same + `.Tests` | none | unit tests, Keycloak Integration tests | the library, `OoBDev.TestUtilities` |
| `OoBDev.Security.OAuthClient.Windows` (new) | Extensions | `OoBDev.Security.OAuthClient.Windows` | `OoBDev.Security.OAuthClient.Windows` | `DpapiSecretStore` | the abstractions, `System.Security.Cryptography.ProtectedData` |
| `OoBDev.Security.OAuthClient.Windows.Tests` (new) | Extensions | same + `.Tests` | none | Windows-only unit tests | the Windows library |

## References that change

None in existing projects at creation. Later items add a reference from each client (DevTerm Wpf, Tui, Web) to the library and register it in the client's composition root.

## Why these locations

- Host-free and generic names, so Framework (rule `layering-and-migration-plans`: reusable means Framework). The library has no ASP.NET reference; `Microsoft.Extensions.Http` is not a web host.
- DPAPI needs a Windows-only package, so it is a leaf in Extensions, never in the Framework library.
- The abstractions are split into Common so a client or adapter can depend on the contract only (the Abstractions-only reference rule).

## Not decided

- Whether the file store belongs in the Framework library or in the Windows leaf's sibling. Recommendation: keep it in the library; it is portable and the test double.
