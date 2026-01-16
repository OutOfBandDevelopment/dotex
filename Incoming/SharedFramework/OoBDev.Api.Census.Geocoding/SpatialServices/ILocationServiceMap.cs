using OoBDev.Api.Census.Geocoding.SpatialServices.Models;
using OoBDev.SpatialServices.Contracts;

namespace OoBDev.Api.Census.Geocoding.SpatialServices
{
    public interface ILocationServiceMap
    {
        string AddressAsString(IAddress address);
        IGlobalPosition LocationToPosition(CoordinatesModel location);
        ResultQuality ConvertQuality();
        IAddressResult LocationToAddress(AddressMatchModel location);
    }
}
