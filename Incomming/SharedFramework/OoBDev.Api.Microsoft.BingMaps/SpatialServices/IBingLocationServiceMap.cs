using BingMapsRESTToolkit;
using OoBDev.SpatialServices.Contracts;

namespace OoBDev.Api.Microsoft.BingMaps.SpatialServices
{
    public interface IBingLocationServiceMap
    {
        string AddressAsString(IAddress address);
        IGlobalPosition? LocationToPosition(Location location);
        ResultQuality ConvertQuality(ConfidenceLevelType confidence);
        IAddressResult? LocationToAddress(Location? location);
    }
}
