using System.Collections.Generic;

namespace OoBDev.DataLoader;

/// <summary>Outcome of a load.</summary>
/// <param name="Read">Rows read.</param>
/// <param name="Written">Rows stored.</param>
/// <param name="Replaced">Rows overwritten.</param>
/// <param name="Skipped">Duplicates left alone.</param>
/// <param name="Errors">Rejected rows with line.</param>
public sealed record LoadReport(int Read, int Written, int Replaced, int Skipped, IReadOnlyList<RowError> Errors);
