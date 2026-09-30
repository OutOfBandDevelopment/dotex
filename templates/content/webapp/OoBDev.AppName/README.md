# OoBDev.AppName

Web API composed from `OoBDev.Common.Complete`.

- Infrastructure: one `TryAllCommonExtensions` call and `UseAllCommonMiddleware`.
- Provider selection: configuration keys `OoBDev::ServiceKeys::{FullTypeName}`.
- Add only the configuration sections this application needs, and document them in `CONFIGURATION_SETTINGS.md`.
- Add a tests project and `.runsettings` variables for any integration dependencies.
