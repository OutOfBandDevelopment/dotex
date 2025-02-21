# OoBDev.Handlebars


## Class: Handlebars.Helpers.DateNowHelperDescriptor
Descriptor for a Handlebars helper that provides the current date and time. 

### Properties

#### Name
Gets the name of the helper.
#### Helper
Gets the Handlebars helper associated with this descriptor.
### Methods


#### Constructor
Initializes a new instance of the 
 *See: T:OoBDev.Handlebars.Helpers.DateNowHelperDescriptor*class. 


##### Parameters
* *date:* The Date provider.




## Class: Handlebars.Helpers.GetHelperDescriptor
Represents a descriptor for a Handlebars helper that retrieves a value from a state store. 

### Properties

#### Name
Gets the name of the helper.
#### Helper
Gets the Handlebars helper associated with this descriptor. This method retrieves the value associated with the specified key from the state store and writes it to the output.
### Methods


#### Constructor
Initializes a new instance of the 
 *See: T:OoBDev.Handlebars.Helpers.GetHelperDescriptor*class. 


##### Parameters
* *log:* The logger used for logging get operations.




## Class: Handlebars.Helpers.GuidNewHelperDescriptor
Descriptor for a Handlebars helper that generates a new GUID. 

### Properties

#### Name
Gets the name of the helper.
#### Helper
Gets the Handlebars helper associated with this descriptor.
### Methods


#### Constructor
Initializes a new instance of the 
 *See: T:OoBDev.Handlebars.Helpers.GuidNewHelperDescriptor*class. 


##### Parameters
* *guid:* The GUID provider.




## Class: Handlebars.Helpers.HashHelperDescriptor
Represents a descriptor for a Handlebars helper that calculates the hash of a given string. 

### Properties

#### Name
Gets the name of the helper. This property returns the name of the Handlebars helper, which is "hash".
#### Helper
Gets the Handlebars helper associated with this descriptor. This property returns a Handlebars helper function that calculates the hash of a given string using the provided hash provider.
### Methods


#### Constructor
Initializes a new instance of the 
 *See: T:OoBDev.Handlebars.Helpers.HashHelperDescriptor*class. 


##### Parameters
* *hash:* The hash provider.




## Class: Handlebars.Helpers.HelperDescriptorBase
Base class for helper descriptors. 

### Properties

#### Helper
Gets the Handlebars helper associated with this descriptor. This property should return the Handlebars helper implementation.
#### Name
Gets the name of the helper.
### Methods


#### Invoke(HandlebarsDotNet.HelperOptions@,HandlebarsDotNet.Context@,HandlebarsDotNet.Arguments@)
Invokes the helper with the specified options, context, and arguments. 


##### Parameters
* *options:* The options for the helper.
* *context:* The context in which the helper is being executed.
* *arguments:* The arguments passed to the helper.




##### Return value
The result of invoking the helper.



#### Invoke(HandlebarsDotNet.EncodedTextWriter@,HandlebarsDotNet.HelperOptions@,HandlebarsDotNet.Context@,HandlebarsDotNet.Arguments@)
Invokes the helper and writes the result to the output. 


##### Parameters
* *output:* The output writer.
* *options:* The options for the helper.
* *context:* The context in which the helper is being executed.
* *arguments:* The arguments passed to the helper.




## Class: Handlebars.Helpers.SetHelperDescriptor
Represents a descriptor for a Handlebars helper that sets a value in a state store. 

### Properties

#### Name
Gets the name of the helper.
#### Helper
Gets the Handlebars helper associated with this descriptor.
### Methods


#### Constructor
Initializes a new instance of the 
 *See: T:OoBDev.Handlebars.Helpers.SetHelperDescriptor*class. 


##### Parameters
* *log:* The logger for logging set operations.




## Class: Handlebars.Helpers.StateStore
Represents a thread-local store for maintaining state. 

### Properties

#### Item(System.String)
Gets or sets the value associated with the specified key. The key of the value to get or set.The value associated with the specified key.
### Methods


#### TryAdd(System.String,System.Object)
Tries to add the specified key and value to the state store. 


##### Parameters
* *key:* The key of the value to add.
* *value:* The value to add.




##### Return value
True if the key and value were added successfully; otherwise, false.



#### TryGetValue(System.String,System.Object@)
Tries to retrieve the value associated with the specified key from the state store. 


##### Parameters
* *key:* The key of the value to retrieve.
* *value:* When this method returns, contains the value associated with the specified key, if the key is found; otherwise, the default value for the type of the value parameter.




##### Return value
True if the state store contains an element with the specified key; otherwise, false.



## Class: Handlebars.Helpers.StringReplaceHelperDescriptor
Represents a descriptor for a Handlebars helper that performs string replacement. 

### Properties

#### Name
Gets the name of the helper.
#### Helper
Gets the Handlebars helper associated with this descriptor. This helper performs string replacement on the input string using the specified find and replacement strings.

## Class: Handlebars.ServiceCollectionExtensions
Provides extension methods for configuring services related to Handlebars templates in the 
 *See: T:Microsoft.Extensions.DependencyInjection.IServiceCollection*. 

### Methods


#### TryAddHandlebarServices(Microsoft.Extensions.DependencyInjection.IServiceCollection)
Adds services related to Handlebars templates to the specified 
 *See: T:Microsoft.Extensions.DependencyInjection.IServiceCollection*. 


##### Parameters
* *services:* The to add the services to.




##### Return value
The modified with added Handlebars template services.



## Class: Handlebars.Templating.HandlebarsTemplateProvider
Provides Handlebars template processing functionality. 

Initializes a new instance of the class.
### Properties

#### SupportedContentTypes
Gets the collection of supported content types by the template provider. text/x-handlebars-template
### Methods


#### Constructor
Initializes a new instance of the class.
Provides Handlebars template processing functionality. 


##### Parameters
* *helpersRegistry:* The collection of helpers registries.
* *blockHelperDescriptors:* The collection of helpers registries.
* *helperDescriptors:* The collection of helpers registries.
* *log:* The logger instance.




#### CanApply(OoBDev.System.Text.Templating.ITemplateContext)
Determines whether this template provider can apply template processing to the given context. 


##### Parameters
* *context:* The template context.




##### Return value
true if the template processing can be applied; otherwise, false.



#### ApplyAsync(OoBDev.System.Text.Templating.ITemplateContext,System.Object,System.IO.Stream)
Asynchronously applies Handlebars template processing to the specified context, data, and target stream. 


##### Parameters
* *context:* The template context.
* *data:* The data to be used during template processing.
* *target:* The target stream where the processed template is written.




##### Return value
A task representing the asynchronous operation. The task result is true if successful, false otherwise.

