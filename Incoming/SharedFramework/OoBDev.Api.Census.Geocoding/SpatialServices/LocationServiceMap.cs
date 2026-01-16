using OoBDev.Api.Census.Geocoding.SpatialServices.Models;
using OoBDev.SpatialServices.Contracts;
using System.Linq;

namespace OoBDev.Api.Census.Geocoding.SpatialServices
{
    public class LocationServiceMap : ILocationServiceMap
    {
        public string AddressAsString(IAddress address) =>
            string.Join(", ", new[]
            {
                address.Address?.Trim(),
                address.City?.Trim(),
                address.State?.Trim(),
                address.ZipCode?.Trim(),
            }.Where(l => !string.IsNullOrEmpty(l)));


        public IAddressResult LocationToAddress(AddressMatchModel location) =>
            new AddressModel
            {
                Address = $"{location?.addressComponents?.fromAddress}..{location?.addressComponents?.toAddress} {location?.addressComponents.streetName}",
                City = location?.addressComponents?.city,
                State = location?.addressComponents?.state,
                ZipCode = location?.addressComponents?.zip,
                County = location?.geographies?.Counties?.FirstOrDefault()?.NAME,

                GlobalPosition = LocationToPosition(location?.coordinates),
                Quality = this.ConvertQuality(),
            };

        public IGlobalPosition LocationToPosition(CoordinatesModel location) =>
            new GlobalPositionModel
            {
                Latitude = (decimal)(location?.y ?? 0),
                Longitude = (decimal)(location?.x ?? 0),
                Quality = ConvertQuality(),
            };

        public ResultQuality ConvertQuality() => ResultQuality.Medium; //No data provided but if it doesnt exist there is no result and the response is within a block not the pinpoint
    }
}
