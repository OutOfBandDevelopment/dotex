using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.Imaging.People;

/// <summary>
/// Finds pets in an image and names their species. An adapter supplies it; the framework ships none (owner Q22).
/// </summary>
public interface IPetDetector
{
    /// <summary>Finds the pets.</summary>
    /// <param name="image">The encoded image (JPEG, PNG).</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The detections, best first, each with a species.</returns>
    ValueTask<IReadOnlyList<DetectedItem>> DetectAsync(byte[] image, CancellationToken cancellationToken = default);
}
