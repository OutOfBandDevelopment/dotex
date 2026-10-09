# 03 — Practices and Conventions

The day-to-day rules the code follows: how things are named and laid out, how projects are built and packaged, how they are documented and tested, and how logging, errors and CI behave. These are the author's preferences as found in the code; the "why" lives in [Architecture](../01-architecture/README.md) and the recurring solutions in [Design Patterns](../02-design-patterns/README.md). Step-by-step use of these rules is in the [New Project Blueprint](../04-new-project-blueprint/README.md); alternatives are weighed in [Industry Alternatives](../05-industry-alternatives/README.md).

<!-- toc:start -->
## Contents

1. [Naming and Project Layout](./01-naming-and-layout.md)
2. [Build and Project File Conventions](./02-build-and-csproj.md)
3. [Documentation Practices](./03-documentation-practices.md)
4. [Testing Practices](./04-testing-practices.md)
5. [Logging, Errors and Configuration](./05-logging-errors-and-configuration.md)
6. [CI/CD and Versioning](./06-cicd-and-versioning.md)
7. [Known Warts (Decide Before Copying)](./07-known-warts.md)
8. [UI Practices (MVVM and Command Binding)](./08-ui-practices.md)
9. [Authentication Practices (OAuth, OIDC, JWT and Token Exchange)](./09-authentication-practices.md)
10. [HTTP API Practices (REST, Querying and GraphQL)](./10-http-api-practices.md)
11. [Authorization Practices (RBAC and Application Rights)](./11-authorization-practices.md)
12. [Security Practices (Beyond Authentication)](./12-security-practices.md)
13. [Observability Practices (Metrics, Tracing and Health Checks)](./13-observability-practices.md)
14. [Resilience Practices (Retries, Timeouts and Idempotency)](./14-resilience-practices.md)
15. [AI, Vector and RAG Practices](./15-ai-vector-rag-practices.md)

### List of Figures

1. [Figure 1 — how a test resolves configuration](./04-testing-practices.md)
2. [Figure 2 — pipeline from commit to package](./06-cicd-and-versioning.md)
3. [Figure 3 — MVVM roles and the direction of dependencies](./08-ui-practices.md)
4. [Figure 4 — exchanging an SSO token for an application token](./09-authentication-practices.md)
5. [Figure 5 — one rights check and one query seam behind three API surfaces](./10-http-api-practices.md)
6. [Figure 6 — role-to-right translation in middleware](./11-authorization-practices.md)
7. [Figure 7 — security controls from client to secret store](./12-security-practices.md)
8. [Figure 8 — instrumentation in libraries, export in the host](./13-observability-practices.md)
9. [Figure 9 — resilience as decorators around an interface](./14-resilience-practices.md)
10. [Figure 10 — ingestion and query paths](./15-ai-vector-rag-practices.md)

### List of Tables

1. [Table 1 — Project naming conventions](./01-naming-and-layout.md)
2. [Table 2 — Type naming conventions](./01-naming-and-layout.md)
3. [Table 3 — What the shared build files do](./02-build-and-csproj.md)
4. [Table 4 — Documentation artifacts](./03-documentation-practices.md)
5. [Table 5 — Test categories](./04-testing-practices.md)
6. [Table 6 — Configuration conventions](./05-logging-errors-and-configuration.md)
7. [Table 7 — Workflows](./06-cicd-and-versioning.md)
8. [Table 8 — Known warts](./07-known-warts.md)
9. [Table 9 — MVVM rules](./08-ui-practices.md)
10. [Table 10 — Authentication rules](./09-authentication-practices.md)
11. [Table 11 — HTTP API rules](./10-http-api-practices.md)
12. [Table 12 — Authorization rules](./11-authorization-practices.md)
13. [Table 13 — Security rules](./12-security-practices.md)
14. [Table 14 — Observability rules](./13-observability-practices.md)
15. [Table 15 — Resilience rules](./14-resilience-practices.md)
16. [Table 16 — AI building blocks](./15-ai-vector-rag-practices.md)
17. [Table 17 — AI rules](./15-ai-vector-rag-practices.md)
<!-- toc:end -->
