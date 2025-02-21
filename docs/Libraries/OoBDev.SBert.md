# OoBDev.SBert


## Class: SBert.ISentenceEmbeddingClient
Represents a client for generating sentence embeddings. 

### Methods


#### GetEmbeddingAsync(System.String)
Asynchronously generates a sentence embedding as an array of single-precision floating-point numbers. 


##### Parameters
* *input:* The input sentence to generate the embedding for.




##### Return value
An array of single-precision floating-point numbers representing the embedding of the input sentence.



#### GetEmbeddingDoubleAsync(System.String)
Asynchronously generates a sentence embedding as an array of double-precision floating-point numbers. 


##### Parameters
* *input:* The input sentence to generate the embedding for.




##### Return value
An array of double-precision floating-point numbers representing the embedding of the input sentence.



#### GetHealthAsync
Check the health status of the sentence embedding service 


##### Return value




## Class: SBert.SbertHealthCheck
Represents a health check implementation for SBERT (Sentence-BERT) service. 

### Methods


#### Constructor
Initializes a new instance of the 
 *See: T:OoBDev.SBert.SbertHealthCheck*class. 


##### Parameters
* *client:* The SBERT client used for health checks.




#### CheckHealthAsync(Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckContext,System.Threading.CancellationToken)
Checks the health of the SBERT service asynchronously. 


##### Parameters
* *context:* The health check context.
* *cancellationToken:* The cancellation token.




##### Return value
A task representing the asynchronous health check operation.



## Class: SBert.SentenceEmbeddingClient
Client for interacting with SBert. 

### Methods


#### Constructor
Initializes a new instance of the 
 *See: T:OoBDev.SBert.SentenceEmbeddingClient*class. 


##### Parameters
* *httpClient:* The HttpClient.




#### GetEmbeddingAsync(System.String)
Retrieves the embedding vector for the given input as an array of single-precision floats. 


##### Parameters
* *input:* The input text.




##### Return value
An array of single-precision floats representing the embedding.



#### GetEmbeddingDoubleAsync(System.String)
Retrieves the embedding vector for the given input as an array of double-precision floats. 


##### Parameters
* *input:* The input text.




##### Return value
An array of double-precision floats representing the embedding.



#### GetHealthAsync
Check the health status of the sentence embedding service 


##### Return value




## Class: SBert.SentenceEmbeddingOptions
Options for configuring SBert. 

### Properties

#### Url
Gets or sets the URL for SBert. Example: http://sbert.example.com:5080

## Class: SBert.SentenceEmbeddingProvider
Provides sentence embeddings using SBert. 

### Properties

#### Length
Gets the length of the embeddings.
### Methods


#### Constructor
Initializes a new instance of the 
 *See: T:OoBDev.SBert.SentenceEmbeddingProvider*class. 


##### Parameters
* *client:* The ISentenceEmbeddingClient for obtaining embeddings.
* *logger:* The ILogger{SentenceEmbeddingProvider} instance for logging.




#### GetEmbeddingAsync(System.String,System.String)
Gets the embedding for the given content asynchronously. 


##### Parameters
* *content:* The content for which to obtain the embedding.
* *model:* The model for which to obtain the embedding.




##### Return value
A task representing the asynchronous operation. The task result contains the embedding as an array of floats.



## Class: SBert.ServiceCollectionExtensions
Provides extension methods for configuring services related to SBERT (Sentence-BERT). 

### Methods


#### TryAddSbertServices(Microsoft.Extensions.DependencyInjection.IServiceCollection,Microsoft.Extensions.Configuration.IConfiguration,System.String)
Configures services for SBERT (Sentence-BERT). 


##### Parameters
* *services:* The to add the services to.
* *configuration:* The to bind SBERT options from.
* *sentenceEmbeddingOptionSection:* The configuration section name containing SBERT options.




##### Return value
The modified .

