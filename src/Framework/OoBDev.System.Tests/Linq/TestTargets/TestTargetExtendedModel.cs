using OoBDev.System.ComponentModel.Search;
using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;

namespace OoBDev.System.Tests.Linq.TestTargets;

[Searchable(FirstNameLastName)]
[Searchable(LastNameFirstName)]
[DebuggerDisplay("{FName} {LName}")]
public class TestTargetExtendedModel
{
    public static readonly DateTime BaseDate = new(2020, 1, 1);
    public const string FirstNameLastName = nameof(FirstNameLastName);
    public const string LastNameFirstName = nameof(LastNameFirstName);

    public const string FC = nameof(FC);
    public const string Module = nameof(Module);

    public TestTargetExtendedModel(int index)
    {
        Index = index;
        FName = $"{nameof(FName)}{1000 - index:0000}";
        LName = (index % 5 == 0) ? null : $"{nameof(LName)}{index:0000}";
        Email = $"{nameof(Email)}{index:0000}@domain.com";
        May = (index % 3 == 0) ? "" :
              (index % 5 == 0) ? null :
              (index % 7 == 0) ? "!" :
              index.ToString();

        Modules = index > 0 ? [.. Enumerable.Range(0, index).Select(i => "Module-" + i)] : [];

        Date = BaseDate.AddMonths(index);
        if (index >= 0)
            DateTimeOffsetNullable = DateTimeNullable = Date;
    }

    [Key]
    [DefaultSort]
    public int Index { get; set; }
    [Searchable]
    public required string FName { get; set; }
    [Searchable]
    public string? LName { get; set; }
    [Searchable]
    public required string Email { get; set; }
    [Searchable]
    public string? May { get; set; }

    [Searchable]
    public string[]? Modules { get; set; }
    [Searchable]
    public string[]? NullModules { get; set; }

    [Searchable]
    public DateTime Date { get; set; }

    public DateTime? DateTimeNullable { get; set; }
    public DateTimeOffset? DateTimeOffsetNullable { get; set; }

    public static Expression<Func<TestTargetExtendedModel, object>>? PropertyMap(string key) =>
        key switch
        {
            FirstNameLastName => e => e.FName + " " + e.LName,
            LastNameFirstName => e => e.LName + " " + e.FName,
            _ => null
        };

#pragma warning disable CS8604 // Possible null reference argument.
    public static Expression<Func<TestTargetExtendedModel, bool>>? PredicateMap(string key, object value) =>
        key switch
        {
            Module => e => e.Modules != null && e.Modules.Any(um => um.Equals(value)),
            //UserStatus => e => value.Equals("-1") || e.Active == value.Equals("1"),
            FC => e => e.FName.Contains(value.ToString()),
            _ => null
        };
#pragma warning restore CS8604 // Possible null reference argument.

}
