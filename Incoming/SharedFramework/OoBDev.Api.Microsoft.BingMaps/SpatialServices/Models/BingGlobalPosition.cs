using OoBDev.SpatialServices.Contracts;
using System.Diagnostics.CodeAnalysis;

namespace OoBDev.Api.Microsoft.BingMaps.SpatialServices.Models
{
    [ExcludeFromCodeCoverage]
    public class BingGlobalPosition : IGlobalPosition
    {
        public BingGlobalPosition(decimal latitude, decimal longitude, ResultQuality resultQuality) =>
            (Latitude, Longitude, Quality) = (latitude, longitude, resultQuality);
        public BingGlobalPosition(double latitude, double longitude, ResultQuality resultQuality) :
            this((decimal)latitude, (decimal)longitude, resultQuality)
        { }

        public static IGlobalPosition Unknown { get; } = new BingGlobalPosition(-1.0m, -1.0m, ResultQuality.Unknown);

        public decimal Latitude { get; }
        public decimal Longitude { get; }
        public ResultQuality Quality { get; }
    }
}
