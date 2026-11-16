# Feature request: framework wish list from the old overview

- **Status:** triaged
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
