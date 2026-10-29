# SQL Server Service Broker — Requirements

[↑ Overview](README.md) · [Architecture →](architecture.md)

**Status:** Draft · **Date:** 2026-10-29

## Contents

- [Goals and non-goals](#goals-and-non-goals)
- [Requirements](#requirements)

### List of Tables

1. [Table 1 — Requirements](#table-1)

## Goals and non-goals

Goals: transactional queueing inside the database the application already has; one more provider behind the existing contracts.

Non-goals: copying the prototype CLI or WPF modeller, cross-database or cross-instance routing in the first version, conversation-group ordering across messages.

## Requirements

<a id="table-1"></a>**Table 1 — Requirements**

| ID | Requirement | Priority |
|----|-------------|----------|
| REQ-001 | `IMessageSenderProvider.SendAsync` sends the wrapped message (same `WrappedQueueMessage` JSON as the other providers) with `SEND ON CONVERSATION` and returns the correlation id | Must |
| REQ-002 | `IMessageReceiverProvider.RunAsync` loops on `WAITFOR (RECEIVE …)` with a timeout and cancels cleanly | Must |
| REQ-003 | Each message is received and its handler run inside one transaction; a handler that completes commits, so the body is delivered once | Must |
| REQ-004 | The provider is selected by the key `sql-service-broker` and configured with a connection string, queue name, service name and contract | Must |
| REQ-005 | A handler that throws rolls back and the message is retried up to a configured count, then ended with an error and logged (poison rule; broker disables the queue after five rollbacks, so the provider counts first) | Must |
| REQ-006 | Service Broker disabled or the queue missing yields a clear configuration exception | Should |
| REQ-007 | The broker objects (message type, contract, queue, service) can be created by a script the provider runs on demand and are part of a dacpac model if `OoBDev.DacFx` carries them | Should |
| REQ-008 | An AsyncAPI contributor describes the channel, as in the other adapters | Should |
| REQ-009 | Tests are Integration (SQL Server container) with Unit tests for the non-I/O parts | Must |
