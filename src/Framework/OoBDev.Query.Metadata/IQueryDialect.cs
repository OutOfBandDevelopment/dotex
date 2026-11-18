namespace OoBDev.Query.Metadata;

/// <summary>
/// The parts of the SQL that differ between databases. The compiler writes everything else.
/// </summary>
public interface IQueryDialect
{
    /// <summary>Quotes an alias.</summary>
    /// <param name="name">The alias.</param>
    /// <returns>The quoted alias.</returns>
    string Quote(string name);

    /// <summary>A case-insensitive pattern match.</summary>
    /// <param name="expr">The expression.</param>
    /// <param name="parameter">The placeholder holding the pattern.</param>
    /// <returns>The condition.</returns>
    string Like(string expr, string parameter);

    /// <summary>The negation of <see cref="Like"/>.</summary>
    /// <param name="expr">The expression.</param>
    /// <param name="parameter">The placeholder holding the pattern.</param>
    /// <returns>The condition.</returns>
    string NotLike(string expr, string parameter);

    /// <summary>The value equals one of the items of a bound array.</summary>
    /// <param name="expr">The expression.</param>
    /// <param name="parameter">The placeholder holding the array.</param>
    /// <returns>The condition.</returns>
    string In(string expr, string parameter);

    /// <summary>A bound value equals one of the items of an array column.</summary>
    /// <param name="parameter">The placeholder holding the value.</param>
    /// <param name="expr">The array expression.</param>
    /// <returns>The condition.</returns>
    string ArrayContains(string parameter, string expr);

    /// <summary>The sort direction with nulls placed last.</summary>
    /// <param name="descending">Whether the sort is descending.</param>
    /// <returns>The text to put after the expression.</returns>
    string Direction(bool descending);

    /// <summary>The row limit clause.</summary>
    /// <param name="rows">The number of rows.</param>
    /// <returns>The clause.</returns>
    string Limit(int rows);

    /// <summary>Text for a part of a date, which sorts in time order.</summary>
    /// <param name="part">year, month, day, hour or weekday.</param>
    /// <param name="expr">The date expression.</param>
    /// <returns>The expression, or null when the dialect has no such part.</returns>
    string? DatePart(string part, string expr);

    /// <summary>Escapes the wildcards of a pattern so the text matches literally.</summary>
    /// <param name="text">The text.</param>
    /// <returns>A contains pattern.</returns>
    string ContainsPattern(string text);

    /// <summary>The grid cell number of a numeric expression, for example <c>floor(expr / cell)</c>.</summary>
    string GridCell(string expr, string cellParameter);

    /// <summary>The great-circle distance in kilometres between a stored position and a bound one.</summary>
    /// <param name="lat">The stored latitude expression.</param>
    /// <param name="lon">The stored longitude expression.</param>
    /// <param name="latParameter">The placeholder holding the anchor latitude.</param>
    /// <param name="lonParameter">The placeholder holding the anchor longitude.</param>
    /// <returns>The distance expression.</returns>
    string Haversine(string lat, string lon, string latParameter, string lonParameter);

    /// <summary>The cosine distance between a stored vector and an anchor vector expression.</summary>
    /// <param name="column">The stored vector expression.</param>
    /// <param name="anchor">An expression that yields the anchor vector.</param>
    /// <returns>The distance expression.</returns>
    string VectorDistance(string column, string anchor);
}
