# OoBDev.Microsoft.B2C


## Class: Microsoft.B2C.Identity.ManageGraphUser
Implementation of 
 *See: T:OoBDev.Identity.IIdentityManager*for managing users in Microsoft Graph. 

Initializes a new instance of the class.
### Methods


#### Constructor
Initializes a new instance of the class.
Implementation of 
 *See: T:OoBDev.Identity.IIdentityManager*for managing users in Microsoft Graph. 


##### Parameters
* *log:* The logger.
* *config:* The configuration.




#### CreateIdentityUserAsync(System.String,System.String,System.String)
Creates a new identity user asynchronously with the specified email, first name, and last name. 


##### Parameters
* *email:* The email address for the new user.
* *firstName:* The first name for the new user.
* *lastName:* The last name for the new user.




##### Return value
A tuple containing the object ID and password of the created user, or null if the user already exists.



#### GetIdentityUsersByEmail(System.String)
Retrieves a list of user identity models based on the provided email address. 


##### Parameters
* *emailAddress:* The email address to query users by.




##### Return value
A list of user identity models matching the specified email address, or null if no matches are found.



#### RemoveIdentityUserAsync(System.String)
Removes an identity user asynchronously based on the specified object ID. 


##### Parameters
* *userId:* The object ID of the user to be removed.




##### Return value
True if the user is successfully removed, false otherwise.



#### GetAuthProvider
Gets the authentication provider for Microsoft Graph. 


##### Return value
The authentication provider.



## Class: Microsoft.B2C.Identity.MicrosoftIdentityOptions
Contains keys related to Azure Active Directory B2C configuration. 

### Properties

#### ClientID
Represents the key for the Azure AD B2C client ID configuration.
#### Issuer
Represents the key for the Azure AD B2C issuer configuration.
#### ClientSecret
Represents the key for the Azure AD B2C client secret configuration.
#### Tenant
Represents the key for the Azure AD B2C tenant configuration.

## Class: Microsoft.B2C.ServiceCollectionExtensions
Extension methods for adding Microsoft B2C services to the service collection. 

### Methods


#### TryAddMicrosoftB2CServices(Microsoft.Extensions.DependencyInjection.IServiceCollection,Microsoft.Extensions.Configuration.IConfiguration,System.String)
Adds Microsoft B2C services to the service collection. 


##### Parameters
* *services:* The service collection to which Microsoft B2C services should be added.
* *configuration:* The to add services to.
* *microsoftIdentityConfigurationSection:* The name for the ConfigurationSectionName.




##### Return value
The modified service collection.

