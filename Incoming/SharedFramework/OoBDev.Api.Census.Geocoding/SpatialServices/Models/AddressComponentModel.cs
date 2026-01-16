using System.Diagnostics.CodeAnalysis;

namespace OoBDev.Api.Census.Geocoding.SpatialServices.Models
{
    [ExcludeFromCodeCoverage]
    public class AddressComponentModel
    {
        public string fromAddress { get; set; }
        public string toAddress { get; set; }
        public string preQualifier { get; set; }
        public string preDirection { get; set; }
        public string preType { get; set; }
        public string streetName { get; set; }
        public string suffixType { get; set; }
        public string suffixDirection { get; set; }
        public string suffixQualifier { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string zip { get; set; }
    }
}
