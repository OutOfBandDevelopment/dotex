using BingMapsRESTToolkit;
using OoBDev.Api.Microsoft.BingMaps.SpatialServices.Models;
using OoBDev.SpatialServices.Contracts;
using System.Linq;

namespace OoBDev.Api.Microsoft.BingMaps.SpatialServices
{
    public class BingLocationServiceMap : IBingLocationServiceMap
    {
        public string AddressAsString(IAddress address) =>
            string.Join(", ", new[]
            {
                address.Address?.Trim(),
                address.City?.Trim(),
                address.State?.Trim(),
                address.ZipCode?.Trim(),
            }.Where(l => !string.IsNullOrEmpty(l)));

        public ResultQuality ConvertQuality(ConfidenceLevelType confidence) =>
            confidence switch
            {
                ConfidenceLevelType.High => ResultQuality.High,
                ConfidenceLevelType.Low => ResultQuality.Low,
                ConfidenceLevelType.Medium => ResultQuality.Medium,
                _ => ResultQuality.Unknown,
            };

        public IAddressResult? LocationToAddress(Location? location) =>
            location switch
            {
                null => null,
                _ => new BingAddress(
                     address: location?.Address.AddressLine ?? "",
                     city: location?.Address.Locality ?? "",
                     state: location?.Address.AdminDistrict ?? "",
                     county: location?.Address.AdminDistrict2 ?? "",
                     zipCode: location?.Address.PostalCode ?? "",
                     quality: ConvertQuality(location?.ConfidenceLevelType ?? ConfidenceLevelType.None),
                     position: LocationToPosition(location) ?? BingGlobalPosition.Unknown
                     )
            };

        public IGlobalPosition? LocationToPosition(Location? location) =>
            location?.Point?.Coordinates switch
            {
                double[] coor when coor.Length == 2 => new BingGlobalPosition(
                    coor.ElementAt(0),
                    coor.ElementAt(1),
                    this.ConvertQuality(location.ConfidenceLevelType)
                    ),
                _ => null
            };
    }
}
