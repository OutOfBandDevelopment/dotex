using OoBDev.System.ComponentModel.Search;
using System.ComponentModel.DataAnnotations;

namespace OoBDev.System.Tests.Linq.TestTargets;

public class TestTarget3Model
{
    [Key]
    [NotSearchable]
    public int Index { get; set; }
    [NotSortable]
    public string Name { get; set; } = default!;
    [NotFilterable]
    public string Email { get; set; } = default!;
}
