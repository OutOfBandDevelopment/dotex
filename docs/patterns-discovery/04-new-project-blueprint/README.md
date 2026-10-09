# 04 — New Project Blueprint

Step-by-step recipes for building new things the same way as the existing code. Each recipe uses the [architecture](../01-architecture/README.md), the [design patterns](../02-design-patterns/README.md) and the [practices](../03-practices-and-conventions/README.md); design work before coding follows the [design document standard](../06-design-document-standard.md).

Choose a recipe:

- a new **capability** (a reusable feature with a default implementation) → [Recipe 1](./01-new-framework-capability.md)
- a new **vendor adapter** for an existing capability → [Recipe 2](./02-new-vendor-adapter.md)
- a new **application or product** on the framework → [Recipe 3](./03-new-application.md)
- a **new framework family** in a new repository → [Recipe 4](./04-new-framework-repository.md)
- a **worker or command-line tool** (generic host, hosted service) → [Recipe 5](./05-new-worker-or-cli.md)

Recipes 1 to 3 are also available as `dotnet new` templates (`oobdev-capability`, `oobdev-adapter`, `oobdev-webapp`); see [templates/README.md](../../../templates/README.md).

<!-- toc:start -->
## Contents

1. [Recipe 1 — New Framework Capability](./01-new-framework-capability.md)
2. [Recipe 2 — New Vendor Adapter](./02-new-vendor-adapter.md)
3. [Recipe 3 — New Application](./03-new-application.md)
4. [Recipe 4 — New Framework Family in a New Repository](./04-new-framework-repository.md)
5. [Recipe 5 — New Worker or Command-Line Tool](./05-new-worker-or-cli.md)

### List of Figures

1. [Figure 1 — steps for a new capability](./01-new-framework-capability.md)
2. [Figure 2 — where the adapter sits](./02-new-vendor-adapter.md)
3. [Figure 3 — request flow through a new application](./03-new-application.md)
4. [Figure 4 — shape of a worker or tool](./05-new-worker-or-cli.md)

### List of Tables

1. [Table 1 — Repository scaffold](./04-new-framework-repository.md)
<!-- toc:end -->
