using System.Collections.Generic;

namespace OoBDev.Query.Metadata;

/// <summary>
/// SQL plus its bound values. Nothing from the request is part of <see cref="Sql"/>.
/// </summary>
/// <param name="Sql">The statement.</param>
/// <param name="Parameters">The bound values by name.</param>
/// <param name="CountSql">The count statement, or null.</param>
/// <param name="Top">The page size.</param>
/// <param name="SortSpec">The normalized sort the cursor belongs to.</param>
/// <param name="SortColumns">How many <c>__s</c> columns the rows carry for the next cursor.</param>
public sealed record CompiledQuery(string Sql, IReadOnlyDictionary<string, object?> Parameters, string? CountSql, int Top, string SortSpec, int SortColumns);
