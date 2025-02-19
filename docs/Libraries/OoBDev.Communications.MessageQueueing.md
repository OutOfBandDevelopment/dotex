# OoBDev.Communications.MessageQueueing


## Class: Communications.MessageQueueing.EmailMessageHandler
Represents a message handler for handling and sending email messages. 

Initializes a new instance of the class.
### Methods


#### Constructor
Initializes a new instance of the 
 *See: T:OoBDev.Communications.MessageQueueing.EmailMessageHandler*class. 


##### Parameters
* *email:* The communication sender for email messages.
* *logger:* The logger.




#### HandleAsync(OoBDev.Communications.Models.EmailMessageModel,OoBDev.MessageQueueing.Services.IMessageContext)
Handles the specified email message asynchronously. 


##### Parameters
* *message:* The email message to handle.
* *context:* The message context.




##### Return value
A task representing the asynchronous operation.



#### HandleAsync(System.Object,OoBDev.MessageQueueing.Services.IMessageContext)
Handles the specified message asynchronously. 


##### Parameters
* *message:* The message to handle.
* *context:* The message context.




##### Return value
A task representing the asynchronous operation.



## Class: Communications.MessageQueueing.ServiceCollectionExtensions
Extension methods for configuring communication queue services. 

### Methods


#### TryAddCommunicationQueueServices(Microsoft.Extensions.DependencyInjection.IServiceCollection)
Tries to add communication queue services to the specified service collection. 


##### Parameters
* *services:* The service collection to which communication queue services should be added.




##### Return value
The modified service collection.

