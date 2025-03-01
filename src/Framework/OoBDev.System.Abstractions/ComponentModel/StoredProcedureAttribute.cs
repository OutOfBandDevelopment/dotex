using System;

namespace OoBDev.System.ComponentModel;

/// <summary>
/// Specifies the name of a stored procedure associated with a class.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class StoredProcedureAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StoredProcedureAttribute"/> class
    /// with the specified stored procedure name.
    /// </summary>
    /// <param name="storedProcedureName">The name of the stored procedure.</param>
    public StoredProcedureAttribute(string storedProcedureName) =>
        StoredProcedureName = storedProcedureName;

    /// <summary>
    /// Gets the name of the stored procedure associated with the class.
    /// </summary>
    public string StoredProcedureName { get; }
}
