Here is the generated documentation for the provided source code files, including class diagrams in PlantUML:

**OoBDev.Keycloak**

OoBDev.Keycloak offers functionality for integrating with Keycloak identity management. Let's explore its features:

### KeycloakIdentityOptions

This class represents the options for configuring Keycloak identity.

### ManageKeycloakUser

Manages user operations in Keycloak for both graph and identity aspects.

#### Methods

* **Constructor**: Initializes a new instance of the ManageKeycloakUser class.
* **CreateIdentityUserAsync(email, firstName, lastName)**: Creates a new identity user asynchronously with the specified details.
* **GetIdentityUsersByEmail(emailAddress)**: Retrieves a list of user identity models based on the specified email address.
* **RemoveIdentityUserAsync(userId)**: Removes an identity user asynchronously based on the specified object ID.

### ServiceCollectionExtensions

Provides extension methods for configuring services related to Keycloak in the Microsoft.Extensions.DependencyInjection.IServiceCollection.

#### Methods

* **TryAddKeycloakServices(services, configuration, keycloakIdentityConfigurationSection)**: Tries to add Keycloak-related services to the specified IServiceCollection.

**Class Diagram (PlantUML)**
```plantuml
@startuml
class KeycloakIdentityOptions {
  - config: KeycloakIdentityOptions
}

class ManageKeycloakUser {
  + Constructor()
  + CreateIdentityUserAsync(email, firstName, lastName)
  + GetIdentityUsersByEmail(emailAddress)
  + RemoveIdentityUserAsync(userId)
}

class ServiceCollectionExtensions {
  + TryAddKeycloakServices(services, configuration, keycloakIdentityConfigurationSection)
}

KeycloakIdentityOptions --*: ManageKeycloakUser
ManageKeycloakUser --*: ServiceCollectionExtensions
ServiceCollectionExtensions --*: KeycloakIdentityOptions

@enduml
```
**Readme.Keycloak.md**

As described in the README file, OoBDev.Keycloak simplifies integration with Keycloak identity management, enabling seamless management of users and their identities.

**ServiceCollectionExtensions.cs**

This C# class provides extension methods for configuring services related to Keycloak in the Microsoft.Extensions.DependencyInjection.IServiceCollection.
```csharp
using OoBDev.Identity;
using OoBDev.Keycloak.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace OoBDev.Keycloak
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection TryAddKeycloakServices(
            this IServiceCollection services,
            IConfiguration configuration,
#if DEBUG
            string keycloakIdentityConfigurationSection
#else
            string keycloakIdentityConfigurationSection = nameof(KeycloakIdentityOptions)
#endif
        )
        {
            services.TryAddTransient<IIdentityManager, ManageKeycloakUser>();

            services.Configure<KeycloakIdentityOptions>(options => configuration.Bind(keycloakIdentityConfigurationSection, options));

            return services;
        }
    }
}
```
Note: The PlantUML class diagram shows the relationships between the classes, including the associations between `KeycloakIdentityOptions` and `ManageKeycloakUser`, and between `ManageKeycloakUser` and `ServiceCollectionExtensions`.