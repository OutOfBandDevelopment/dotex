# OoBDev.Identity


## Class: Identity.ServiceCollectionExtensions
Provides extension methods for adding identity-related services to the service collection. 

### Methods


#### TryAddIdentityServices(Microsoft.Extensions.DependencyInjection.IServiceCollection,Microsoft.Extensions.Configuration.IConfiguration)
Tries to add identity-related services to the specified service collection. 


##### Parameters
* *services:* The service collection to which identity services should be added.
* *configuration:* The configuration used to bind options.




##### Return value
The updated service collection.



## Class: Identity.UserManagementProvider
Represents a provider for user management operations. 

Initializes a new instance of the class.
### Methods


#### Constructor
Initializes a new instance of the class.
Represents a provider for user management operations. 


##### Parameters
* *user:* The user manager for managing graph users.




#### CreateAccountAsync(OoBDev.Identity.UserCreateModel)
Creates a new user account asynchronously. 


##### Parameters
* *model:* The model containing user information for account creation.




##### Return value
A task representing the asynchronous operation. The result is a model containing the created user's information.

