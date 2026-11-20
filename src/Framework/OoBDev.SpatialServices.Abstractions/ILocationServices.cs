using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.SpatialServices;

/// <summary>Turns addresses into positions and back through one contract and swappable vendors.</summary>
public interface ILocationServices
{
    /// <summary>Finds the position of an address; never throws for a vendor failure.</summary>
    /// <param name="address">The address.</param>
    /// <param name="cancellationToken">Cancellation.</param>
    Task<GeocodeResult> GeocodeAsync(AddressQuery address, CancellationToken cancellationToken);

    /// <summary>Finds the address at a position; a vendor without this returns <see cref="GeocodeResult.NoMatch"/>.</summary>
    /// <param name="position">The position.</param>
    /// <param name="cancellationToken">Cancellation.</param>
    Task<GeocodeResult> ReverseGeocodeAsync(GlobalPosition position, CancellationToken cancellationToken);
}
