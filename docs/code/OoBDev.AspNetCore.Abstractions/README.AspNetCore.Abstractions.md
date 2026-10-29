# OoBDev.AI.Abstractions

## Summary

This library contains the abstract definitions for ASP.Net Core Extensions

## Notes

The document describes the OoBDev.AspNetCore.Abstractions namespace, which includes two classes:

* AspNetCore.Mvc.Filters.ApplicationRightAttribute:
  * Represents an attribute that specifies the required rights for accessing an endpoint.
  * Rights: Represents a list of required rights.

* AspNetCore.Mvc.Filters.ApplicationRightRequirementFilter:
  * Represents an authorization filter that compares application rights for a user to the rights required by an endpoint.
  * OnAuthorization(context): Ensures that the current authenticated user possesses at least one of the requested rights.
  * context: Represents the authorization filter context.
