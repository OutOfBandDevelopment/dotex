namespace OoBDev.DevTerm.DeviceManifests.Editing;

/// <summary>One row of the picker's path list: a <see cref="ValuePath"/> plus how to show it.</summary>
public sealed record PickerPath(ValuePath Path, string Detail, string? Example)
{
    /// <summary>The text inserted into an expression for this path (<c>{id}</c>).</summary>
    public string Reference => "{" + Path.Path + "}";

    public override string ToString() => Detail.Length == 0 ? Path.Path : $"{Path.Path}  ({Detail})";
}
