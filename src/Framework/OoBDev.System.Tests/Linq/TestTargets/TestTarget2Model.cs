using OoBDev.System.ComponentModel.Search;
using System.ComponentModel.DataAnnotations;

namespace OoBDev.System.Tests.Linq.TestTargets;

[Searchable("Fake")]
public class TestTarget2Model
{
    [Key]
    [NotSortable]
    public int Index { get; set; }
    [Searchable]
    public string Name { get; set; } = default!;
    [NotFilterable]
    public string Email { get; set; } = default!;
}
