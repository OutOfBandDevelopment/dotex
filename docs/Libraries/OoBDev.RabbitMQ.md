# OoBDev.RabbitMQ


## Class: RabbitMQ.MessageQueueing.IQueueClientFactory
Factory for creating instances of 
 *See: T:RabbitMQ.Client.IConnection*and 
 *See: T:RabbitMQ.Client.IModel*for Rabbit MQ Queues. 

### Methods


#### Create(Microsoft.Extensions.Configuration.IConfigurationSection)
Creates a new instance of 
 *See: T:RabbitMQ.Client.IConnection*and 
 *See: T:RabbitMQ.Client.IModel*based on the provided configuration section. 


##### Parameters
* *config:* The configuration section containing connection string and queue name.




##### Return value
A new instance of and for the specified Rabbit MQ Queue.



##### Exceptions

* *System.ApplicationException:* Thrown if the required configuration values ("ConnectionString" or "QueueName") are missing.




## Class: RabbitMQ.MessageQueueing.QueueClientFactory
Factory for creating instances of 
 *See: T:RabbitMQ.Client.IConnection*and 
 *See: T:RabbitMQ.Client.IModel*&gt; for Rabbit MQ Queues. 

### Methods


#### Create(Microsoft.Extensions.Configuration.IConfigurationSection)
Creates a new instance of 
 *See: T:RabbitMQ.Client.IConnection*based on the provided configuration section. 


##### Parameters
* *config:* The configuration section containing connection string and queue name.




##### Return value
A new instance of and for the specified Rabbit MQ Queue.



##### Exceptions

* *System.ApplicationException:* Thrown if the required configuration values ("ConnectionString" or "QueueName") are missing.




## Class: RabbitMQ.MessageQueueing.RabbitMQQueueMessageProvider
Provides functionality for sending and receiving messages using Rabbit MQ Queues. 
Initializes a new instance of the 
 *See: T:OoBDev.RabbitMQ.MessageQueueing.RabbitMQQueueMessageProvider*class. 

### Methods


#### Constructor
Provides functionality for sending and receiving messages using Rabbit MQ Queues. 


##### Parameters
* *serializer:* The JSON serializer for message serialization and deserialization.
* *clientFactory:* The factory for creating Rabbit MQ Queue clients.
* *logger:* The logger for logging messages.




#### SendAsync(System.Object,OoBDev.MessageQueueing.Services.IMessageContext)
Sends a message asynchronously to an Rabbit MQ Queue. 


##### Parameters
* *message:* The message to be sent.
* *context:* The message context containing additional information.




##### Return value
The message ID if the send operation is successful; otherwise, null.



#### SetHandlerProvider(OoBDev.MessageQueueing.Services.IMessageHandlerProvider)
Sets the message handler provider for processing received messages. 


##### Parameters
* *handlerProvider:* The message handler provider.




##### Return value
The current instance of .



#### RunAsync(System.Threading.CancellationToken)
Runs the message receiver asynchronously, continuously listening for incoming messages. 


##### Parameters
* *cancellationToken:* The cancellation token to stop the receiver.




##### Return value
A task representing the asynchronous operation.



## Class: RabbitMQ.RabbitMQGlobals
Contains global constants related to Rabbit MQ. 

### Fields

#### MessageProviderKey
The key associated with the Rabbit MQ message provider.

## Class: RabbitMQ.ServiceCollectionEx
Provides extension methods for configuring Rabbit MQ services in the 
 *See: T:Microsoft.Extensions.DependencyInjection.IServiceCollection*. 

### Methods


#### TryAddRabbitMQServices(Microsoft.Extensions.DependencyInjection.IServiceCollection)
Tries to add Rabbit MQ services including blob and queue services to the specified 
 *See: T:Microsoft.Extensions.DependencyInjection.IServiceCollection*. 


##### Parameters
* *services:* The to add services to.




##### Return value
The modified .



#### TryAddRabbitMQQueueServices(Microsoft.Extensions.DependencyInjection.IServiceCollection)
Tries to add Rabbit MQ queue services to the specified 
 *See: T:Microsoft.Extensions.DependencyInjection.IServiceCollection*. 


##### Parameters
* *services:* The to add services to.




##### Return value
The modified .

