using System;
using System.Collections.Generic;

namespace OoBDev.EventSourcing;

/// <summary>
/// Marks an event class as carrying personal data: which property holds the data subject's id and which properties are
/// blanked when that subject is erased. An event type without the mark is never touched by erasure.
/// </summary>
/// <param name="subjectProperty">The payload property that holds the subject id (it is kept, so later runs still find the event).</param>
/// <param name="fields">The payload properties that are personal data.</param>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
public sealed class PersonalDataAttribute(string subjectProperty, params string[] fields) : Attribute
{
    /// <summary>Gets the property that holds the subject id.</summary>
    public string SubjectProperty { get; } = subjectProperty;

    /// <summary>Gets the personal-data properties.</summary>
    public IReadOnlyList<string> Fields { get; } = fields;
}
