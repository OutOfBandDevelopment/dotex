using System.Diagnostics.CodeAnalysis;

namespace OoBDev.Api.Census.Geocoding.SpatialServices.Models
{
    [ExcludeFromCodeCoverage]
    public class CoordinatesModel
    {
        /// <summary>
        /// long
        /// </summary>
        public double x { get; set; }
        /// <summary>
        /// lat
        /// </summary>
        public double y { get; set; }
    }
}
