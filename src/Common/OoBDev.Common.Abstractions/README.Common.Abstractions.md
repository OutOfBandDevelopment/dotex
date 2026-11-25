# OoBDev.Common.Abstractions

Aggregator package for the contracts of the framework: it holds no code and references every `OoBDev.*.Abstractions` project under `src/Framework` by an MSBuild glob (and removes the ones under `src/Extensions`), so a consumer that only defines or consumes contracts takes one reference.

The implementations are in [OoBDev.Common](../OoBDev.Common/README.Common.md); everything, including ASP.NET Core, hosting, external services and hardware devices, is in [OoBDev.Common.Complete](../OoBDev.Common.Complete/README.Common.Complete.md).
