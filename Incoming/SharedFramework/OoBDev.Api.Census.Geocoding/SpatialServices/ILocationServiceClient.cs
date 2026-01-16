using OoBDev.Api.Census.Geocoding.SpatialServices.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OoBDev.Api.Census.Geocoding.SpatialServices
{
    public interface ILocationServiceClient
    {
        Task<CoordinatesModel> GetPositionAsync(string address);
        Task<IEnumerable<AddressMatchModel>> GetLocationsAsync(string address);
    }
}