# Local package feeds

`scripts/ci/pack.ps1` writes the packages (the `.githooks/post-merge` hook runs it when a merge commit lands on `main`): NuGet packages into `G:\nuget`, and npm packages into `G:\npm` for any publishable (non-private, non-example) npm project, all with the one GitVersion version. There is no npm project to pack today.

Other projects consume the feeds through the user NuGet source `oobdev-local` (`dotnet nuget add source G:\nuget --name oobdev-local`)
and the user environment variables `LOCAL_NUGET` and `LOCAL_NPM`; install an npm package with `npm install "$env:LOCAL_NPM\<file>.tgz"`.
