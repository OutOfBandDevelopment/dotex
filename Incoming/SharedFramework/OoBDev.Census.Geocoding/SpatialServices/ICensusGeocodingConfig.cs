namespace OoBDev.Census.Geocoding.SpatialServices
{
    public interface ICensusGeocodingConfig
    {
        int BenchmarkId { get;}
        int VintageId { get; }
        string UrlFormatter { get; }
    }
}