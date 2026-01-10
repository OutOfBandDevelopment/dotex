using System.Collections.Generic;

namespace OoBDev.System.ResponseModel;

/// <summary>
/// Represents an interface for capturing and publishing result messages.
/// </summary>
public interface ICaptureResultMessage
{
    /// <summary>
    /// Publishes the specified result messages.
    /// </summary>
    /// <param name="resultMessage">The result messages to publish.</param>
    void Publish(params ResultMessage[] resultMessage);

    /// <summary>
    /// Peeks at the captured result messages without removing them.
    /// </summary>
    /// <returns>An IReadOnlyCollection of result messages.</returns>
    IReadOnlyCollection<ResultMessage> Peek();

    /// <summary>
    /// Clears the captured result messages.
    /// </summary>
    void Clear();

    /// <summary>
    /// Captures and returns the result messages, removing them from the capture.
    /// </summary>
    /// <returns>An IReadOnlyCollection of captured result messages.</returns>
    IReadOnlyCollection<ResultMessage> Capture();
}
