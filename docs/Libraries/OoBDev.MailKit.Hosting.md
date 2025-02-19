# OoBDev.MailKit.Hosting


## Class: MailKit.Hosting.EmailMessageReceiverHost
Hosted service responsible for starting and stopping message receivers based on the configured providers. 

Initializes a new instance of the class.
### Methods


#### Constructor
Initializes a new instance of the class.
Hosted service responsible for starting and stopping message receivers based on the configured providers. 


##### Parameters
* *logger:* The logger.
* *queue:* The queue.
* *imapClientFactory:* The client factory.
* *config:* imap config.
* *messageFactory:* The message factory.




#### Dispose
Disposes of the resources used by the 
 *See: T:OoBDev.MailKit.Hosting.EmailMessageReceiverHost*. 


#### Dispose(System.Boolean)
Disposes of the resources used by the 
 *See: T:OoBDev.MailKit.Hosting.EmailMessageReceiverHost*. 


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



## Class: MailKit.Hosting.ServiceCollectionExtensions
Provides extension methods for configuring IoC (Inversion of Control) services to support all Message Queueing within this library. 

### Methods


#### TryAddMailKitHosting(Microsoft.Extensions.DependencyInjection.IServiceCollection)
Add IOC configurations to support all Mailkit Hosting within this library. 


##### Parameters
* *services:* 




##### Return value


