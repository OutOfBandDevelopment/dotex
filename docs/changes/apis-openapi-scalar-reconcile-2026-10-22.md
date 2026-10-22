# OpenApiScalar design reconciled with the code

**Date:** 2026-10-22 · **Design:** [OpenApiScalar](../design/OpenApiScalar/README.md)

- REQ-001 to REQ-005 and REQ-008 were already covered by the document tests. REQ-006 (OAuth2 sign-in) and REQ-007 (title, version, description) had code but no tests; both now have Unit tests in `OoBDev.AspNetCore.Tests/OpenApi` (`OAuthApiReferenceConfiguratorTests`, `ApiInfoDocumentTransformerTests`) and rows in the design's test table.
- REQ-009 (AsyncAPI) is covered by the [AsyncApi](../design/AsyncApi/README.md) design.
- `OoBDev.AspNetCore.Tests` Unit suite: 20 of 20 passed. The Scalar sign-in flow was not exercised in a browser against the Keycloak container.
