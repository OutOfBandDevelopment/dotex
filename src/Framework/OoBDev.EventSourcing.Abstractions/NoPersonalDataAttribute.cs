using System;

namespace OoBDev.EventSourcing;

/// <summary>
/// Declares that an event class has a <c>SubjectId</c> property but carries no personal data, only a reference to the subject
/// (for example the erasure events themselves). Without this or <see cref="PersonalDataAttribute"/>, the registry refuses
/// such a class so a personal field cannot be forgotten.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
public sealed class NoPersonalDataAttribute : Attribute
{
}
