## OoBDev - Twilio Communication Services

## Summary

Support for Communication Central to use a Twilio for SMS messages

## Configurations

| Key                                     | Default      | Note                                |
| --------------------------------------- | ------------ | ----------------------------------- |
| Twilio:SmsMessaging:AccountSid          |              | Account SID                         |
| Twilio:SmsMessaging:AuthToken           |              | Twilio Auth token                   |
| Twilio:SmsMessaging:Default:From        |              | Default from number if not provided in request |
| OoBDev:ConfiguredServices:OoBDev.Communications.Contracts.Channels.ISendSmsProvider |  | Set to "OoBDev.Api.Twilio.SmsMessaging.Communications.SendTwilioSmsHandler" to enable this provider

## Notes

https://www.twilio.com/docs/sms/quickstart/csharp-dotnet-framework