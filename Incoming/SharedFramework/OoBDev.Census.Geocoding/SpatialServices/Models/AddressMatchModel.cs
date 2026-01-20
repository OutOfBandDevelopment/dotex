using System.Diagnostics.CodeAnalysis;

namespace OoBDev.Census.Geocoding.SpatialServices.Models
{
    [ExcludeFromCodeCoverage]
    public class AddressMatchModel
    {
        public string matchedAddress { get; set; }
        public CoordinatesModel coordinates { get; set; }
        public AddressComponentModel addressComponents { get; set; }
        public GeographiesModel geographies { get; set; }
    }
}