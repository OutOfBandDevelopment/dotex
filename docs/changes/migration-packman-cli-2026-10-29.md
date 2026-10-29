# BinaryDataDecoders PackMan CLI (2026-10-29)

Ported the PackMan CLI as `src/Tools/OoBDev.PackMan.Cli` (tool command `oobdev-packman`) with `OoBDev.PackMan.Cli.Tests` (20 Unit tests, all passing).

## Decisions

- The source's reflection-based option mapping and `ServiceCollection` wiring (about 250 lines) were replaced by `PackManOptions`, parsed with `Microsoft.Extensions.Configuration.CommandLine` like the Xslt CLI.
- The version lookup sits behind `IPackageVersionSource` so the upgrade rules are tested without a network. The two copy-pasted upgrade branches of the source became one rule set.
- The project update logic moved out of `Program` into `ProjectUpdater`; `Program` is wiring only.
- `-v` now prints the tool version; the source threw `NotSupportedException`.
- Package versions: `NuGet.Protocol` was added to `src/Directory.Packages.props`. `System.Security.Cryptography.Pkcs` is referenced directly so the tool does not take the vulnerable 8.0.1 transitive version (NU1902, NU1903).
- Logging uses `[LoggerMessage]`; no DI container (the tool needs none).

## Not run

The lookup against a NuGet feed (`NuGetFeedVersionSource`) was not run, since it needs the network. The tool was not packed or installed as a dotnet tool.
