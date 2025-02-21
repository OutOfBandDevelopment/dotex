# OoBDev.MessageQueueing.Abstractions


## Class: MessageQueueing.IMessageQueueHandler
Represents a generic interface for handling messages from a message queue. 

### Methods


#### HandleAsync(System.Object,OoBDev.MessageQueueing.Services.IMessageContext)
Handles the specified message asynchronously. 


##### Parameters
* *message:* The message to handle.
* *context:* The context associated with the message.




##### Return value
A task that represents the asynchronous handling of the message.



#### 
Handles the specified message asynchronously. 


##### Parameters
* *message:* The message to handle.
* *context:* The context associated with the message.




##### Return value
A task that represents the asynchronous handling of the message.



## Class: MessageQueueing.IMessageQueueHandler`1
Represents a generic interface for handling messages of type 
from a message queue. 
The type of channel for the messages. 


## Class: MessageQueueing.IMessageQueueHandler`2
Represents a generic interface for handling messages of type 
from a message queue associated with a specific channel of type 
. 
The type of channel for the messages. 
The type of messages to handle. 

### Methods


#### HandleAsync(`1,OoBDev.MessageQueueing.Services.IMessageContext)
Handles the specified message asynchronously. 


##### Parameters
* *message:* The message to handle.
* *context:* The context associated with the message.




##### Return value
A task that represents the asynchronous handling of the message.



## Class: MessageQueueing.IMessageQueueSender
Represents a generic interface for sending messages to a message queue. 

### Methods


#### SendAsync(System.Object,System.String)
Sends the specified message asynchronously. 


##### Parameters
* *message:* The message to send.
* *messageId:* The optional message identifier.




##### Return value
A task that represents the asynchronous sending of the message.



## Class: MessageQueueing.IMessageQueueSender`1
Represents a generic interface for sending messages of type 
to a message queue. 
The type of channel for the messages. 


## Class: MessageQueueing.MessageQueueAttribute
Attribute used to mark a class as a message queue handler and provide configuration options. 

Initializes a new instance of the class with the specified simple name.
### Properties

#### SimpleName
Gets the simple name used for named targeting in the configuration value.
#### TypeId
Gets a unique identifier for this attribute.
### Methods


#### Constructor
Initializes a new instance of the class with the specified simple name.
Attribute used to mark a class as a message queue handler and provide configuration options. 


##### Parameters
* *simpleName:* The simple name used for named targeting in the configuration value.




## Class: MessageQueueing.Services.IMessageContext
Represents the context of a message being processed in a message queue. 

### Properties

#### OriginMessageId
Gets the origin message identifier.
#### CorrelationId
Gets or sets the correlation identifier.
#### RequestId
Gets the request identifier.
#### SentId
Gets or sets the sent identifier.
#### ChannelType
Gets the type of the channel.
#### MessageType
Gets the type of the message.
#### SentAt
Gets the timestamp when the message was sent.
#### SentBy
Gets the entity that sent the message.
#### SentFrom
Gets the entity from which the message was sent.
#### Item(System.String)
Gets or sets the value associated with the specified key. The key of the value to get or set.The value associated with the specified key.
#### Headers
Gets the collection of headers associated with the message.
#### Config
Gets the configuration section associated with the message context.
### Methods


#### 
Creates a new instance of 
 *See: T:OoBDev.MessageQueueing.Services.IMessageContext*with the specified parameters. 


##### Parameters
* *channelType:* The type of the message channel.
* *messageType:* The type of the message.
* *originMessageId:* The origin message identifier.
* *correlationId:* The correlation identifier.
* *requestId:* The request identifier.
* *configuration:* The configuration section associated with the message context.
* *caller:* The calling method.
* *callerLine:* The line number at which the method is called.
* *callerFile:* The path to the source file that contains the calling method.




##### Return value
A new instance of .



#### 
Creates a new instance of 
 *See: T:OoBDev.MessageQueueing.Services.IMessageContext*for the given channel and message. 


##### Parameters
* *channelType:* The type of the message channel.
* *message:* The queue message.
* *configuration:* The configuration section associated with the message context.




##### Return value
A new instance of .



## Class: MessageQueueing.Services.IMessageContextFactory
Factory for creating instances of 
 *See: T:OoBDev.MessageQueueing.Services.IMessageContext*. 

### Methods


#### Create(System.Type,System.Type,System.String,System.String,System.String,Microsoft.Extensions.Configuration.IConfigurationSection,System.Reflection.MethodBase,System.Int32,System.String)
Creates a new instance of 
 *See: T:OoBDev.MessageQueueing.Services.IMessageContext*with the specified parameters. 


##### Parameters
* *channelType:* The type of the message channel.
* *messageType:* The type of the message.
* *originMessageId:* The origin message identifier.
* *correlationId:* The correlation identifier.
* *requestId:* The request identifier.
* *configuration:* The configuration section associated with the message context.
* *caller:* The calling method.
* *callerLine:* The line number at which the method is called.
* *callerFile:* The path to the source file that contains the calling method.




##### Return value
A new instance of .



#### Create(System.Type,OoBDev.MessageQueueing.Services.IQueueMessage,Microsoft.Extensions.Configuration.IConfigurationSection)
Creates a new instance of 
 *See: T:OoBDev.MessageQueueing.Services.IMessageContext*for the given channel and message. 


##### Parameters
* *channelType:* The type of the message channel.
* *message:* The queue message.
* *configuration:* The configuration section associated with the message context.




##### Return value
A new instance of .



## Class: MessageQueueing.Services.IMessageHandlerProvider
Provides a mechanism for handling queue messages. 

### Properties

#### Config
Gets the configuration section associated with the message handler provider.
### Methods


#### HandleAsync(OoBDev.MessageQueueing.Services.IQueueMessage,System.String)
Handles the specified queue message with the given message identifier. 


##### Parameters
* *message:* The queue message to handle.
* *messageId:* The identifier associated with the message.




##### Return value
A task representing the asynchronous operation.



#### 
Sets the collection of message queue handlers for the provider. 


##### Parameters
* *handlers:* The collection of message queue handlers.




##### Return value
The updated message handler provider.



#### 
Sets the type of the message channel for the provider. 


##### Parameters
* *channelType:* The type of the message channel.




##### Return value
The updated message handler provider.



#### 
Sets the configuration section for the provider. 


##### Parameters
* *config:* The configuration section.




##### Return value
The updated message handler provider.



## Class: MessageQueueing.Services.IMessageHandlerProviderWrapped
Provides a internal extensions for mechanism for handling queue messages. 

### Methods


#### SetHandlers(System.Collections.Generic.IEnumerable{OoBDev.MessageQueueing.IMessageQueueHandler})
Sets the collection of message queue handlers for the provider. 


##### Parameters
* *handlers:* The collection of message queue handlers.




##### Return value
The updated message handler provider.



#### SetChannelType(System.Type)
Sets the type of the message channel for the provider. 


##### Parameters
* *channelType:* The type of the message channel.




##### Return value
The updated message handler provider.



#### SetConfig(Microsoft.Extensions.Configuration.IConfigurationSection)
Sets the configuration section for the provider. 


##### Parameters
* *config:* The configuration section.




##### Return value
The updated message handler provider.



## Class: MessageQueueing.Services.IMessagePropertyResolver
Resolves properties related to message handling, such as provider, message ID, and configuration. 

### Methods


#### ProviderSafe(System.Type,System.Type)
Retrieves provider information for the specified message channel and message types. 


##### Parameters
* *channelType:* The type of the message channel.
* *messageType:* The type of the message.




##### Return value
A tuple containing provider key, simple target name, simple message name, and configuration path.



#### Provider(System.Type,System.Type)
Retrieves the provider key for the specified message channel and message types. 


##### Parameters
* *channelType:* The type of the message channel.
* *messageType:* The type of the message.




##### Return value
The provider key.



#### MessageId(System.Type,System.Type,System.String)
Retrieves the message ID for the specified message channel, message type, and optional original message ID. 


##### Parameters
* *channelType:* The type of the message channel.
* *messageType:* The type of the message.
* *messageId:* The original message ID (optional).




##### Return value
The resolved message ID.



#### GenerateId(System.Type,System.Type)
Generates a unique ID for the specified message channel and message types. 


##### Parameters
* *channelType:* The type of the message channel.
* *messageType:* The type of the message.




##### Return value
The generated unique ID.



#### ConfigurationSafe(System.Type,System.Type)
Retrieves configuration information for the specified message channel and message types. 


##### Parameters
* *channelType:* The type of the message channel.
* *messageType:* The type of the message.




##### Return value
A tuple containing the configuration section, simple target name, simple message name, and configuration path.



#### Configuration(System.Type,System.Type)
Retrieves the configuration section for the specified message channel and message types. 


##### Parameters
* *channelType:* The type of the message channel.
* *messageType:* The type of the message.




##### Return value
The configuration section.



## Class: MessageQueueing.Services.IMessageReceiverProvider
Provides functionality for receiving messages from a message queue. 

### Methods


#### SetHandlerProvider(OoBDev.MessageQueueing.Services.IMessageHandlerProvider)
Sets the handler provider for processing received messages. 


##### Parameters
* *handlerProvider:* The message handler provider.




##### Return value
The current instance of the message receiver provider.



#### RunAsync(System.Threading.CancellationToken)
Runs the message receiver asynchronously. 


##### Parameters
* *cancellationToken:* The cancellation token to observe for cancellation requests.




##### Return value
A task representing the asynchronous operation.



#### 
Creates a collection of message receiver providers. 


##### Return value
An enumerable collection of message receiver providers.



## Class: MessageQueueing.Services.IMessageReceiverProviderFactory
Factory for creating instances of 
 *See: T:OoBDev.MessageQueueing.Services.IMessageReceiverProvider*. 

### Methods


#### Create
Creates a collection of message receiver providers. 


##### Return value
An enumerable collection of message receiver providers.



## Class: MessageQueueing.Services.IMessageSenderProvider
Represents a provider for sending messages to a message queue. 

### Methods


#### SendAsync(System.Object,OoBDev.MessageQueueing.Services.IMessageContext)
Sends a message asynchronously to the message queue. 


##### Parameters
* *message:* The message to send.
* *context:* The context associated with the message.




##### Return value
A task representing the asynchronous operation. The task result is the unique identifier assigned to the sent message.



#### 
Creates an instance of 
 *See: T:OoBDev.MessageQueueing.Services.IMessageSenderProvider*for the specified channel and message types. 


##### Parameters
* *channelType:* The type of the message channel.
* *messageType:* The type of the message.




##### Return value
An instance of .



## Class: MessageQueueing.Services.IMessageSenderProviderFactory
Represents a factory for creating instances of 
 *See: T:OoBDev.MessageQueueing.Services.IMessageSenderProvider*. 

### Methods


#### Sender(System.Type,System.Type)
Creates an instance of 
 *See: T:OoBDev.MessageQueueing.Services.IMessageSenderProvider*for the specified channel and message types. 


##### Parameters
* *channelType:* The type of the message channel.
* *messageType:* The type of the message.




##### Return value
An instance of .



## Class: MessageQueueing.Services.IQueueMessage
Represents a message within a message queue. 

### Properties

#### ContentType
Gets the content type of the message.
#### CorrelationId
Gets the correlation identifier of the message.
#### Payload
Gets the payload object of the message.
#### PayloadType
Gets the type of the payload object.
#### Properties
Gets the properties associated with the message.

## Class: MessageQueueing.Services.WrappedQueueMessage
Represents a wrapped queue message. 

### Properties

#### ContentType
Gets or initializes the content type of the message.
#### CorrelationId
Gets or initializes the correlation ID of the message.
#### Payload
Gets or initializes the payload of the message.
#### PayloadType
Gets or initializes the payload type of the message.
#### Properties
Gets or initializes the properties associated with the message.