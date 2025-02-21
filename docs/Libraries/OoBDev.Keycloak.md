# OoBDev.Keycloak


## Class: Keycloak.Identity.KeycloakIdentityOptions
Represents the options for configuring Keycloak identity. 


## Class: Keycloak.Identity.ManageKeycloakUser
Manages user operations in Keycloak for both graph and identity aspects. 

### Methods


#### Constructor
Manages user operations in Keycloak for both graph and identity aspects. 


##### Parameters
* *log:* The logger for logging purposes.
* *config:* The configuration options for Keycloak identity.




#### CreateIdentityUserAsync(System.String,System.String,System.String)
Creates a new identity user asynchronously with the specified details. 


##### Parameters
* *email:* The email address of the user.
* *firstName:* The first name of the user.
* *lastName:* The last name of the user.




##### Return value
A task that represents the asynchronous operation. The task result contains the object ID and an optional password for the created user.



#### GetIdentityUsersByEmail(System.String)
Retrieves a list of user identity models based on the specified email address. 


##### Parameters
* *emailAddress:* The email address to search for.




##### Return value
A task that represents the asynchronous operation. The task result contains the list of user identity models or null if no users are found.



#### RemoveIdentityUserAsync(System.String)
Removes an identity user asynchronously based on the specified object ID. 


##### Parameters
* *userId:* The object ID of the user to remove.




##### Return value
A task that represents the asynchronous operation. The task result is true if the user was successfully removed; otherwise, false.



## Class: Keycloak.ServiceCollectionExtensions
Provides extension methods for configuring services related to Keycloak in the 
 *See: T:Microsoft.Extensions.DependencyInjection.IServiceCollection*. 

### Methods


#### TryAddKeycloakServices(Microsoft.Extensions.DependencyInjection.IServiceCollection,Microsoft.Extensions.Configuration.IConfiguration,System.String)
Tries to add Keycloak-related services to the specified 
 *See: T:Microsoft.Extensions.DependencyInjection.IServiceCollection*. 


##### Parameters
* *services:* The instance.
* *configuration:* The configuration containing Keycloak-related settings.
* *keycloakIdentityConfigurationSection:* The configuration section name for Keycloak identity options. Default is "KeycloakIdentityOptions".




##### Return value
The updated instance.

