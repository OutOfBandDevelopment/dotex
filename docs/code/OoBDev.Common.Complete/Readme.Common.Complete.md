# OoBDev - Complete Framework

## Summary

This one reference add the full power of the OoBDev Shared Framework.

## Usage

Adding the reference do your project as well as adding the following to your IoC container 
will enable all of the functionality from this framework.  All framework features will be 
available for use, configuration and replacement as required for your application.

### Example

```csharp
using OoBDev.Common;


//Add this after you IServiceCollection registation
        services.TryAllCommonExtensions();
```