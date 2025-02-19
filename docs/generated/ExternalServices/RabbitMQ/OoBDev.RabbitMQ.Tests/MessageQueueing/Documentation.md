Here is the documentation for the `RabbitMQQueueMessageSenderProviderTests` class in PlantUML:

```plantuml
@startuml
class RabbitMQQueueMessageSenderProviderTests {
  - corridorId: string
  + TestContext: TestContext
  + SendAsyncTest_ByFullType
  + SendAsyncTest_ByKeyed
  + FindProviderTests
  + TestContext_Write
}

class RabbitMQQueueMessageProvider
  - AssemblyQualifiedName: string

class ConfigurationBuilder {
  + AddInMemoryCollection
  + Build
}

class Microsoft.Extensions.Configuration
  - GetSection

class RabbitMQGlobals {
  + MessageProviderKey: string
}

class MessageSenderTests {
  + GetServiceProvider
  + TryAddRabbitMQServices
}

class ServiceProvider {
  + TryAddRabbitMQServices
  + GetRequiredService
}

class IMessageQueueSender <RabbitMQQueueMessageSenderProviderTests> {
  + SendAsync
}

class IMessageQueueHandler
  + HandleMessage
}

class TestMessageHandler
  + HandleMessage

class TestMessageHandlerWithProvider
  + HandleMessage

class TestMessageHandlerWithProviderAndMessage
  + HandleMessage

class IMessageReceiverProviderFactory
  + Create
}

@enduml
```

And here is the documentation for the `RabbitMQQueueMessageSenderProviderTests` class in natural language:

**Class Documentation:**

The `RabbitMQQueueMessageSenderProviderTests` class is a test class for the `RabbitMQQueueMessageSenderProvider` class. It contains three test methods: `SendAsyncTest_ByFullType`, `SendAsyncTest_ByKeyed`, and `FindProviderTests`. These tests verify that the `RabbitMQQueueMessageSenderProvider` class can send messages asynchronously and find providers correctly.

**Properties:**

* `TestContext`: a required property that is used to test the class.
* `correlationId`: a property that is used to store the correlation id returned by the `SendAsync` method.

**Methods:**

* `SendAsyncTest_ByFullType`: tests the `SendAsync` method by sending a full-type message.
* `SendAsyncTest_ByKeyed`: tests the `SendAsync` method by sending a keyed message.
* `FindProviderTests`: tests the provider finder by creating a service provider and finding providers.
* `TestContext_Write`: writes a message to the test context.

**Other Notes:**

This class uses the `MessageSenderTests` class to create a service provider and add RabbitMQ services. It also uses the `IMessageQueueSender` and `IMessageQueueHandler` interfaces to send and handle messages. The `IMessageReceiverProviderFactory` is used to create a provider factory.