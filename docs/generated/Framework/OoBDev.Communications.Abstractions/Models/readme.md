**README File**

This repository contains a set of C# classes that define models for email and SMS messages. The classes are designed to represent the structure and content of these messages, and can be used for routing, processing, and storing email and SMS communications.

**Summary**

The repository includes four main classes:

* `AttachmentReferenceModel`: represents a reference to an attachment, with properties for the container name and document key.
* `EmailMessageModel`: represents an email message, with properties for the reference ID, sender's address, recipient addresses, subject, content, and headers.
* `ReceivedEmailMessageModel`: inherits from `EmailMessageModel` and adds properties for the host server and inbox path.
* `SmsMessageModel`: represents an SMS message, with properties for the sender's phone number, recipient's phone number, request ID, and body.

**Technical Summary**

The architecture is based on a simple object-oriented design, with each class representing a specific type of message. The classes are designed to be loosely coupled, with each class having its own set of properties and methods. The use of abstract classes (`EmailMessageModel`) and inheritance (e.g., `ReceivedEmailMessageModel`) allows for a flexible and scalable design.

The classes also follow a Domain-Driven Design (DDD) approach, with each class representing a specific concept or entity within the domain of email and SMS communications.

**Component Diagram**

```plantuml
@startuml
class AttachmentReferenceModel {
  -containerName: string
  -documentKey: string
}

class EmailMessageModel {
  -referenceId: string?
  -fromAddress: string?
  -toAddresses: List<string>
  -ccAddresses: List<string>
  -bccAddresses: List<string>
  -subject: string?
  -textContent: string?
  -htmlContent: string?
  -headers: Dictionary<string, string>
  -attachments: List<AttachmentReferenceModel>
}

class ReceivedEmailMessageModel {
  -emailMessageModel
  -server: string
  -path: string?
}

class SmsMessageModel {
  -from: string?
  -to: string?
  -requestCode: Guid
  -body: string?
  -headers: Dictionary<string, object>
}

EmailMessageModel -- AttachmentReferenceModel
ReceivedEmailMessageModel -- EmailMessageModel
SmsMessageModel --*

@enduml
```

This component diagram shows the relationships between the classes, including inheritance relationships (e.g., `ReceivedEmailMessageModel` inherits from `EmailMessageModel`) and composition relationships (e.g., `EmailMessageModel` has a list of `AttachmentReferenceModel` objects).