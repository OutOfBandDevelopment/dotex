using OoBDev.SpatialServices.Contracts;
using System.Diagnostics.CodeAnalysis;

namespace OoBDev.Census.Geocoding.SpatialServices.Models
{
    [ExcludeFromCodeCoverage]
    public class GlobalPositionModel : IGlobalPosition
    {
        public ResultQuality Quality { get; internal set; }
        public decimal Latitude { get; internal set; }
        public decimal Longitude { get; internal set; }

        public override string ToString() =>
            new {
                Quality,
                Latitude,
                Longitude,
            }.ToString();
    }
}