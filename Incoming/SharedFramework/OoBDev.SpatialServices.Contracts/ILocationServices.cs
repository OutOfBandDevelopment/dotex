using OoBDev.Toolkit.DependencyInjection;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OoBDev.SpatialServices.Contracts
{
    /// <summary>
    /// this interface may be used to Geocode and lookup partial addresses
    /// </summary>
    [ContractConfig(
        IsRequired = true,
        RequireConfiguration = true,
        AllowDefault = true,
        ConfigKey = "OoBDev:LocationServices:Type"
        )]
    public interface ILocationServices
    {
        /// <summary>
        /// Geocode best match by Address model
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        Task<IGlobalPosition> AddressToLatLongAsync(IAddress address);

        /// <summary>
        /// Geocode best match by concatenated address 
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        Task<IGlobalPosition> AddressToLatLongAsync(string address);

        /// <summary>
        /// Lookup possible matches by address model
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        Task<IEnumerable<IAddressResult>> LookupAddress(IAddress address);

        /// <summary>
        /// Lookup possible matches by concatenated address 
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        Task<IEnumerable<IAddressResult>> LookupAddress(string address);
    }
}
