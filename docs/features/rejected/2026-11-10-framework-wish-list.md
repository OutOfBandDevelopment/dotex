# Feature request: framework wish list from the old overview

- **Status:** declined (owner, 2026-11-19, Q19: closed as stale; test data uses the CSV utility project, reference designs are covered by the patterns blueprint; reopen a line by filing a new request)
- **Requested by:** owner (kept from `docs/Framework/MajorFunctionality.md`, removed 2026-11-10)
- **Date:** 2026-11-10
- **Area:** unknown (many libraries)

## Need

The old hand-written framework overview ended with a wish list. Some entries are built since (blob and document services, the correlation and health work may overlap `OoBDev.Telemetry`); none was triaged. They are kept here verbatim so triage can sort each into a design document, `BACKLOG.md`, or a decline.

## Proposed behavior

Triage each entry; the original text follows.

## Acceptance criteria

- Given each entry below, when triaged, then it is merged into a design document and the backlog, or declined with a reason.

## Existing code checked

Not done. Start with `docs/design/DocumentServices`, `docs/design/SharedCaching`, `OoBDev.Telemetry` and `BACKLOG.md` section 8 (the owner wish list).

## Security and operations

Unknown per entry.

## Priority

- **Impact:** unknown
- **Needed by:** no date
- **Workaround today:** none

## Open questions

Which entries are still wanted?

## Triage notes

Original text (headings lowered by one level):

### Proposed Features


- [ ] Create common chatbox integration
- [ ] Look at creating a global notification preferences model
- [ ] Create bulk user management
- [ ] Create application roles/claims management
- [ ] enable client side event bus support using signal/socket.io/websockets

- [ ] Event Scheduler
  - [ ] assign default rules as attribute based on event generator
    - [ ] allow overriding though configuration
    - [ ] possibly allow extensions to override the schedule provider
  - [ ] provide means to know last run time so the system can catch up
  - [ ] add support for NCrontab but also allow other options for recurrence   calculation such as RRule
    * https://github.com/atifaziz/NCrontab
    * https://icalendar.org/rrule-tool.html

- [ ] Document Management and Conversion

  - [ ] normalize blob storage
  - [ ] https://github.com/okhosting/awesome-storage
  - [ ] https://github.com/topics/blob-storage
  - [ ] upload/download support
  - [ ] restore support for transforming documents 
  - [ ] example, markdown to html, html to pdf
  - [ ] add support to create/read zip files

- [ ] Resource Translation
  - [ ] add support for server side translation.  key/culture => value mapping
  - [ ] allow for use from within the text engine

- [ ] Demo Kubernetes Cluster
  - [ ] Integration Test Examples
  - [ ] DAPR

- Tools
  - [ ] DACPAC Merge
  - [ ] DACPAC extentions/translations for other databases
  - [ ] MongoDB extensions for Entity Framework

- [ ] Design Pattern Examples
  - [ ] Microservices
  - [ ] Business Events
  - [ ] [SideCar](https://learn.microsoft.com/en-us/azure/architecture/patterns/sidecar)
  - [ ] [Ambassador](https://learn.microsoft.com/en-us/azure/architecture/patterns/ambassador)

#### Analytics and Health monitoring

* add support for checking the status of dependent services like message queues, email servers and the like
* add context correlation support to allow flowing a correlation ID from the client to dependent services and logs
  * https://stackoverflow.com/questions/56068619/should-i-use-request-id-x-request-id-or-x-correlation-id-in-the-request-header
  * https://devcenter.heroku.com/articles/http-request-id
  * https://www.rapid7.com/blog/post/2016/12/23/the-value-of-correlation-ids/
  * https://hilton.org.uk/blog/microservices-correlation-id
  * https://stackoverflow.com/questions/25433258/what-is-the-x-request-id-http-header

### Known Issues

- [ ] Tech Debt: Filters on Date for 'equals to' don't work
- [ ] Create example dotnet template set
- [ ] Add data redaction extension for query engine

### SBOM references (for the [SBOM request](../completed/2026-11-10-sbom-report.md))

* https://spdx.dev/use/tools/
* https://github.com/microsoft/sbom-tool
* https://github.com/CycloneDX/cyclonedx-dotnet

2026-11-11: triaged. Accepted as a backlog item: design first (`docs/patterns-discovery/06-design-document-standard.md`), no code until a consumer appears. Backlog: BACKLOG.md, SharedFramework requests.

## Provisional triage (2026-11-19, nothing closed; Q19 decides which lines are wanted)

| Line | Finding | Where it goes |
|------|---------|---------------|
| Event Scheduler (defaults, catch-up, cron or RRule) | designed | [ComplexEvents](../../design/ComplexEvents/README.md) open points |
| Resource translation, use from the text engine | `LocalizedTemplateEngine` resolves `name.{culture}` (2026-11-16); key/culture value store not built | candidate for a design if still wanted |
| Dependent-service health checks | `AddHealthChecks` and `HealthCheckPath` exist in `OoBDev.AspNetCore.Mvc`; per-dependency checks not audited | check against the adapters, then backlog or close |
| Correlation id flow | `CorrelationInfoHttpPrepareRequestFeature` exists; end-to-end with `OoBDev.Telemetry` not audited | check, then close or backlog |
| Example dotnet template set | `templates/` pack exists (`oobdev-capability`, `oobdev-adapter`, `oobdev-webapp`) | looks done; confirm and close |
| Document management and conversion (blob normalisation, zip, markdown to html) | partly built (`OoBDev.Documents*`, DocumentToMarkdown) | [DocumentServices](../../design/DocumentServices/README.md) |
| Chat box, notification preferences, bulk user and roles management, client event bus | not designed | keep open until wanted (Q19) |
| DACPAC merge and extensions, MongoDB EF extensions | DACPAC tooling exists (`OoBDev.DacPacCompiler.Cli`); the rest not designed | [DesiredStateDatabase](../../design/DesiredStateDatabase/README.md) for DACPAC; others keep open |
| Demo Kubernetes cluster, DAPR, design pattern examples | not designed | keep open until wanted (Q19) |
| Known issues: date equals filter, data redaction in the query engine | not reproduced here | reproduce with a test first, then backlog |
