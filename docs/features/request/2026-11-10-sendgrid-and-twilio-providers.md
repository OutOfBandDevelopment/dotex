# Feature request: SendGrid email and Twilio SMS providers

- **Status:** triaged
- **Requested by:** owner (SharedFramework comparison, 2026-11-10)
- **Date:** 2026-11-10
- **Area:** `src/ExternalServices/Twilio` (new adapters), `OoBDev.Communications*` (Epic 2 Communications)

## Need

Send email through SendGrid and SMS through Twilio behind the Communications channel abstraction. dotex has MailKit for email and the channel contracts, but no cloud email or SMS adapter.

## Proposed behavior

Two adapters that implement `IChannelProvider` (names `sendgrid` and `twilio`), reference Abstractions only, and read credentials from configuration. A message mapper turns the channel message into the vendor request.

## Source projects

Under `C:\repo\mine\proving-grounds\examples\SharedFramework` (read-only reference):

- `OoBDev.Twilio.SendGrid` (`SendGridEmailHandler`, `EmailClient`, `EmailMessageMapper`, `MessageBuilder`)
- `OoBDev.Twilio.SmsMessaging` (`SendTwilioSmsHandler`, `SmsClient`)
- Matching `.Tests` projects.

## Acceptance criteria

- Given a channel message, then the adapter builds the vendor request and returns the vendor message id.
- Given a vendor failure, then the result carries the error and no secret.
- Given live accounts, then tests are `LiveIntegration` only; unit tests use a fake client.

## Existing code checked

`OoBDev.Communications.Abstractions/Sending` (`IChannel`, `IChannelProvider`, `IChannelRegistry`) and `OoBDev.MailKit`. Searched `src` for SendGrid and Twilio: only mentions in tests and the abstractions' comments. The design exists: [Communications](../../design/Communications/README.md).

## Security and operations

API keys and account tokens from configuration or user secrets only. Never log message bodies or phone numbers. Respect rate limits and opt-out rules for SMS.

## Priority

- **Impact:** cloud email and SMS delivery.
- **Needed by:** no date (the epic waits for a consumer)
- **Workaround today:** MailKit for email; no SMS

## Open questions

Are these vendors still wanted? Does a different SMS or email vendor fit better?

## Triage notes

Origin: SharedFramework. Design exists; no code until a consumer appears.

2026-11-11: triaged. Accepted as a backlog item: design first (`docs/patterns-discovery/06-design-document-standard.md`), no code until a consumer appears. Backlog: BACKLOG.md, SharedFramework requests.
