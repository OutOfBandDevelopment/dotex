namespace OoBDev.SpatialServices.Abstractions
{
    public interface IAddressResult : IAddress
    {
        IGlobalPosition GlobalPosition { get; }
        ResultQuality Quality { get; }
    }
}
