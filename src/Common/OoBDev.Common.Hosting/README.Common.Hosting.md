# OoBDev.Common.Hosting

This library provides a set of classes and extension methods for configuring common hosting services 
in .NET applications.

## Common.Hosting.HostingBuilder

Represents a builder for configuring hosting extensions.

### Properties

#### DisableMailKit
Gets or sets a value indicating whether MailKit functionality should be disabled. Set to `true` to 
disable MailKit functionality; otherwise, set to `false`. The default value is `false`.

#### DisableMessageQueueing
Gets or sets a value indicating whether message queueing should be disabled. Set to `true` to disable
message queueing; otherwise, set to `false`. The default value is `false`.

## Common.Hosting.ServiceCollectionExtensions

Provides extension methods for configuring common hosting services in the `IServiceCollection`.

### Methods

#### TryCommonHosting
Tries to add common hosting services to the specified `IServiceCollection`.

##### Parameters

* `services`: The instance.
* `configuration`: The configuration containing settings for hosting services.
* `builder`: Optional builder for configuring hosting. Default is `null`.

##### Return value

The updated instance.

This library is designed to make it easy to configure common hosting services in your .NET applications. 
By using the `HostingBuilder` and `ServiceCollectionExtensions` classes, you can quickly and easily add 
the hosting services your application needs.