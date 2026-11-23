using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.Imaging.People;

/// <summary>
/// Turns a detected pet into an embedding. An adapter supplies it; the framework ships none (owner Q22).
/// </summary>
public interface IPetEmbedder
{
    /// <summary>Computes the embedding of one detection.</summary>
    /// <param name="image">The encoded image the detection came from.</param>
    /// <param name="item">The detection.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The embedding vector.</returns>
    ValueTask<float[]> EmbedAsync(byte[] image, DetectedItem item, CancellationToken cancellationToken = default);
}
