# OoBDev.Microsoft.B2C

OoBDev.Microsoft.B2C provides implementations and extensions for integrating with Microsoft Azure Active Directory B2C. Here's an overview of its key components:

## ManageGraphUser
Implementation of `OoBDev.Identity.IIdentityManager` for managing users in Microsoft Graph.

### Methods
- **Constructor**: Initializes a new instance of the class.
- **CreateIdentityUserAsync**: Creates a new identity user asynchronously with the specified email, first name, and last name.
- **GetIdentityUsersByEmail**: Retrieves a list of user identity models based on the provided email address.
- **RemoveIdentityUserAsync**: Removes an identity user asynchronously based on the specified object ID.
- **GetAuthProvider**: Gets the authentication provider for Microsoft Graph.

## MicrosoftIdentityOptions
Contains keys related to Azure Active Directory B2C configuration.

### Properties
- **ClientID**: Represents the key for the Azure AD B2C client ID configuration.
- **Issuer**: Represents the key for the Azure AD B2C issuer configuration.
- **ClientSecret**: Represents the key for the Azure AD B2C client secret configuration.
- **Tenant**: Represents the key for the Azure AD B2C tenant configuration.

## ServiceCollectionExtensions
Extension methods for adding Microsoft B2C services to the service collection.

### Methods
- **TryAddMicrosoftB2CServices**: Adds Microsoft B2C services to the service collection.

OoBDev.Microsoft.B2C streamlines integration with Microsoft Azure Active Directory B2C, facilitating user management and authentication workflows within applications.
