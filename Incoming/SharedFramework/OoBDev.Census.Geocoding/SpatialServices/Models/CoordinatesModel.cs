using System.Diagnostics.CodeAnalysis;

namespace OoBDev.Census.Geocoding.SpatialServices.Models
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
