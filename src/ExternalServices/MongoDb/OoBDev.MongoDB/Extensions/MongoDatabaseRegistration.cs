using System;
using System.Collections.Generic;

namespace OoBDev.MongoDB.Extensions;

/// <summary>
/// Represents a registration of types for MongoDB databases.
/// </summary>
public class MongoDatabaseRegistration : IMongoDatabaseRegistration
{
    /// <summary>
    /// Gets the internal set of types registered for MongoDB databases.
    /// </summary>
    internal HashSet<Type> InternalTypes = [];

    /// <summary>
    /// Gets the read-only collection of types registered for MongoDB databases.
    /// </summary>
    public IReadOnlyCollection<Type> Types => InternalTypes;
}
