# OoBDev.AspNetCore.JwtAuthentication


## Class: AspNetCore.JwtAuthentication.JwtExtensionBuilder
Represents a builder for configuring JWT extensions. 

### Properties

#### DefaultSchema
Gets or sets the default authentication schema for JWT. Specifies the default authentication schema used for JWT. The default value is .
#### JwtBearerConfigurationSection
Gets or sets the configuration section name for JwtBearerOptions. Specifies the configuration section name for JwtBearerOptions. The default value is the name of .
#### OAuth2SwaggerConfigurationSection
Gets or sets the configuration section name for OAuth2SwaggerOptions. Specifies the configuration section name for OAuth2SwaggerOptions. The default value is the name of .

## Class: AspNetCore.JwtAuthentication.ServiceCollectionExtensions
Extension methods for configuring JWT Bearer authentication and SwaggerGen services in 
 *See: T:Microsoft.Extensions.DependencyInjection.IServiceCollection*. 

### Methods


#### TryAddJwtBearerServices(Microsoft.Extensions.DependencyInjection.IServiceCollection,Microsoft.Extensions.Configuration.IConfiguration,OoBDev.AspNetCore.JwtAuthentication.JwtExtensionBuilder)
Tries to add JWT Bearer authentication and SwaggerGen services to the specified 
 *See: T:Microsoft.Extensions.DependencyInjection.IServiceCollection*. 


##### Parameters
* *services:* The to add the services to.
* *configuration:* The configuration.
* *builder:* The default authentication scheme.




##### Return value
The modified .



#### TryAddJwtBearerAuthentication(Microsoft.Extensions.DependencyInjection.IServiceCollection,Microsoft.Extensions.Configuration.IConfiguration,System.String,System.String)
Tries to add JWT Bearer authentication services to the specified 
 *See: T:Microsoft.Extensions.DependencyInjection.IServiceCollection*. 


##### Parameters
* *services:* The to add the services to.
* *configuration:* The configuration.
* *defaultScheme:* The default authentication scheme.
* *configurationSection:* The configuration section for JwtBearer options.




##### Return value
The modified .



#### TryAddJwtBearerSwaggerGen(Microsoft.Extensions.DependencyInjection.IServiceCollection,Microsoft.Extensions.Configuration.IConfiguration,System.String)
Tries to add SwaggerGen services for OAuth2 to the specified 
 *See: T:Microsoft.Extensions.DependencyInjection.IServiceCollection*. 


##### Parameters
* *services:* The to add the services to.
* *configuration:* The configuration.
* *configurationSection:* The configuration section for OAuth2Swagger options.




##### Return value
The modified .



## Class: AspNetCore.JwtAuthentication.SwaggerGen.ConfigureOAuthSwaggerGenOptions
Configures SwaggerGen options for OAuth2 authentication. 

Initializes a new instance of the class.
### Methods


#### Constructor
Initializes a new instance of the class.
Configures SwaggerGen options for OAuth2 authentication. 


##### Parameters
* *config:* The OAuth2 Swagger options.
* *logger:* Logger




#### GetScopes
Gets the OAuth2 scopes. 


##### Return value
The OAuth2 scopes.



#### Configure(Swashbuckle.AspNetCore.SwaggerGen.SwaggerGenOptions)
Configures SwaggerGen options for OAuth2 authentication. 


##### Parameters
* *options:* The SwaggerGen options to configure.




## Class: AspNetCore.JwtAuthentication.SwaggerGen.ConfigureOAuthSwaggerUIOptions
Configures SwaggerUI options for OAuth authentication. 

Initializes a new instance of the class.
### Methods


#### Constructor
Initializes a new instance of the class.
Configures SwaggerUI options for OAuth authentication. 


##### Parameters
* *options:* The JwtBearer options.
* *logger:* The logger options.




#### Configure(Swashbuckle.AspNetCore.SwaggerUI.SwaggerUIOptions)
Configures SwaggerUI options for OAuth authentication. 


##### Parameters
* *options:* The SwaggerUI options to configure.




## Class: AspNetCore.JwtAuthentication.SwaggerGen.OAuth2SwaggerOptions
Represents the options for configuring OAuth2 in Swagger. 

### Properties

#### UserReadApiClaim
Gets or sets the claim that Swagger will use to determine the authenticated user's API access.
#### AuthorizationUrl
Gets or sets the URL for the authorization endpoint.
#### TokenUrl
Gets or sets the URL for the token endpoint.