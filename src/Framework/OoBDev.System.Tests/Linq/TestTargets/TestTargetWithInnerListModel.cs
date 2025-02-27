using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace OoBDev.System.Tests.Linq.TestTargets;

public class TestTargetWithInnerListModel
{
    public TestTargetWithInnerListModel(int index)
    {
        Index = index;
        Name = $"{nameof(Name)}{index}";
        Email = $"{nameof(Email)}{index:000}@domain.com";
        Children = [.. Enumerable.Range(index / 100 % 10, index % 10).Select(i => $"Child{i:000}")];
    }

    [Key]
    public int Index { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }

    public List<string> Children { get; set; }
}
