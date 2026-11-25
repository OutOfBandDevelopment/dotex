# OoBDev - Shared .Net Libraries

## Summary

This folder holds the shared libraries, applications and examples. Layers: Common, Framework, Extensions, HardwareDevices, ExternalServices, Applications, Tools and Examples (see [the architecture docs](../docs/architecture/README.md)).

## Useful Scripts

* `build.bat`, `build.sh` - build the solution into `publish/libs`
* `test.sh` - run the tests and merge the results into `LatestTestResults.trx`
* `../scripts/ci` - the manual CI (build, test, Docker integration stack); there is no hosted pipeline

Run `dotnet` commands from this folder: `Directory.Build.props` finds the solution directory from the working directory.
