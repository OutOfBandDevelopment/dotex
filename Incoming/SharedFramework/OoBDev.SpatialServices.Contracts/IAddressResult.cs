namespace OoBDev.SpatialServices.Contracts
{
    public interface IAddressResult : IAddress
    {
        IGlobalPosition GlobalPosition { get; }
        ResultQuality Quality { get; }
    }
}
