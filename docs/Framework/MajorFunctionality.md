# OoBDev - Shared Framework for .Net

## Summary

This is a collection of libraries designed to allow for faster and more consistent development using .Net 

## Major Features

### Message Queueing

See [Message Queueing](MessageQueueing.md)

### Text Templating

See [Text Templating](TextTemplating.md)

### ASP.Net Core Extensions

These extensions include

* Common query though IQueryable<T> support as controller actions
* SwaggerGen extensions for oauth2 authentication as well as enumeration of options for querying 
  * Tested to support Azure B2C and Keycloak providers
  * User management is tested for B2C
  * User management is in progress for Keycloak 
* CultureInfo mapping based on request/response headers
* Application Permission filtering based on Claims authorization
* Improved OAuth/JWT configuration support

### Communications 

See [Text Templating](CommunicationServices.md)

### MongoDB Extensions

See [MongoDB Extensions](MongoDbExtensions.md)

### Common Query Extension

Common query support is build into the base framework.  `OoBDev.System.Linq.Search`  This
extension also provides a common means for not only search and filter but also sorting and paging.

* want to add support for claims based predicates
* want to add support for claims based redaction/masking

### MS Test Extensions

`OoBDev.TestUtilities`

* current extensions only exist for easily attaching result content as attachments to the test results
* should add support to present the content back in the reports

## Tooling

### TextTemplating

`OoBDev.TemplateEngine.Cli`

* support exists for XSLT and Handlebars
* add support for T4?
* add support for razor?
* add support for custom HTML?
* add support for python?
* https://en.wikipedia.org/wiki/Template_processor
* add translation support
  * custom extensions with callbacks to allow translation a code value into a language based on a culture

### FixSourceLinking

`OoBDev.FixSourceLinks.Cli`

not needed as long as UseSourceLinks is not enabled in .runsettings for MSTest

### Software Bill of Materials

* https://spdx.dev/use/tools/
* https://devblogs.microsoft.com/engineering-at-microsoft/generating-software-bills-of-materials-sboms-with-spdx-at-microsoft/
* https://github.com/microsoft/sbom-tool
* https://www.howtogeek.com/devops/how-to-generate-an-sbom-with-microsofts-open-source-tool/
* https://github.com/topics/sbom-generator
* https://github.com/CycloneDX/cyclonedx-dotnet

## Proposed Features


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

### Analytics and Health monitoring

* add support for checking the status of dependent services like message queues, email servers and the like
* add context correlation support to allow flowing a correlation ID from the client to dependent services and logs
  * https://stackoverflow.com/questions/56068619/should-i-use-request-id-x-request-id-or-x-correlation-id-in-the-request-header
  * https://devcenter.heroku.com/articles/http-request-id
  * https://www.rapid7.com/blog/post/2016/12/23/the-value-of-correlation-ids/
  * https://hilton.org.uk/blog/microservices-correlation-id
  * https://stackoverflow.com/questions/25433258/what-is-the-x-request-id-http-header

## Known Issues

- [ ] Tech Debt: Filters on Date for 'equals to' don't work
- [ ] Create example dotnet template set
- [ ] Add data redaction extension for query engine
