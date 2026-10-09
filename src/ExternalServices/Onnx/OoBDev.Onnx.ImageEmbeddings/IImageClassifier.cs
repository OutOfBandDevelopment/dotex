using Microsoft.Extensions.AI;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.Onnx.ImageEmbeddings;

/// <summary>Assigns labels to an image.</summary>
public interface IImageClassifier
{
    /// <summary>Returns the most likely labels, best first.</summary>
    /// <param name="image">Encoded image.</param>
    /// <param name="topK">Number of labels to return.</param>
    /// <param name="cancellationToken">Cancellation.</param>
    /// <returns>Up to <paramref name="topK"/> labels with probabilities.</returns>
    Task<IReadOnlyList<ImageLabel>> ClassifyAsync(DataContent image, int topK = 5, CancellationToken cancellationToken = default);
}
