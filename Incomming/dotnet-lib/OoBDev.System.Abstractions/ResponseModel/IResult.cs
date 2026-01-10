using System.Collections.Generic;

namespace OoBDev.System.ResponseModel;

/// <summary>
/// Represents a result, which may include a collection of messages.
/// </summary>
public interface IResult
{
    /// <summary>
    /// Gets a collection of result messages.
    /// </summary>
    IReadOnlyCollection<ResultMessage>? Messages { get; }
}
