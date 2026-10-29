# SQL Server Service Broker Queue Provider — Overview

**Status:** Draft · **Date:** 2026-10-29 · **Request:** [SQL Server Service Broker support](../../features/request/2026-10-28-sql-service-broker-tools.md) · **Related:** [RabbitMQ provider](../../../src/ExternalServices/RabbitMQ/OoBDev.RabbitMQ/README.RabbitMQ.md), [design standard](../../patterns-discovery/06-design-document-standard.md)

Send and receive messages through SQL Server Service Broker using the existing `IMessageSenderProvider` and `IMessageReceiverProvider` contracts, selected by configuration like the RabbitMQ, SQS and Service Bus providers.

## Contents

1. [Requirements](requirements.md)
2. [Architecture](architecture.md)
3. [API design](api-design.md)
4. [Testing strategy](testing-strategy.md)

### List of Tables

1. [Table 1 — Requirements](requirements.md#table-1)
2. [Table 2 — Alternatives](architecture.md#table-2)

### List of Figures

1. [Figure 1 — Components](architecture.md#figure-1)

## Summary

A new adapter project `OoBDev.SqlServer.ServiceBroker` (ExternalServices) mirrors `OoBDev.RabbitMQ`: a `ServiceBrokerQueueMessageProvider` implementing both provider interfaces, a connection factory, `TryAddServiceBrokerQueueServices()` and an AsyncAPI contributor. It uses `Microsoft.Data.SqlClient` (already central). The old 2008-era prototypes in proving-grounds are not reused: their origin is a book and licence is unchecked, so the provider is written fresh against current abstractions.

Open items are in the design: poison-message rule (REQ-005) and whether the dacpac model carries the broker objects (REQ-007).
