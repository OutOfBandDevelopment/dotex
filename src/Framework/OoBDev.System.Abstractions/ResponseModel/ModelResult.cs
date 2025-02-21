using System.Collections.Generic;

namespace OoBDev.System.ResponseModel;

/// <summary>
/// Represents a result containing a single model.
/// </summary>
/// <typeparam name="TModel">The type of the model.</typeparam>
public record ModelResult<TModel> : IModelResult<TModel>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ModelResult{TModel}"/> class with the specified data.
    /// </summary>
    /// <param name="data">The model data.</param>
    public ModelResult(TModel data) => Data = data;

    /// <summary>
    /// Gets the model data.
    /// </summary>
    public TModel? Data { get; }

    /// <summary>
    /// Gets the model data as an object.
    /// </summary>
    object? IModelResult.Data => Data;

    /// <summary>
    /// Gets or sets the collection of result messages.
    /// </summary>
    public IReadOnlyCollection<ResultMessage>? Messages { get; init; }
}
