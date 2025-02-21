# OoBDev.MessageQueueing.Hosting


## Class: MessageQueueing.Hosting.MessageReceiverHost
Hosted service responsible for starting and stopping message receivers based on the configured providers. 

Initializes a new instance of the class.
### Methods


#### Constructor
Initializes a new instance of the class.
Hosted service responsible for starting and stopping message receivers based on the configured providers. 


##### Parameters
* *logger:* The logger.
* *factory:* The message receiver provider factory.




#### Dispose
Disposes of the resources used by the 
 *See: T:OoBDev.MessageQueueing.Hosting.MessageReceiverHost*. 


#### Dispose(System.Boolean)
Disposes of the resources used by the 
 *See: T:OoBDev.MessageQueueing.Hosting.MessageReceiverHost*. 


##### Parameters
* *disposing:* 




#### StartAsync(System.Threading.CancellationToken)
Starts the message receiver host. 


##### Parameters
* *cancellationToken:* The cancellation token.




##### Return value
A task representing the start operation.



#### StopAsync(System.Threading.CancellationToken)
Stops the message receiver host. 


##### Parameters
* *cancellationToken:* The cancellation token.




##### Return value
A task representing the stop operation.



## Class: MessageQueueing.Hosting.ServiceCollectionExtensions
Provides extension methods for configuring IoC (Inversion of Control) services to support all Message Queueing within this library. 

### Methods


#### TryAddMessageQueueingHosting(Microsoft.Extensions.DependencyInjection.IServiceCollection)
Add IOC configurations to support all Message Queueing within this library. 


##### Parameters
* *services:* 




##### Return value


