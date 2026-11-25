# Feature request: reference domain applications (an event-sourced ledger first)

- **Status:** declined (2026-11-25)
- **Requested by:** owner
- **Date:** 2026-11-25
- **Area:** `docs/design/`, `src/Examples` or `src/Applications` (new), `docs/patterns-discovery`

## Need

The Example application shows single features, each as a one-off use. It does not show the framework working as one compiled flow: a domain application whose pieces (event store, scheduling, documents, messaging, rights and audit, telemetry) fit together end to end. A complete application on a domain people already know, in the way of the "Nerd Dinner" sample, would show what the framework is for and would be a reference for the owner's own future products. Such an application has a high bar: it must be real enough to teach, and tested.

## Proposed behavior

1. A design, in the repository's feature-document standard, for a reference domain application, with the harvested `AccountingEngine` (proving-grounds, about 1,000 lines, no tests) as the source of domain rules only: loan account, typed transaction ledger, payment clearing order, day-count interest, late-fee dunning, amortization schedule.
2. The ledger is event sourced: transactions are immutable events, the balance is a projection, corrections are compensating entries; monthly billing and dunning run as scheduled work; statements and letters are generated documents; posting a credit or refund needs an application right and is audited.
3. Beyond the first one, the same request covers whether other full applications are wanted as references, each with a design, tests and user guides generated from journey runs.

## Acceptance criteria

- Given the design documents, when read, then each framework piece used (event store, schedule, documents, messaging, rights, audit, telemetry) is named with the reason it is there.
- Given a backdated payment, when posted, then the balance and the dunning result are recomputed deterministically from the events (fixed `TimeProvider`).
- Given the same events replayed, when projected, then the result is identical.

## Existing code checked

- `AccountingEngine` (read 2026-11-25): `IBillingEngine`, `IClearingEngine`, `IDunningEngine`, `IAmortizationEngine`, `ITaxEngine`, `ITerminationEngine`; `Account` and `Transaction` models; `decimal` money; `Financial.Pmt` from Visual Basic for installments; no tests, no documentation, one project. The rules are worth reading; the interfaces are tied to one fixed-terms loan shape and are not pluggable, so the code is not a base to extract.
- `docs/design/CqrsEventSourcing` (event store, projections) and the Schedule store cover the infrastructure; nothing in dotex models a domain ledger.
- `src/Examples/OoBDev.Example.Web` is a feature showcase of one-off uses, not a compiled flow; whether it overlaps an event-sourced flow was not checked.

## Security and operations

A financial domain brings rounding, idempotent posting, audit and replay. It is a teaching application with no real money or real accounts.

## Priority

- **Impact:** a worked reference for the whole framework; no feature is blocked without it.
- **Needed by:** no date
- **Workaround today:** the Example application for single features and the patterns documents for the architecture.

## Open questions

- Which domain is best: a loan ledger, or a smaller one such as ordering, inventory or booking (a first application needs fewer rules)?
- ~~Where does it live?~~ Answered (owner, 2026-11-25): domain-specific examples belong in child repositories that consume dotex as NuGet packages, not in this solution; the `src/Examples` project stays for single-feature uses.
- Does the AccountingEngine rules list stay the only source, or are real servicing documents wanted?

## Triage notes

2026-11-25: placement decided (owner): a child repository per reference application, consuming the dotex packages, which also tests that the packages are usable from outside the solution. Declined for now (owner): large build, no consumer, and the backlog is long. The idea stays: a full reference application with a high bar, much like Nerd Dinner, and possibly more than one. Reopen by moving this file back to `request/` with status `triaged`. Related: [harvest candidates](../../harvest-candidates.md) (AccountingEngine stays a read-only reference), [CQRS and event sourcing design](../../design/CqrsEventSourcing/README.md).
