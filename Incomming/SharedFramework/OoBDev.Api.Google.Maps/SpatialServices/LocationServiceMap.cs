using GoogleApi.Entities.Common;
using GoogleApi.Entities.Common.Enums;
using GoogleApi.Entities.Maps.Geocoding.Common;
using GoogleApi.Entities.Maps.Geocoding.Common.Enums;
using OoBDev.Api.Google.Maps.SpatialServices.Models;
using OoBDev.SpatialServices.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using static GoogleApi.Entities.Common.Enums.AddressComponentType;

namespace OoBDev.Api.Google.Maps.SpatialServices
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

        public IGlobalPosition LocationToPosition(Coordinate? location, GeometryLocationType? confidence) =>
             new GlobalPositionModel
             {
                 Latitude = (decimal)(location?.Latitude ?? 0),
                 Longitude = (decimal)(location?.Longitude ?? 0),
                 Quality = ConvertQuality(confidence),
             };

        public ResultQuality ConvertQuality(GeometryLocationType? confidence) =>
            confidence switch
            {
                GeometryLocationType.Rooftop => ResultQuality.High,
                GeometryLocationType.Geometric_Center => ResultQuality.Medium,
                GeometryLocationType.Range_Interpolated => ResultQuality.Medium,
                GeometryLocationType.Approximate => ResultQuality.Low,
                _ => ResultQuality.Unknown,
            };

        public IAddressResult LocationToAddress(Result location) =>
            new AddressModel
            {
                Address = FromParts(location.AddressComponents, Street_Number, Route),
                City = FromParts(location.AddressComponents, Locality),
                State = FromParts(location.AddressComponents, Administrative_Area_Level_1),
                ZipCode = FromParts(location.AddressComponents, Postal_Code),
                County = FromParts(location.AddressComponents, Administrative_Area_Level_2),

                GlobalPosition = LocationToPosition(location?.Geometry?.Location, location?.Geometry?.LocationType),
                Quality = this.ConvertQuality(location?.Geometry?.LocationType),
            };

        private string FromParts(IEnumerable<AddressComponent> addressComponents, params AddressComponentType[] types)
        {
            var values = from type in types
                         let value = addressComponents.FirstOrDefault(item => item.Types.Contains(type))
                         where !string.IsNullOrWhiteSpace(value?.LongName)
                         select value.LongName;
            var mapped = string.Join(" ", values);
            return mapped;
        }
    }
}
