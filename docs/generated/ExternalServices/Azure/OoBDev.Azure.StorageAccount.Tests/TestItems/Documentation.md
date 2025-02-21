Here is the documentation for the provided source code files, including class diagrams in PlantUML:

**Class Diagram**

```
@startuml
class TestMessageHandler {
  - logger: ILogger<TestMessageHandler>
  - testContext: TestContext/TestContext

  + HandleAsync(object, IMessageContext) : Task
}

class TestMessageHandlerWithProvider {
  - logger: ILogger<TestMessageHandlerWithProvider>
  - testContext: TestContext/TestContext

  + HandleAsync(object, IMessageContext) : Task
}

class TestMessageHandlerWithProviderAndMessage {
  - logger: ILogger<TestMessageHandlerWithProviderAndMessage>
  - testContext: TestContext/TestContext

  + HandleAsync(TestQueueMessage, IMessageContext) : Task
  + HandleAsync(object, IMessageContext) : Task
}

class TestQueueMessage {
  #message: string
}

class IMessageQueueHandler {
  + HandleAsync(object, IMessageContext) : Task
}

class IMessageContext {
  - config: Configuration/Configuration
  #path: string
}

@Configuration {
  #path: string
}

@enduml
```

**Documentation for Each File:**

**TestMessageHandler.cs**

This class implements the `IMessageQueueHandler` interface and handles messages asynchronously. It takes an `ILogger` and `TestContext` object in its constructor.

* `HandleAsync(object message, IMessageContext context)`: This method logs information about the message and adds the result to the test context.

**TestMessageHandlerWithProvider.cs**

This class is similar to `TestMessageHandler.cs`, but it takes a specific provider type (`AzureStorageQueueMessageSenderProviderTests`) in its type parameter.

* `HandleAsync(object message, IMessageContext context)`: This method logs information about the message and adds the result to the test context.

**TestMessageHandlerWithProviderAndMessage.cs**

This class is similar to `TestMessageHandlerWithProvider.cs`, but it takes a specific message type (`TestQueueMessage`) as a type parameter.

* `HandleAsync(TestQueueMessage message, IMessageContext context)`: This method logs information about the message and adds the result to the test context.
* `HandleAsync(object message, IMessageContext context)`: This method is an override that handles the specific `TestQueueMessage` type.

**TestQueueMessage.cs**

This is a simple record class that represents a test queue message.

Note that the `IMessageQueueHandler` interface and `IMessageContext` class are not provided in the source code, but they can be assumed to exist based on the code that uses them.