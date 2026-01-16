using System.Diagnostics.CodeAnalysis;

namespace OoBDev.Api.Census.Geocoding.SpatialServices.Models
{
    [ExcludeFromCodeCoverage]
    public class GeographiesModel
    {
        public StatesModel[] States { get; set; }
        public CountyModel[] Counties { get; set; }
    }
}