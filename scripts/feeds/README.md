# Publish scripts

`publish-packages.ps1` packs every packable .NET project into `G:\nuget` and every npm project into `G:\npm`
(`-NugetPath`, `-NpmPath`, `-SkipNuget`, `-SkipNpm` override).

Version: `{major}.{minor}.{commits since the last version tag}` from GitVersion (git depth), with the branch label as the
prerelease part off `main` (for example `0.5.170-dev-example-app-fixes`). It reads only the local clone.

Other projects consume the feeds through the user NuGet source `oobdev-local` (`dotnet nuget add source G:\nuget --name oobdev-local`)
and the user environment variables `LOCAL_NUGET` and `LOCAL_NPM`; install an npm package with `npm install "$env:LOCAL_NPM\<file>.tgz"`.
