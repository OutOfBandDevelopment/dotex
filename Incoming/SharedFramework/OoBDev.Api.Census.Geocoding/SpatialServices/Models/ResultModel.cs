using System.Diagnostics.CodeAnalysis;

namespace OoBDev.Api.Census.Geocoding.SpatialServices.Models
{
    [ExcludeFromCodeCoverage]
    public class ResultModel
    {
        public AddressMatchModel[] addressMatches { get; set; }
    }
}
