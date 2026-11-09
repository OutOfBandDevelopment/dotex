# OoBDev - Mailkit

## Summary

Mailkit support add the ability to send email over SMTP

## Notes and References

* https://github.com/ChangemakerStudios/Papercut-SMTP
* https://maildev.github.io/maildev/
  * https://github.com/maildev/maildev
* https://github.com/rnwood/smtp4dev/wiki/Installation
* https://mailcatcher.me/
* https://ndumbster.sourceforge.net/default.html
* https://nilhcem.com/FakeSMTP/
* http://www.aboutmyip.com/AboutMyXApp/DevNullSmtp.jsp
## Channel provider

`MailKitChannelProvider` is the `mailkit` provider of the `email` protocol (`MailKitGlobals`) for `ISendService`.
It sends any `IEmailMessage` (for example `EmailMessage`) through the configured SMTP server and registers with
`TryAddMailKitExtensions` when SMTP is configured. Failures come back as a failed `SendResult`: a busy, unavailable
or full mailbox, a dropped connection and a timeout are `Transient` (retried by `SendService`); other SMTP replies,
authentication errors and a message without a body are `Permanent`; a message that is not an email is `Rejected`.
Attachments are not mapped yet (`MimeMessageFactory` has the same TODO).

## IMAP poller

`MailKitImapChannelProvider` (`mailkit-imap` / `email`, an `IPollableChannelProvider`) reads unread messages from the IMAP inbox
(`MailKitImapClientOptions`) when IMAP is configured. `PollAsync` opens the inbox read-only, so nothing is marked read yet; each message carries a
receipt `uidValidity:uid`. `AcknowledgeAsync` marks the receipts seen, and skips receipts from an earlier UID validity (the mailbox was rebuilt).
Declare a channel with `Provider = mailkit-imap` and call `IReceiveService.PollAsync("<channel>")`. It cannot send. Attachments are not mapped yet.
