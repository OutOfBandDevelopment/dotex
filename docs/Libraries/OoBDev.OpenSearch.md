# OoBDev.OpenSearch


## Class: OpenSearch.IOpenSearchClientFactory
Factory interface for creating instances of the 
 *See: T:OpenSearch.Net.IOpenSearchLowLevelClient*. 

### Methods


#### Create
Creates a new instance of the 
 *See: T:OpenSearch.Net.IOpenSearchLowLevelClient*. 


##### Return value
A new instance of the .



## Class: OpenSearch.LexicalProvider
Provides functionality for storing and searching lexical content using OpenSearch. 


## Class: OpenSearch.OpenSearchClientFactory
Implementation of the 
 *See: T:OoBDev.OpenSearch.IOpenSearchClientFactory*interface for creating instances of the 
 *See: T:OpenSearch.Net.IOpenSearchLowLevelClient*. 

### Methods


#### Constructor
Initializes a new instance of the 
 *See: T:OoBDev.OpenSearch.OpenSearchClientFactory*class. 


##### Parameters
* *config:* The configuration options for the OpenSearch client.




#### Create
Creates a new instance of the 
 *See: T:OpenSearch.Net.IOpenSearchLowLevelClient*. 


##### Return value
A new instance of the .



## Class: OpenSearch.OpenSearchOptions
Configuration options for the OpenSearch client. 

### Properties

#### HostName
Gets or sets the hostname of the OpenSearch server.
#### Port
Gets or sets the port number of the OpenSearch server.
#### IndexName
Gets or sets the index name used for OpenSearch operations.
#### UserName
Gets or sets the username for authentication (if required).
#### Password
Gets or sets the password for authentication (if required).

## Class: OpenSearch.ServiceCollectionExtensions
Provides extension methods for configuring services related to OpenSearch. 

### Methods


#### TryAddOpenSearchServices(Microsoft.Extensions.DependencyInjection.IServiceCollection,Microsoft.Extensions.Configuration.IConfiguration,System.String)
Configures services for OpenSearch. 


##### Parameters
* *services:* The to add the services to.
* *configuration:* The to bind OpenSearch options from.
* *openSearchOptionSections:* The configuration section name containing OpenSearch options.




##### Return value
The modified .

