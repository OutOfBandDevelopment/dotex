# Owner decisions (always apply)

- **UI:** MVVM with command binding. Commands are real, retestable `ICommand` types with injected dependencies, not lambdas. For JS/TS pick frameworks with view-model binding and note where they diverge from MVVM.
- **Authentication:** OAuth 2.0 / OIDC / JWT bearer. Applications validate tokens and never store passwords. Services understand only JWT; the STS or an edge adapter converts any other credential (API key, HMAC, basic, Kerberos/NTLM, SAML). RFC 8693 token exchange is optional and config-gated. See `docs/patterns-discovery/03-practices-and-conventions/09-authentication-practices.md`.
- **HTTP and authorization:** REST first, OpenAPI is the contract, errors are RFC 9457 problem details. OData over HTTP `QUERY` (with `POST` fallback) and GraphQL are under evaluation beside the custom search syntax, with no breaking change to it. RBAC with application rights (`[ApplicationRight]`); roles are translated to rights in middleware; same checks on every API surface (docs 10 and 11 in that folder).
- **Dependencies:** see the `coding-standards` rule. Owner answers are recorded under each verdict in `docs/patterns-discovery/05-industry-alternatives/`.
- **Scope:** all BinaryDataDecoders features are migrated (phases are priority order, not selection). Check `C:\repo\mine\binary-data-decoders` before migrating, since it can be ahead of `Incoming/BinaryDecoders`.
- **Process:** read `/TODO.md` (the `Backlog:` sections) before starting work, and update it when work finishes.
