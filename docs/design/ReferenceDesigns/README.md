# Reference designs (common entity patterns)

Status: **proposed catalogue, written 2026-11-19 (Q19 default B: nothing closes); no code.** Request: [reference designs](../../features/rejected/2026-11-10-reference-designs.md). Q19 asks whether this is only the [patterns-discovery blueprint](../../patterns-discovery/README.md) under another name; this catalogue differs by naming entities, states and rights, not project layout.

## Contents

1. [Purpose](#purpose)
2. [Catalogue](#catalogue)
3. [Entry template](#entry-template)
4. [Rules](#rules)

### List of Tables

1. Table 1 — Catalogue

## Purpose

A short list of entity patterns that applications reuse, so a new application starts from a named design instead of inventing users, contacts or workflows again. Each entry is a design page (entities, states, rights), not code.

## Catalogue

**Table 1 — Catalogue**

| Pattern | Entities | States | Rights (`[ApplicationRight]`) | Existing design |
|---------|----------|--------|-------------------------------|-----------------|
| User profile | `UserProfile`, `ProfileModule`, `ProfileValue` | active, disabled | `profile.read`, `profile.write`, `profile.admin` | [ModularProfileManagement](../SharedIdentity/ModularProfileManagement/requirements.md) |
| Contact | `Contact`, `ContactMethod` (email, phone, address), `ContactGroup` | active, merged, archived | `contacts.read`, `contacts.write` | none |
| Scheduled work | `ScheduleItem`, `ScheduleRun` | pending, leased, completed, failed | `schedules.read`, `schedules.manage` | [ComplexEvents](../ComplexEvents/README.md) |
| State machine | `StateDefinition`, `Transition`, `StateHistory` | defined per machine | one right per guarded transition | none |
| Workflow | `WorkflowDefinition`, `WorkflowInstance`, `WorkItem` | defined, running, waiting, completed, cancelled | `workflow.start`, `workflow.approve` | none |

*Table 1 lists the patterns; only the first two rows have, or need, a dedicated design page first (user profile exists; contact is the smallest new one).*

## Entry template

Each pattern gets `docs/design/ReferenceDesigns/<Pattern>.md` with: purpose, entities and relationships (a PlantUML class diagram), state diagram, the rights it needs and who holds them, the audit and erasure behaviour (see [erasure](../CqrsEventSourcing/erasure.md)), and the tests a consumer should copy.

## Rules

- A reference design names no vendor and no host; it is realised as a Framework library plus an EF Core store when a consumer needs it.
- State changes are events (event-sourced where the application already uses it); a transition is guarded by a right, checked by the shared right checker.
- A pattern with no consumer stays a page; nothing is built ahead of a consumer.
