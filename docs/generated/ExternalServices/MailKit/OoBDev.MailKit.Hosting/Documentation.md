Here is the documentation for the given source code files:

**OoBDev.MailKit.Hosting.csproj**

This is a .NET Core project file that defines the build settings and dependencies for the OoBDev.MailKit.Hosting assembly. It specifies the target framework as .NET 8.0 and includes references to the Microsoft.Extensions.DependencyInjection.Abstractions, Microsoft.Extensions.Logging.Abstractions, and Microsoft.Extensions.Hosting.Abstractions NuGet packages. It also includes a reference to the OoBDev.MessageQueueing.Abstractions and OoBDev.MailKit projects.

**EmailMessageReceiverHost.cs**

This is a C# class that implements the `IHostedService` interface, which allows it to be used as a hosted service in a .NET Core application. The class has the following responsibilities:

* Initialize a new instance of the `EmailMessageReceiverHost` class, which takes an ILogger, IMessageQueueSender, IImapClientFactory, IOptions<MailKitImapClientOptions>, and IMimeMessageFactory as parameters.
* Start the message receiver host, which creates an IMAP client, opens the inbox folder, searches for unread messages, and sends each message to the configured message queue.
* Stop the message receiver host, which cancels any pending tasks and clears the list of tasks.

The class includes the following methods:

* `Dispose`: Disposes of the resources used by the `EmailMessageReceiverHost` instance.
* `StartAsync`: Starts the message receiver host, which creates an IMAP client, opens the inbox folder, searches for unread messages, and sends each message to the configured message queue.
* `StopAsync`: Stops the message receiver host, which cancels any pending tasks and clears the list of tasks.

**Readme.MailKit.Hosting.md**

This is a Markdown file that provides a summary and notes about the MailKit hosting library. It includes links to external resources and provides additional information about the library.

**ServiceCollectionExtensions.cs**

This is a C# class that provides extension methods for configuring IoC (Inversion of Control) services to support all Message Queueing within the OoBDev.MailKit.Hosting library. The class includes a single method, `TryAddMailKitHosting`, which adds the necessary services to the ServiceCollection instance.

Class diagram in PlantUML:

```plantuml
@startuml
class EmailMessageReceiverHost {
    - logger: ILogger
    - queue: IMessageQueueSender
    - imapClientFactory: IImapClientFactory
    - config: IOptions<MailKitImapClientOptions>
    - messageFactory: IMimeMessageFactory
  }

  class IHostedService {
    + StartAsync(CancellationToken)
    + StopAsync(CancellationToken)
  }

  EmailMessageReceiverHost -->> IHostedService
@enduml
```

This class diagram shows the `EmailMessageReceiverHost` class, which implements the `IHostedService` interface. The `EmailMessageReceiverHost` class has four properties: `logger`, `queue`, `imapClientFactory`, and `messageFactory`.