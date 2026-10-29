# DevTerm device manifests

Date: 2026-10-29. Branch `dev/example-ui-tests`.

- New `OoBDev.DevTerm.DeviceManifests` (Framework): manifest model, loader, validator, writer, catalog, control surface, reply and frame presenters, `.ksy` importer, sample data; the pre-packaged loopback manifest.
- 168 ported tests in `OoBDev.DevTerm.DeviceManifests.Tests` passing; the `.ksy` tests now read the files in each hardware device's `docs` folder.
- Manifest editor (`Editing`: editor view model, forms, expression picker) ported; 168 tests in total pass.
