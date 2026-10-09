# OoBDev.Keycloak

OoBDev.Keycloak offers functionality for integrating with Keycloak identity management. Let's explore its features:

## KeycloakIdentityOptions

This class represents the options for configuring Keycloak identity.

## ManageKeycloakUser

Manages user operations in Keycloak for both graph and identity aspects.

### Methods

- **Constructor**: Initializes a new instance of the ManageKeycloakUser class.
- **CreateIdentityUserAsync(email, firstName, lastName)**: Creates a new identity user asynchronously with the specified details.
- **GetIdentityUsersByEmail(emailAddress)**: Retrieves a list of user identity models based on the specified email address.
- **RemoveIdentityUserAsync(userId)**: Removes an identity user asynchronously based on the specified object ID.

## ServiceCollectionExtensions

Provides extension methods for configuring services related to Keycloak in the Microsoft.Extensions.DependencyInjection.IServiceCollection.

### Methods

- **TryAddKeycloakServices(services, configuration, keycloakIdentityConfigurationSection)**: Tries to add Keycloak-related services to the specified IServiceCollection.

OoBDev.Keycloak simplifies integration with Keycloak identity management, enabling seamless management of users and their identities.
