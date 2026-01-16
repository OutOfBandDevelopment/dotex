using OoBDev.SpatialServices.Contracts;
using System.Diagnostics.CodeAnalysis;

namespace OoBDev.Api.Microsoft.BingMaps.SpatialServices.Models
{
    [ExcludeFromCodeCoverage]
    public class BingAddress : IAddressResult
    {
        public string Address { get; }
        public string City { get; }
        public string State { get; }
        public string County { get; }
        public string ZipCode { get; }
        public ResultQuality Quality { get; }
        public IGlobalPosition GlobalPosition { get; }

        public BingAddress(
            string address, string city, string state, string county, string zipCode, 
            ResultQuality quality,
            IGlobalPosition position
            )
        {
            this.Address = address;
            this.City = city;
            this.State = state;
            this.County = county;
            this.ZipCode = zipCode;
            this.Quality = quality;
            this.GlobalPosition = position;
        }
    }
}