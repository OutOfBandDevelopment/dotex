README File:

**Summary**
This package contains a set of extensions and constants for working with Claims Principals in .NET. The `ClaimsPrincipalExtensions` class provides methods for iterating and searching Through Claims Principals, while the `CommonClaims` class defines a set of constants for common claims used in authentication.

**Technical Summary**
The `ClaimsPrincipalExtensions` class uses the Observer pattern to iterate and search through claims principals. The `GetAllClaims` method uses LINQ to select and flatten the list of claims from each identity in the claims principal. The `GetClaimValues` method uses LINQ to join the list of claims to an input array of claim types and filter out claims with null or empty values. The `GetClaimValue` method uses the `FirstOrDefault` method to return the first matched claim value.

The `CommonClaims` class is an example of a Singleton pattern, where the constants are defined as public static properties.

**Component Diagram**
```plantuml
@startuml
!include https://raw.githubusercontent.com/plantuml-stdlib/C4-PlantUml/master/c4component.puml

UseCase "User Authentication" as user_authentication

SystemBoundary "OoBDev.Security" as security

Component "ClaimsPrincipalExtensions" as claims_extension {
  UseCase "Get all claims" as get_all_claims
  UseCase "Get claim values" as get_claim_values
  UseCase "Get claim value" as get_claim_value
}

Component "CommonClaims" as common_claims {
  UseCase "Get user ID claim" as get_user_id_claim
  UseCase "Get user culture claim" as get_user_culture_claim
  UseCase "Get extended properties claim" as get_extended_properties_claim
  UseCase "Get application rights claim" as get_application_rights_claim
  UseCase "Get object ID claim" as get_object_id_claim
  UseCase "Get object identifier claim" as get_object_identifier_claim
  UseCase "Get name identifier claim" as get_name_identifier_claim
}

security -> claims_extension: dependencies
claims_extension -> common_claims: dependencies
common_claims -> user_authentication: dependencies
@enduml
```
Note: This component diagram is not exhaustive and only shows the main components and their relationships.