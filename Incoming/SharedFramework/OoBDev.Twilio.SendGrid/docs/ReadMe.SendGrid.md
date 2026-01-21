## OoBDev - Send Grid Communication Services

## Summary

Support for Communication Central to use a Send Grid for Email

## Configurations

| Key                                     | Default      | Note                                |
| --------------------------------------- | ------------ | ----------------------------------- |
| Twilio:SendGrid:ApiKey                  |              | API Key for Send Grid               |
| Twilio:SendGrid:Default:From:Email      |              | Default From address if not provided in request |
| Twilio:SendGrid:Default:Subject         |              | Default Subject if not provided in request |
| OoBDev:ConfiguredServices:OoBDev.Communications.Abstractions.Channels.ISendEmailProvider |  | Set to "OoBDev.Twilio.SendGrid.Communications.SendGridEmailHandler" to enable this provider

## Notes

https://docs.microsoft.com/en-us/azure/sendgrid-dotnet-how-to-send-email#to-find-your-sendgrid-api-key
https://docs.microsoft.com/en-us/azure/sendgrid-dotnet-how-to-send-email
