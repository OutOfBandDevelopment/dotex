# OoBDev - Complete

## Summary

This one reference adds the direct OoBDev features for the Common Framework excluding 
ASP.Net Core and Third Party Extensions.  This functionality and more is included in the 
`OoBDev.Common.Complete` library

## Usage

Adding the reference do your project as well as adding the following to your IoC container 
will enable all of the functionality from this framework.  All framework features will be 
available for use, configuration and replacement as required for your application.

### Example

```csharp
using OoBDev.Common;


//Add this after you IServiceCollection registation
        services.TryCommonExtensions();
```