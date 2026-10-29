# OoBDev.PackMan.Cli

A dotnet tool (`oobdev-packman`) that keeps `Directory.Packages.props` current: it sets property versions, raises `PackageVersion` items from a NuGet feed, and aligns the `PackageReference` versions of projects.

## Description

Source: BinaryDataDecoders `PackMan.Cli`.

```
oobdev-packman [-s <props>] [-o <output>] [-n] [--no-nuget] [-t <Name,1.2.3;...>] [--nuget-url <url>]
               [--nuget-feed-type <type>] [--get-properties] [--follow-imports] [-g <property>] [-v]
               [--update-projects --projects <glob;!glob> --projects-path <folder>]
```

| Switch | Meaning |
|--------|---------|
| `-s`, `--source` | Props file; defaults to `Directory.Packages.props` in the current folder |
| `-o`, `--output` | Save here instead of over the source |
| `-n`, `--no-save` | Do not save changes |
| `--no-nuget` | Do not look up newer versions |
| `-t`, `--target-versions` | Set property versions, `Name,1.2.3;Other,2.0.0` |
| `--nuget-url`, `--nuget-feed-type` | The feed (default nuget.org, `HttpV3`) |
| `--get-properties` | Print the properties of the props file |
| `--follow-imports` | Replace `Import` elements with the imported content |
| `-g`, `--get-property` | Print one property and stop |
| `-v`, `--get-version` | Print the tool version and stop |
| `--update-projects` | Set `PackageReference` versions of matching projects from the props file |
| `--projects`, `--projects-path` | Globs separated by `;` (a leading `!` excludes) and the folder they start from |

Version rules for `PackageVersion` items:

- An item without a `Condition` on its `ItemGroup` moves to the newest release.
- An item in a conditioned `ItemGroup` (for example one per target framework) stays on its major version and takes a prerelease only when it already is one.
- An item with `<_LockVersion>true</_LockVersion>`, or a version that is not a literal (`$(Property)`), is left alone.

| Type | Role |
|------|------|
| `PackManOptions` | Command line parser |
| `PackageProps` | Open, change, read and save the props document |
| `IPackageVersionSource`, `NuGetFeedVersionSource` | Version lookup, and the NuGet client implementation |
| `ProjectUpdater` | Align project references with the props file |
| `NuGetLoggerBridge` | Sends NuGet client messages to `Microsoft.Extensions.Logging` |

## Verified

Unit tests cover option parsing, the version rules with a fake version source, imports, saving and project updates. The NuGet feed lookup was not run against a feed (it needs network); `--no-nuget --get-properties` was run against this repository's props file.
