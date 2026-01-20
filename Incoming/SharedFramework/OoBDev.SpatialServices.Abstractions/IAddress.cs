namespace OoBDev.SpatialServices.Abstractions
{
    public interface IAddress
    {
        string Address { get; }
        string City { get; }
        string State { get; }
        string ZipCode { get; }
        string County { get; }

    }
}
