#nullable disable

using OoBDev.SpatialServices.Contracts;
using System.Diagnostics.CodeAnalysis;

namespace OoBDev.Api.Google.Maps.SpatialServices.Models
{
    [ExcludeFromCodeCoverage]
    public class AddressModel : IAddressResult
    {
        public IGlobalPosition GlobalPosition { get; internal set; }
        public ResultQuality Quality { get; internal set; }
        public string Address { get; internal set; }
        public string City { get; internal set; }
        public string State { get; internal set; }
        public string ZipCode { get; internal set; }
        public string County { get; internal set; }

        public override string ToString() =>
            new { 
                GlobalPosition,
                Quality,
                Address,
                City,
                State,
                ZipCode,
                County,
            }.ToString();
    }
}