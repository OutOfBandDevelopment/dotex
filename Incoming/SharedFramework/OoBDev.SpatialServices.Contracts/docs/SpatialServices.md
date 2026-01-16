# OoBDev - Spatial Services

## Summary

These services may be used to lookup incomplete addresses as well as geocode
to latitude and longitude. 

When used with `ISelectedService<>` and multiple service providers have been 
registered in the IOC container for this service.  You may configure a preferred
provider by setting the configuration value `OoBDev:LocationServices:Type` to
the full name of that provider.  Configuration is required is multiple providers 
are registered.

## ILocationServices

### Task<IGlobalPosition> AddressToLatLongAsync(IAddress address)

Geocode best match by Address mode

### Task<IGlobalPosition> AddressToLatLongAsync(string address)

Geocode best match by concatenated address 


### Task<IEnumerable<IAddressResult>> LookupAddress(IAddress address)

Lookup possible matches by address model

### Task<IEnumerable<IAddressResult>> LookupAddress(string address)

Lookup possible matches by concatenated address