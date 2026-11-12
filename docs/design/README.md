# Design documents

One folder per feature, written to the [design-document standard](../patterns-discovery/06-design-document-standard.md): requirements, architecture, API design and testing strategy. Living documents, updated with the code. Application designs are under [docs/applications](../applications/README.md), protocol references under `docs/protocols`, and finished work under [docs/changes](../changes/README.md).

**Table 1 — Features**

| Feature | Summary |
|---------|---------|
| [AllMiniLmL6V2](AllMiniLmL6V2/README.md) | Plan to replace the unmaintained fork `src/ExternalServices/AllMiniLML6v2Sharp` (a fork of someone else's work, about 1,250 lines, with `Microsoft.ML` and `libtorch-cpu-win-x64` dependencies) with a first-party, thread-safe implementation that runs **the same  |
| [ApplicationRights](ApplicationRights/README.md) | Role-based access control with application rights. |
| [AsyncApi](AsyncApi/README.md) | Publishes an AsyncAPI 3.0 document for the message queue surfaces (SQS, Service Bus, RabbitMQ, in-process) and a viewer page, so queue contracts are as discoverable as the REST contracts. |
| [CgiMiddleware](CgiMiddleware/README.md) | An opt-in ASP.NET Core middleware that runs CGI/1.1 ([RFC 3875](https://www.rfc-editor.org/rfc/rfc3875)) programs as child processes, so a Kestrel application can serve legacy scripts and small native tools without changing Kestrel. |
| [Communications](Communications/README.md) | The Communications Platform provides a **multi-channel communications orchestration system** that enables applications to send messages through various channels (Email, SMS, Push Notifications) with: |
| [ConnectionProfiles](ConnectionProfiles/README.md) | Saved connection profiles and how they are chosen at startup. |
| [ContractParser](ContractParser/README.md) | ContractParser is a proposed Domain-Specific Language (DSL) and parser for describing service contracts using a text-based format with ANTLR4 grammar. |
| [DataEnhancement](DataEnhancement/README.md) | The Data Enhancement Pipeline is a **generic, reusable framework** for enriching data through attribute-discovered providers. |
| [DesiredStateDatabase](DesiredStateDatabase/README.md) | Derive migrations from plain `CREATE` scripts for SQL Server, Postgres, SQLite and Oracle, and manage the objects a schema diff cannot model (extensions, queues, event streams) with idempotent ensure scripts. |
| [DevTermParity](DevTermParity/README.md) | Owner requirement (2026-10-29): the OoBDev version of dev-term has 100% parity with dev-term, every protocol and every feature. |
| [DeviceDecoding](DeviceDecoding/README.md) | Protocol decoders for hardware devices (DE-5000, Busylight, K8055, NMEA, Radex One, Zoom H4n, later SCPI and Z-Wave), migrated from dev-term as one package per device on the existing `OoBDev.System.IO` device model. |
| [DocumentServices](DocumentServices/README.md) | Design documents for the document-processing services of the SharedFramework (Epic 6). |
| [ImageAnalysis](ImageAnalysis/README.md) | Histograms, colour facets and exposure metrics as pure functions over RGB buffers; placement waits for the owner. |
| [MediaMetadata](MediaMetadata/README.md) | Read-only EXIF reader and tag dump for images (JPEG, PNG, WebP, TIFF), merged from the storage catalog; placement waits for the owner. |
| [GeneratedGuides](GeneratedGuides/README.md) | A user or API guide is never edited by hand to stay current. |
| [HttpQueryVerb](HttpQueryVerb/README.md) | Lets a client send a query as a request body with the HTTP `QUERY` verb (safe, idempotent), with a `POST` fallback for clients and proxies that cannot send `QUERY`. |
| [HybridSearch](HybridSearch/README.md) | Hybrid search is a means to combine multiple search providers into a single correlated result. |
| [ImageEmbeddings](ImageEmbeddings/README.md) | First-party, thread-safe image embedding and classification on ONNX Runtime, using the same pattern as the text embedders: one generic runner, small model presets, model files downloaded on first use into the shared Hugging Face hub cache, and every preset com |
| [Jws](Jws/README.md) | A small JSON Web Signature (RFC 7515) signer and verifier written from the specification, so the signing and verification steps are understood and testable instead of hidden behind a library. |
| [LlmCodeGen](LlmCodeGen/README.md) | A single command-line tool, `OoBDev.LlmCodeGen.Cli`, that walks a source tree, builds one prompt per folder from a template, sends it to a configured LLM provider and saves the response. |
| [MasterData](MasterData/README.md) | Infrastructure for loading, versioning and managing reference data (master data) and for loading and cleaning up test data (Epic 05). |
| [MessageComposition](MessageComposition/README.md) | Orchestrates template rendering, data enhancement and document conversion to produce pre-formatted messages ready for delivery (Epic 12). |
| [MessageQueuing](MessageQueuing/README.md) | This directory contains the documentation for message queue provider architecture used throughout the OoBDev framework. |
| [MultiSessionUi](MultiSessionUi/README.md) | Several live sessions in one window or screen. |
| [NetworkDevices](NetworkDevices/README.md) | Finding network devices and configuring serial-to-Ethernet bridges. |
| [ODataQuery](ODataQuery/README.md) | Lets any `IQueryable<T>` endpoint answer the OData system query options (`$filter`, `$orderby`, `$top`, `$skip`, `$select`, `$count`) beside the custom search syntax, on `GET` and, through the HTTP `QUERY` verb, with the options in a form body. |
| [OpenApiScalar](OpenApiScalar/README.md) | Swashbuckle is removed. |
| [PluginSandbox](PluginSandbox/README.md) | A .NET 10 host that runs plugins written in any language as isolated processes, with approved permissions, brokered access to external resources, network isolation and resource limits. |
| [Plugins](Plugins/README.md) | Plugin contracts, discovery, isolation and what plugins can contribute. |
| [Presenters](Presenters/README.md) | Text, numeric, protocol-decoding and rendering presenters that turn a transport's bytes into something readable. |
| [ProjectState](ProjectState/README.md) | Saving and restoring a set of connections. |
| [ScpiInstruments](ScpiInstruments/README.md) | Instrument control over SCPI. Moved from the DevTerm design folder on 2026-11-04. |
| [Ser2NetWin](Ser2NetWin/README.md) | Ser2NetWin bridges Windows COM ports to TCP (raw or RFC 2217, optionally advertised over mDNS). |
| [SessionChannel](SessionChannel/README.md) | Sharing a live session across processes over a named pipe. |
| [SessionLogging](SessionLogging/README.md) | The versioned JSON Lines session-log format, the passive session observer that records it and the playback engine. |
| [SharedCaching](SharedCaching/README.md) | Design documents for the caching features of the SharedFramework (Epic 4). |
| [SharedFramework](SharedFramework/README.md) | This directory contains **implementation-ready design documentation** for SharedFramework features. |
| [SharedIdentity](SharedIdentity/README.md) | Design documents for the identity features of the SharedFramework (Epic 7). |
| [SqlServiceBroker](SqlServiceBroker/README.md) | Send and receive messages through SQL Server Service Broker using the existing `IMessageSenderProvider` and `IMessageReceiverProvider` contracts, selected by configuration like the RabbitMQ, SQS and Service Bus providers. |
| [StreamTools](StreamTools/README.md) | Detecting what a captured stream contains and converting it. |
| [TemplateTools](TemplateTools/README.md) | Backlog item: stack review of `OoBDev.Xslt.Cli` and `OoBDev.TemplateEngine.Cli` (owner, 2026-10-30). |
| [TextTemplating](TextTemplating/README.md) | 1/ **Handlebars Template Provider** - Industry-standard template support |
| [Theming](Theming/README.md) | Light, dark, system and user-defined JSON themes with semantic color roles, shared by the WPF and Terminal.Gui front ends. |
| [Transports](Transports/README.md) | The `ITransport` contract and the transports built on it (serial, TCP, HID, USBTMC, BLE, RFC 2217, VXI-11, MQTT, AMQP, STOMP, loopback). |
| [UiDefinitions](UiDefinitions/README.md) | A framework-agnostic, serializable model for declaring a control panel once so every front end renders it generically, plus the no-code device manifest and the control-module model built on it. |
| [VectorSearch](VectorSearch/README.md) | This page adds support to MS SQL Server for Vector Search Functionality. |
| [WebFrontend](WebFrontend/README.md) | Web-accessible host service with WebSocket tunnels and a Blazor front end. |
| [WindowsForms](WindowsForms/README.md) | `ValidTextBox` (a `TextBox` whose background shows whether the text matches a regex filter) and its `ValidationEventArgs`, carried over from the BinaryDataDecoders `OoBDev.Windows.Forms` library. |
| [ZWave](ZWave/README.md) | Z-Wave support through the Serial API. Moved from the DevTerm design folder on 2026-11-04. |
| [example-ui](example-ui/README.md) | Part of [the Example UI plan](example-ui/plan.md). |
