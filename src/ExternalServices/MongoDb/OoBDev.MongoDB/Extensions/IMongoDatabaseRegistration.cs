using System;
using System.Collections.Generic;

namespace OoBDev.MongoDB.Extensions;

/// <summary>
/// Internal registry for MongoDatabase connections.  
/// </summary>
public interface IMongoDatabaseRegistration
{
    /// <summary>
    /// List of registered interfaces for MongoDatabase instances.
    /// </summary>
    IReadOnlyCollection<Type> Types { get; }
}
