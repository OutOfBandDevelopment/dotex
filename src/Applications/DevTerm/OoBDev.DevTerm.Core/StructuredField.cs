namespace OoBDev.DevTerm;

/// <summary>One named value of a <see cref="StructuredMessage"/>, with the unit it is in when the decoder knows it.</summary>
public sealed record StructuredField(string Name, string Value, string? Unit = null)
{
    /// <summary><c>temp=21.5 C</c>, or <c>state=OK</c> without a unit.</summary>
    public override string ToString() => Unit is { Length: > 0 } ? $"{Name}={Value} {Unit}" : $"{Name}={Value}";
}
