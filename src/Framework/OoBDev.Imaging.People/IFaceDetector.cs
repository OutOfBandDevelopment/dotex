using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.Imaging.People;

/// <summary>
/// Finds faces in an image. An adapter supplies it (an ONNX model, an HTTP service); the framework ships none (owner Q22).
/// </summary>
public interface IFaceDetector
{
    /// <summary>Finds the faces.</summary>
    /// <param name="image">The encoded image (JPEG, PNG).</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The detections, best first.</returns>
    ValueTask<IReadOnlyList<DetectedItem>> DetectAsync(byte[] image, CancellationToken cancellationToken = default);
}
