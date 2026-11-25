namespace OoBDev.Data.DesiredState;

/// <summary>What a plan step can do to existing data.</summary>
public enum HazardKind
{
    /// <summary>Removes a table, column, schema or database: data is lost.</summary>
    DataLoss,

    /// <summary>Removes another object (index, constraint, view, function); no row data is lost, but the object is gone.</summary>
    Drop,

    /// <summary>Changes a column type; the new type may be narrower than the old one and the engine may rewrite the table.</summary>
    TypeChange,
}
