using GoogleApi.Entities.Common;
using GoogleApi.Entities.Maps.Geocoding.Common;
using GoogleApi.Entities.Maps.Geocoding.Common.Enums;
using OoBDev.SpatialServices.Contracts;

namespace OoBDev.Api.Google.Maps.SpatialServices
{
    public interface ILocationServiceMap
    {
        string AddressAsString(IAddress address);
        IGlobalPosition LocationToPosition(Coordinate? location, GeometryLocationType? quality);
        ResultQuality ConvertQuality(GeometryLocationType? confidence);
        IAddressResult LocationToAddress(Result location);
    }
}
