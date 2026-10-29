# OoBDev.Communications.MessageQueueing

OoBDev.Communications.MessageQueueing is a module designed to facilitate message queueing for communication-related tasks. Let's delve into its components:

## EmailMessageHandler

This class represents a message handler tailored for handling and sending email messages asynchronously.

### Methods

- **Constructor**: Initializes a new instance of the EmailMessageHandler class.
- **HandleAsync(EmailMessageModel, IMessageContext)**: Handles the specified email message asynchronously.
- **HandleAsync(Object, IMessageContext)**: Handles the specified message asynchronously.

## ServiceCollectionExtensions

This class provides extension methods for configuring communication queue services within the application.

### Methods

- **TryAddCommunicationQueueServices**: Attempts to add communication queue services to the specified service collection.

OoBDev.Communications.MessageQueueing offers streamlined functionality for integrating email message handling into message queueing systems, enhancing communication workflows within applications.
