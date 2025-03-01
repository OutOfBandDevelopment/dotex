using System;

namespace OoBDev.System.ComponentModel;

/// <summary>
/// Specifies the name of the connection string to associate with a class.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class ConnectionStringNameAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConnectionStringNameAttribute"/> class 
    /// with the specified connection string name.
    /// </summary>
    /// <param name="connectionStringName">The name of the connection string.</param>
    public ConnectionStringNameAttribute(string connectionStringName) =>
        ConnectionStringName = connectionStringName;

    /// <summary>
    /// Gets the name of the connection string associated with the class.
    /// </summary>
    public string ConnectionStringName { get; }
}
