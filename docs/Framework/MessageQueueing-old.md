# OoBDev - Message Queueing

## Summary

This library allows for the ability to send messages over various queuing technologies in
an abstract way.

## Details

Each possible Queue channel type (technology/platform) is listed in the QueueTypes enumerate

### Current Queue Types

* Special
* Configured
* Default
* Azure Storage Queue
* Azure Service Bus Queues
* Azure Storage Bus Topics
* Amazon Simple Queue

## Examples


## Configuration Options

### Configured Queue Type

If a QueueType property on the MessageQueue attribute is set to Configured the application will
check for a configuration value.  If no matching configuration is found the system will return Default.

```csharp
[MessageQueue(QueueType = QueueTypes.Configured)]
public class MyService
{
    private readonly IMessageSender<MyService> _queue;

    public MyService(
        IMessageSender<MyService> queue
    ){
        _queue = queue;
    }
}
```

```json
{
    "OoBDev:MessageQueueing:MyService:QueueType": "Azure Storage Bus Topics"
}
```

### Configure Queue Name

Queue names may be set withing the attribute on your class, redirected to a configuration value, defaulted to the full class name.

#### Directly assigned

If you directly set the QueueName on your sender target's MessageQueue attribute then this value will be provided to tne engine at runtime.

```csharp
[MessageQueue(QueueName = "my-target-queue")]
public class MyService
{
    private readonly IMessageSender<MyService> _queue;

    public MyService(
        IMessageSender<MyService> queue
    ){
        _queue = queue;
    }
}
```

#### Targeted Configuration

You may provide a configuration key which will be used in combination with the queue type to resolve a configuration value for the queue name

```csharp
[MessageQueue(QueueName = "%Queue%", QueueType = QueueTypes.AzureStorageQueue)]
public class MyService
{
    private readonly IMessageSender<MyService> _queue;
    ...
}
[MessageQueue(SimpleName = "YourQueue", QueueType = QueueTypes.AzureServiceBusQueue)]
public class OtherService
{
    private readonly IMessageSender<OtherService> _queue;
    ...
}
```

```json
// configuration keys are composed as {Queue Type Prefix}:{Queue Class or Simple Name or "Default"}:{Set Suffix or "QueueName"}
//Note, Default will resolve for any queue of the same type.  it is recommended to avoid 

// Possible Keys for the above MyService

// QueueType = Default
"OoBDev:MessageQueueing:MyService:Queue":"your-queue"
"OoBDev:MessageQueueing:Default:Queue":"your-queue"

// QueueType = AmazonSimpleQueue
"Amazon:SimpleQueue:MyService:Queue":"your-queue"
"Amazon:SimpleQueue:Default:Queue":"your-queue"

// QueueType = AzureServiceBusQueue or AzureServiceBusTopic
"Azure:ServiceBus:MyService:Queue":"your-queue"
"Azure:ServiceBus:Default:Queue":"your-queue"

// QueueType = AzureStorageQueue
"Azure:Storage:MyService:Queue":"your-queue"
"Azure:Storage:Default:Queue":"your-queue"

// Possible Keys for the above OtherService

// QueueType = Default
"OoBDev:MessageQueueing:YourQueue:QueueName":"your-queue"
"OoBDev:MessageQueueing:Default:QueueName":"your-queue"

// QueueType = AmazonSimpleQueue
"Amazon:SimpleQueue:YourQueue:QueueName":"your-queue"
"Amazon:SimpleQueue:Default:QueueName":"your-queue"

// QueueType = AzureServiceBusQueue or AzureServiceBusTopic
"Azure:ServiceBus:YourQueue:QueueName":"your-queue"
"Azure:ServiceBus:Default:QueueName":"your-queue"

// QueueType = AzureStorageQueue
"Azure:Storage:YourQueue:QueueName":"your-queue"
"Azure:Storage:Default:QueueName":"your-queue"

```

## IMessageSender<TQueueTarget>

Message queue writer.  The type referenced by TQueueTarget will be used to resolve 
the queue type and name

### Task<string?> SendAsync<T>(T message, string? messageId = null) where T : class

This will enqueue a message into the referenced `TQueueTarget`.  The message 
will be enqueued as configured (default is JSON) and a content type extension
will reference the .Net type passed in as the type parameter `T`.

## MessageQueueAttribute

This attribute on a .Net type when used in combination with the `IMessageSender<>`
allows for application level configuration and targeting of a message queue

### string QueueName

Full or templated name for the queue. Portion of this value wrapped with 
percent `%` signs will be resolved from the application configuration.

### QueueTypes QueueType

Queue provider type to be used when resolving a collection to the reference queue

### string? SimpleName 

Simple name allows for named targeting for the configuration value

## QueueTypes

Selectable queue type

### Special 

This will throw an exception if you try to resolve an message reader for this type.
The current intention is to be used for read-only queues.

Prefix for related configuration values `Example:MessageQueueing:Special`

### Configured

If this is selected the configuration key matching "Example:MessageQueueing:{MessageQueueAttribute.SimpleName}:QueueType" 
must be set to one of the valid values below.

### Default

This will use the application configured default queue.  

Prefix for related configuration values `Example:MessageQueueing`

### AzureStorageQueue

This message queue is configured to use Azure Storage Queues

### AzureServiceBusQueue

This message queue is configured to use Azure Service Bus Queues

### AzureServiceBusTopic

This message queue is configured to use Azure Service Bus Topics

### AmazonSimpleQueue

This message queue is configured to use Amazon Simple Queue Service (SQS)

