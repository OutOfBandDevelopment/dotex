# Feature request: SQL Server Service Broker support

- **Status:** triaged
- **Requested by:** owner (filed from the notes repository, `projects/dotex-consolidation/sql-service-broker-tools.md`)
- **Date:** 2026-10-28
- **Area:** `OoBDev.MessageQueueing` (suggested `OoBDev.MessageQueueing.SqlServiceBroker`); dotex decides the name

## Need

Queue-based messaging inside SQL Server with Service Broker, using the same abstractions dotex uses for other queues. The owner has older prototype tools (a message broker CLI
and database project, a WPF modeller, T-SQL examples) in the `proving-grounds` repository, `examples/OoBDev.MessageBroker`, `examples/Ssb.Modeling` and `examples/SQL`. The
notes say they date from the SQL Server 2008 era and the likely origin is the Apress book *Pro SQL Server 2008 Service Broker*, not the owner's own code. Licence and reuse
rights were not checked.

## Proposed behavior

Send and receive Service Broker messages through the existing queue sender and receiver contracts, selected by configuration like the other providers. One concrete example:
configure a connection string and a queue name, send a message, and a hosted receiver handles it and replies.

Details of the contract mapping are unknown and need the maintainers' view of `OoBDev.MessageQueueing`.

## Acceptance criteria

- Given a SQL Server container with Service Broker enabled, when a message is sent through the provider, then the receiver gets the same body once.
- Given a handler that throws, when a message is received, then it is retried or dead-lettered by a defined rule (unknown; decide in design).
- Given the database deployed from a dacpac, when the Service Broker objects are part of the model, then the deployment creates the queues and services.
- Given the integration test stack, when the suite runs, then the Service Broker tests are in the Integration category.

## Existing code checked

- `docs/harvest-candidates.md` lists these prototypes, and the incoming recommendation plan (decision D7) already puts Service Broker tools on the evaluate-later list in `BACKLOG.md`.
- `OoBDev.MessageQueueing` and `OoBDev.Communications.MessageQueueing` exist; `OoBDev.ComplexEvents.DatabaseExtensions` has post-deployment scripts. I did not check whether any of them already covers Service Broker.
- `OoBDev.DacFx` builds dacpacs; whether its model carries Service Broker objects is unknown to me. The notes say DACPAC models Service Broker.

## Security and operations

Needs a SQL Server login that may send and receive on the queue, and Service Broker enabled on the database. Queue storage lives in the database. Cost: one SQL Server in
the test stack (a SQL Server container already exists there).

## Priority

- **Impact:** nothing is blocked; the existing queue providers work.
- **Needed by:** no date.
- **Workaround today:** use the RabbitMQ, Azure or SQS providers.

## Open questions

- Reuse any of the old prototype code, or write fresh against current abstractions?
- Where does it sit relative to the desired-state database tooling request (queues are one of its non-schema features)?
- Is the book-derived code usable at all?

## Triage notes

2026-10-29: accepted. Decisions (reversible): write fresh against current abstractions, not from the prototypes (book origin, licence unchecked); new adapter `OoBDev.SqlServer.ServiceBroker` mirroring `OoBDev.RabbitMQ`; the desired-state tooling request stays separate and may later own the broker objects. Design: [docs/design/SqlServiceBroker](../../design/SqlServiceBroker/README.md). Backlog: BACKLOG.md, messaging.
