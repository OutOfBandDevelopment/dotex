using System.Diagnostics.CodeAnalysis;

namespace OoBDev.Census.Geocoding.SpatialServices.Models
{
    [ExcludeFromCodeCoverage]
    public class GeographiesModel
    {
        public StatesModel[] States { get; set; }
        public CountyModel[] Counties { get; set; }
    }
}