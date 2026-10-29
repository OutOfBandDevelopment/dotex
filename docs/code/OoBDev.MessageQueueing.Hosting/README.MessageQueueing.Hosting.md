# OoBDev.MessageQueueing.Hosting

## MessageReceiverHost

Hosted service responsible for starting and stopping message receivers based on the configured providers.

### Methods

#### Constructor

Initializes a new instance of the class.

##### Parameters

- *logger*: The logger.
- *factory*: The message receiver provider factory.

#### Dispose

Disposes of the resources used by the MessageReceiverHost.

#### Dispose(System.Boolean)

Disposes of the resources used by the MessageReceiverHost.

##### Parameters

- *disposing*

#### StartAsync(System.Threading.CancellationToken)

Starts the message receiver host.

##### Parameters

- *cancellationToken*: The cancellation token.

##### Return value

A task representing the start operation.

#### StopAsync(System.Threading.CancellationToken)

Stops the message receiver host.

##### Parameters

- *cancellationToken*: The cancellation token.

##### Return value

A task representing the stop operation.

## ServiceCollectionExtensions

Provides extension methods for configuring IoC (Inversion of Control) services to support all Message Queueing within this library.

### Methods

#### TryAddMessageQueueingHosting(Microsoft.Extensions.DependencyInjection.IServiceCollection)

Add IOC configurations to support all Message Queueing within this library.

##### Parameters

- *services*
