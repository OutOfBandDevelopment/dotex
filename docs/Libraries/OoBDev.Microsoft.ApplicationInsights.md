# OoBDev.Microsoft.ApplicationInsights


## Class: Microsoft.ApplicationInsights.Extensibility.CorrelationInfoTelemetryProcessor
Implements an 
 *See: T:Microsoft.ApplicationInsights.Extensibility.ITelemetryProcessor*that adds correlation information to telemetry items. 

### Methods


#### Constructor
Initializes a new instance of the 
 *See: T:OoBDev.Microsoft.ApplicationInsights.Extensibility.CorrelationInfoTelemetryProcessor*class. 


##### Parameters
* *stringAccessor:* The accessor for managing correlation information.




#### Process(Microsoft.ApplicationInsights.Channel.ITelemetry)
Processes telemetry items by adding correlation information to the global properties. 


##### Parameters
* *item:* The telemetry item to process.




## Class: Microsoft.ApplicationInsights.Extensibility.UserTelemetryProcessor
Telemetry processor that extracts user information from the HTTP context and adds it to telemetry items. 

### Methods


#### Constructor
Initializes a new instance of the 
 *See: T:OoBDev.Microsoft.ApplicationInsights.Extensibility.UserTelemetryProcessor*class. 


##### Parameters
* *accessor:* The accessor for accessing the current HTTP context.




#### Process(Microsoft.ApplicationInsights.Channel.ITelemetry)
Processes telemetry items by extracting user information from the HTTP context and adding it to the global properties. 


##### Parameters
* *item:* The telemetry item to process.




## Class: Microsoft.ApplicationInsights.ServiceCollectionExtensions
Extension methods for configuring Application Insights services. 

### Methods


#### TryAddApplicationInsightsExtensions(Microsoft.Extensions.DependencyInjection.IServiceCollection)
Tries to add custom Application Insights telemetry processors to the service collection. 


##### Parameters
* *services:* The service collection to add the processors to.




##### Return value
The modified service collection.

