namespace OoBDev.SpatialServices.Abstractions
{
    public interface IGlobalPosition
    {
        ResultQuality Quality { get; }
        decimal Latitude { get; }
        decimal Longitude { get; }
    }
}
