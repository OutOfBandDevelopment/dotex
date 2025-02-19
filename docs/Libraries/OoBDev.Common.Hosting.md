# OoBDev.Common.Hosting


## Class: Common.Hosting.HostingBuilder
Represents a builder for configuring hosting extensions. 

### Properties

#### DisableMailKit
Gets or sets a value indicating whether MailKit functionality should be disabled. Set to true to disable MailKit functionality; otherwise, set to false. The default value is false.
#### DisableMessageQueueing
Gets or sets a value indicating whether message queueing should be disabled. Set to true to disable message queueing; otherwise, set to false. The default value is false.

## Class: Common.Hosting.ServiceCollectionExtensions
Provides extension methods for configuring common hosting services in the 
 *See: T:Microsoft.Extensions.DependencyInjection.IServiceCollection*. 

### Methods


#### TryCommonHosting(Microsoft.Extensions.DependencyInjection.IServiceCollection,Microsoft.Extensions.Configuration.IConfiguration,OoBDev.Common.Hosting.HostingBuilder)
Tries to add common hosting services to the specified 
 *See: T:Microsoft.Extensions.DependencyInjection.IServiceCollection*. 


##### Parameters
* *services:* The instance.
* *configuration:* The configuration containing settings for hosting services.
* *builder:* Optional builder for configuring hosting. Default is null.




##### Return value
The updated instance.

