using System;
using System.Collections.Generic;

namespace OoBDev.System.Reflection;

internal record EnumModel : IEnumModel
{
    public int Id { get; init; }
    public required string Name { get; init; }
    public required string Code { get; init; }

    public string? Description { get; init; }

    public int? Order { get; init; }

    public object Value { get; init; } = null!;
    public bool IsEndState { get; init; }
    public bool IsExcludeFromUnique { get; init; }

    public IReadOnlyCollection<string> PossibleNames { get; init; } = Array.Empty<string>();

    public override string ToString() =>
        $"{new { Id, Name, Code, Description, xId = Id.ToString("x") }}";
}
internal record EnumModel<TEnum> : EnumModel, IEnumModel<TEnum> where TEnum : struct, Enum
{
    public new TEnum Value
    {
        get => (TEnum)base.Value;
        init => base.Value = value;
    }
}
