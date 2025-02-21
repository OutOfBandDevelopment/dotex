# OoBDev.Communications.Abstractions


## Class: Communications.Models.AttachmentReferenceModel
Represents a model for referencing attachments. 

### Properties

#### ContainerName
Gets the name of the container where the attachment is stored.
#### DocumentKey
Gets the key or identifier of the document associated with the attachment.

## Class: Communications.Models.EmailMessageModel
Represents an email message model. 

### Properties

#### ReferenceId
Gets or sets the reference ID of the email message.
#### FromAddress
Gets or sets the sender's email address.
#### ToAddresses
Gets or sets the list of recipient email addresses.
#### CcAddresses
Gets or sets the list of carbon copy (CC) email addresses.
#### BccAddresses
Gets or sets the list of blind carbon copy (BCC) email addresses.
#### Subject
Gets or sets the subject of the email message.
#### TextContent
Gets or sets the plain text content of the email message.
#### HtmlContent
Gets or sets the HTML content of the email message.
#### Headers
Gets or sets the headers of the email message.
#### Attachments
Gets or sets the list of attachment references in the email message.

## Class: Communications.Models.ReceivedEmailMessageModel
This model represents inbound email messages 

### Properties

#### Server
this is the host reference for the inbound message
#### Path
what inbox path is the message received on

## Class: Communications.Models.SmsMessageModel
Represents a model for SMS messages. 

### Properties

#### From
Gets or sets the phone number or identifier of the sender.
#### To
Gets or sets the phone number or identifier of the recipient.
#### RequestId
Gets or sets the unique identifier associated with the SMS request.
#### Body
Gets or sets the body or content of the SMS message.
#### Headers
Gets or sets the headers associated with the SMS message.

## Class: Communications.Services.ICommunicationSender`1
Represents an interface for sending communication messages of a specified type. 

### Methods


#### SendAsync(`0)
Asynchronously sends the specified communication message. 


##### Parameters
* *message:* The communication message to send.




##### Return value
A task representing the asynchronous operation. The result is the unique identifier associated with the sent message.

