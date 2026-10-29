# DevTerm schemas

Date: 2026-10-29. Branch `dev/example-ui-tests`.

- New `OoBDev.DevTerm.Schemas` (Extensions, executable): generates JSON Schemas for the device manifest, UI definition and connection profile from the C# model. Committed output in `schemas/devterm/`.
- 4 tests in `OoBDev.DevTerm.Schemas.Tests` passing, including the drift check (committed schemas equal the generated text).
- Difference from dev-term: schemas are no longer regenerated on every Debug build; run the tool and commit the result. The `$id` host is open question Q2.
