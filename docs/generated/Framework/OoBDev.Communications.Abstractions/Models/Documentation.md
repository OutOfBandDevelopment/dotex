# OoBDev Communications Models Documentation

This documentation outlines the classes and models used to represent email and SMS messages, attachments, and other relevant data in the OoBDev Communications system.

## Class Diagram

```plantuml
@startuml
class AttachmentReferenceModel {
  - ContainerName: string
  - DocumentKey: string
}

class EmailMessageModel {
  + ReferenceId: string?
  + FromAddress: string?
  + ToAddresses: List<string>
  + CcAddresses: List<string>
  + BccAddresses: List<string>
  + Subject: string?
  + TextContent: string?
  + HtmlContent: string?
  + Headers: Dictionary<string, string>
  + Attachments: List<AttachmentReferenceModel>
}

class ReceivedEmailMessageModel extends EmailMessageModel {
  + Server: string
  + Path: string?
}

class SmsMessageModel {
  + From: string?
  + To: string?
  + RequestId: Guid
  + Body: string?
  + Headers: Dictionary<string, object>
}

@enduml
```

## Component Model

The OoBDev Communications system consists of several components, including:

* **Email Processing**: responsible for processing inbound and outbound email messages
* **SMS Processing**: responsible for processing inbound and outbound SMS messages
* **Attachment Management**: responsible for managing attachments associated with email and SMS messages
* **Message Storage**: responsible for storing and retrieving email and SMS messages

These components interact with each other to provide a comprehensive communication solution.

## Sequence Diagram

The following sequence diagram illustrates the flow of an email message from sending to receiving:
```plantuml
@startuml
participant Sender as "Email Client"
participant EmailServer as "Email Server"
participant InboundProcessor as "Inbound Processor"
participant MessageStorage as "Message Storage"
participant ReceivedEmailMessageModel as "Received Email Message Model"

Sender ->> EmailServer: Send Email
EmailServer ->> InboundProcessor: Process Email
InboundProcessor ->> MessageStorage: Store Email
MessageStorage ->> ReceivedEmailMessageModel: Retrieve Email
ReceivedEmailMessageModel ->> InboundProcessor: Update Email Status
InboundProcessor ->> Receiver: Deliver Email

@enduml
```

The following sequence diagram illustrates the flow of an SMS message from sending to receiving:
```plantuml
@startuml
participant Sender as "SMS Client"
participant SMSServer as "SMS Server"
participant InboundProcessor as "Inbound Processor"
participant MessageStorage as "Message Storage"
participant ReceivedSmsMessageModel as "Received SMS Message Model"

Sender ->> SMSServer: Send SMS
SMSServer ->> InboundProcessor: Process SMS
InboundProcessor ->> MessageStorage: Store SMS
MessageStorage ->> ReceivedSmsMessageModel: Retrieve SMS
ReceivedSmsMessageModel ->> InboundProcessor: Update SMS Status
InboundProcessor ->> Receiver: Deliver SMS

@enduml
```

The OoBDev Communications system provides a robust and scalable solution for managing email and SMS messages. The system's components work together to ensure that messages are correctly processed, stored, and delivered to the intended recipients.