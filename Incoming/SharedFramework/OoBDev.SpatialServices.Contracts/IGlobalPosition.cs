namespace OoBDev.SpatialServices.Contracts
{
    public interface IGlobalPosition
    {
        ResultQuality Quality { get; }
        decimal Latitude { get; }
        decimal Longitude { get; }
    }
}
