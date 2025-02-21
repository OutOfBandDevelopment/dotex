**AzureStorageQueueMapper Class Diagram**

```plantuml
@startuml
!include https://raw.githubusercontent.com/plantuml-stdlib/C4-PlantUML/master/C4_Container.puml

!include https://raw.githubusercontent.com/plantuml-stdlib/C4-PlantUML/master/C4_Context.puml

!include https://raw.githubusercontent.com/plantuml-stdlib/C4-PlantUML/master/C4_Component.puml

!include https://raw.githubusercontent.com/plantuml-stdlib/C4-PlantUML/master/C4_UML.puml

Context "AzureStorageQueueMapper" {
  boundary "AzureStorageQueueMapper" as AzureStorageQueueMapper
  container "OoBDev.Azure.StorageAccount.MessageQueueing" as Container

  (AzureStorageQueueMapper) *-1> (Container)

  AzureStorageQueueMapper ..> WrappedQueueMessage as "Wrap"
  AzureStorageQueueMapper ..> IConfiguration as "Get Ensure Queue Exists"
  AzureStorageQueueMapper ..> int as "Get Wait Delay"

  class WrappedQueueMessage {
  }

  class IConfiguration {
  }

  class int {
  }
}

@enduml
```

**AzureStorageQueueMessageProvider Class Diagram**

```plantuml
@startuml
!include https://raw.githubusercontent.com/plantuml-stdlib/C4-PlantUML/master/C4_Container.puml

!include https://raw.githubusercontent.com/plantuml-stdlib/C4-PlantUML/master/C4_Context.puml

!include https://raw.githubusercontent.com/plantuml-stdlib/C4-PlantUML/master/C4_Component.puml

!include https://raw.githubusercontent.com/plantuml-stdlib/C4-PlantUML/master/C4_UML.puml

Context "AzureStorageQueueMessageProvider" {
  boundary "AzureStorageQueueMessageProvider" as AzureStorageQueueMessageProvider
  container "OoBDev.Azure.StorageAccount.MessageQueueing" as Container

  (AzureStorageQueueMessageProvider) *-1> (Container)

  AzureStorageQueueMessageProvider ..> IMessageSenderProvider as "Send"
  AzureStorageQueueMessageProvider ..> IMessageReceiverProvider as "SetHandlerProvider"
  AzureStorageQueueMessageProvider ..> ILogger as "Logger"
  AzureStorageQueueMessageProvider ..> IJsonSerializer as "Serializer"
  AzureStorageQueueMessageProvider ..> IQueueClientFactory as "ClientFactory"
  AzureStorageQueueMessageProvider ..> IAzureStorageQueueMapper as "Mapper"

  class IMessageSenderProvider {
  }

  class IMessageReceiverProvider {
  }

  class ILogger {
  }

  class IJsonSerializer {
  }

  class IQueueClientFactory {
  }

  class IAzureStorageQueueMapper {
  }
}

@enduml
```

**IAzureStorageQueueMapper Interface Diagram**

```plantuml
@startuml
!include https://raw.githubusercontent.com/plantuml-stdlib/C4-PlantUML/master/C4_Interface.puml

Class IAzureStorageQueueMapper {
  +bool EnsureQueueExists(IConfiguration configuration)
  +WrappedQueueMessage Wrap(object message, IMessageContext context)
  +int WaitDelay(IConfiguration configuration)
}

@enduml
```

**IQueueClientFactory Interface Diagram**

```plantuml
@startuml
!include https://raw.githubusercontent.com/plantuml-stdlib/C4-PlantUML/master/C4_Interface.puml

Class IQueueClientFactory {
  +QueueClient Create(IConfigurationSection config)
}

@enduml
```

**QueueClientFactory Class Diagram**

```plantuml
@startuml
!include https://raw.githubusercontent.com/plantuml-stdlib/C4-PlantUML/master/C4_Class.puml

Class QueueClientFactory implements IQueueClientFactory {
  +QueueClient Create(IConfigurationSection config)
}

@enduml
```