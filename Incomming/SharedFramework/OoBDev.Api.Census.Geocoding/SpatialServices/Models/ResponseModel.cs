using System.Diagnostics.CodeAnalysis;

namespace OoBDev.Api.Census.Geocoding.SpatialServices.Models
{
    [ExcludeFromCodeCoverage]
    public class ResponseModel
    {
        public string[] errors { get; set;}
        public ResultModel result { get; set; }
    }
}
