# OoBDev dotnet templates

`dotnet new` templates that implement the recipes in [docs/patterns-discovery/04-new-project-blueprint](../docs/patterns-discovery/04-new-project-blueprint/README.md).

| Short name | Recipe | Creates | Run from |
|------------|--------|---------|----------|
| `oobdev-capability` | [1](../docs/patterns-discovery/04-new-project-blueprint/01-new-framework-capability.md) | `OoBDev.<Name>.Abstractions`, `OoBDev.<Name>`, `OoBDev.<Name>.Tests` | `src/Framework` |
| `oobdev-adapter` | [2](../docs/patterns-discovery/04-new-project-blueprint/02-new-vendor-adapter.md) | `OoBDev.<Vendor>.<Capability>` and `.Tests` | `src/ExternalServices/<Vendor>` |
| `oobdev-webapp` | [3](../docs/patterns-discovery/04-new-project-blueprint/03-new-application.md) | Web API composition root | `src/Examples` or your app folder |

Recipe 4 (a new framework repository) is not a template; it is a copy of the repository root files listed in its document.

## Install

```bash
dotnet new install ./templates/content/capability
dotnet new install ./templates/content/adapter
dotnet new install ./templates/content/webapp
# or as one package
dotnet pack templates -o artifacts && dotnet new install artifacts/OoBDev.Templates.*.nupkg
```

Uninstall with `dotnet new uninstall <path or OoBDev.Templates>`.

## Use

```bash
cd src/Framework
dotnet new oobdev-capability -n Notifications        # OoBDev.Notifications*

cd ../ExternalServices && mkdir Twilio && cd Twilio
dotnet new oobdev-adapter -n Twilio --capability Notifications   # OoBDev.Twilio.Notifications*

cd ../../Examples
dotnet new oobdev-webapp -n MyApp                    # OoBDev.MyApp
```

The templates are in-repository templates: the generated projects rely on `src/Directory.Build.props` and use relative project references, so they must be created inside `src/`. Add the generated projects to `OoBDev.sln` afterwards (`dotnet sln src/OoBDev.sln add ...`).

## Verify

`scripts/templates/verify-templates.ps1` installs the templates, generates each into the source tree, builds and tests them, then removes the output.
