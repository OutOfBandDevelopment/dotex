using System.Collections.Generic;

namespace OoBDev.WebApi.Models;

/// <summary>The outcome of a load and the rows that ended up in the table.</summary>
/// <param name="Read">Rows read.</param>
/// <param name="Written">Rows stored.</param>
/// <param name="Replaced">Rows overwritten.</param>
/// <param name="Skipped">Duplicates left alone.</param>
/// <param name="Errors">Rejected rows with their line.</param>
/// <param name="Columns">The column names of the stored rows.</param>
/// <param name="Rows">The stored rows, values in column order.</param>
public sealed record DataLoadResponseModel(int Read, int Written, int Replaced, int Skipped, IReadOnlyList<DataLoadErrorModel> Errors, IReadOnlyList<string> Columns, IReadOnlyList<IReadOnlyList<string?>> Rows);
