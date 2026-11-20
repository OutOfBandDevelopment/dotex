# OoBDev.Census.Geocoding

US Census one-line address geocoder behind `ILocationServices`. No key; the service is rate limited and has no reverse lookup (reverse returns no match). Register with `services.TryAddCensusGeocoding()` (typed `HttpClient`). Matches map to `Interpolated` when a TIGER line is present, otherwise `Approximate`. Not built: Google, Azure Maps and OSM adapters, the cache decorator and the fallback chain.
