# OoBDev - US Census Geocoding Services

## Summary

This library adds support for OoBDev.SpatialServices to use US Census data for GeoCoding.

## Configuration Options

To select US Census services for ILocationServices you will need the following configuration
and to register this library with you IOC container.

``json
"OoBDev:LocationServices:Type": "CensusGeocodingLocationServices"
```

### Service Configuration

| Key                           | Usage                                                      | Default     |
| ----------------------------- | ---------------------------------------------------------- | ----------- |
| Census:Geocoding:BenchmarkId  | This is the dataset from US Census to use for your queries | 4 (Current) |
| Census:Geocoding:VintageId    | This is the year range of data to use for your queries     | 4 (Current) |
| Census:Geocoding:UrlFormatter | This is the Service URL to access your data                | https://geocoding.geo.census.gov/geocoder/geographies/onelineaddress?address={0}&benchmark={1}&vintage={2}&format=json 

### Keys for URL Formatters

| Value | Function           |
| ----- | ------------------ |
| 0     | Address to geocode |
| 1     | Benchmark ID       |
| 2     | Vintage ID         |
