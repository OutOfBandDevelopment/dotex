using System.Collections.Generic;

namespace OoBDev.System.ResponseModel;

/// <summary>
/// Represents a generic interface for a result containing model data and messages.
/// </summary>
public interface IModelResult
{
    /// <summary>
    /// Gets the data associated with the result.
    /// </summary>
    object? Data { get; }

    /// <summary>
    /// Gets a collection of messages associated with the result.
    /// </summary>
    IReadOnlyCollection<ResultMessage>? Messages { get; }
}

/// <summary>
/// Represents a generic interface for a result containing model data of type <typeparamref name="TModel"/> and messages.
/// </summary>
/// <typeparam name="TModel">The type of the model data.</typeparam>
public interface IModelResult<TModel> : IModelResult
{
    /// <summary>
    /// Gets the strongly-typed data associated with the result.
    /// </summary>
    new TModel? Data { get; }
}
