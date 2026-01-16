using GoogleApi.Entities.Common;
using GoogleApi.Entities.Maps.Geocoding.Common;
using GoogleApi.Entities.Maps.Geocoding.Common.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OoBDev.Api.Google.Maps.SpatialServices
{
    public interface ILocationServiceClient
    {
        Task<(Coordinate? location, GeometryLocationType? quality)> GetPositionAsync(string address);
        Task<IEnumerable<Result>> GetLocationsAsync(string address);
    }
}