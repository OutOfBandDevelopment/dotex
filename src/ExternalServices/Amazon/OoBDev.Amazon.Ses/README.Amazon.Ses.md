# OoBDev.Amazon.Ses

AWS SES v2 email sending and SNS delivery notifications for the Communications framework (design: [docs/design/Communications/SendReceive](../../../../docs/design/Communications/SendReceive/requirements.md)).

> **Tested against emulators only.** There is no live AWS connection for testing. The unit tests mock the SES client and the Integration tests run against the Moto container in the test stack. Behavior against real SES (quotas, sandbox mode, sender verification, SNS signature checks) is not verified.

## What it provides

| Type | Role |
|------|------|
| `SesChannelProvider` | `IChannelProvider` `aws-ses` for protocol `email`. Maps `IEmailMessage` (To, Cc, Bcc, Reply-To, text and HTML body, attachments) to a v2 `SendEmail` with simple content. Attachments are sent as raw content, inline when a content id is set. Failures are classified: throttling is `RateLimited`, rejected or unverified is `Rejected`, bad request is `Permanent`, the rest `Transient`. |
| `SnsDeliveryWebhookProcessor` | `IWebhookProcessor` for the SNS HTTP endpoint that receives SES event publishing. Maps Send, Delivery, Bounce, Complaint, Reject, Rendering Failure, Open and Click to `DeliveryStatusEvent`; DeliveryDelay is ignored. The event id is the `message-id` tag the provider sets when a configuration set is used, else the SES message id. |
| `SesClientFactory` / `SesOptions` | Client from options; `ServiceUrl` points it at Moto. |

`ValidateSignature` parses the SNS message, checks `AllowedTopicArns` when set, and verifies the signature with the AWS SDK. A subscription confirmation yields no events: confirm the subscription in AWS (the processor does not call `SubscribeURL`).

## Configuration

```json
{
  "SesOptions": {
    "Region": "us-east-1",
    "AccessKeyId": "",
    "SecretAccessKey": "",
    "ServiceUrl": "http://localhost:4566",
    "DefaultFromAddress": "noreply@example.test",
    "ConfigurationSetName": "events",
    "AllowedTopicArns": ["arn:aws:sns:us-east-1:123456789012:ses-events"]
  }
}
```

Register with `services.TryAddAmazonSesServices(configuration)`; nothing is added when the section is missing. Empty keys use the AWS credential chain.

## Not included

SMS and push through SNS (the design has no `ISmsMessage` or `IPushMessage` contract in code yet) and automatic subscription confirmation.
