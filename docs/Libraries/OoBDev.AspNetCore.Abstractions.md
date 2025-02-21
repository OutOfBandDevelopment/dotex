# OoBDev.AspNetCore.Abstractions


## Class: AspNetCore.Mvc.Filters.ApplicationRightAttribute
At least one of these declared rights must be assigned to the user to access this point 

### Properties

#### Rights
list of required rights
### Methods


#### Constructor
Declare required rights for endpoint 


##### Parameters
* *rights:* 




## Class: AspNetCore.Mvc.Filters.ApplicationRightRequirementFilter
Authorization filter to compared application rights for user to rights required by endpoint 

### Methods


#### Constructor
Authorization filter to compared application rights for user to rights required by endpoint 


#### OnAuthorization(Microsoft.AspNetCore.Mvc.Filters.AuthorizationFilterContext)
Ensure that current authenticated user matches as least one requested right 


##### Parameters
* *context:* 


