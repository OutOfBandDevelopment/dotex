using System.Diagnostics.CodeAnalysis;

namespace OoBDev.Census.Geocoding.SpatialServices.Models
{
    [ExcludeFromCodeCoverage]
    public class ResultModel
    {
        public AddressMatchModel[] addressMatches { get; set; }
    }
}
