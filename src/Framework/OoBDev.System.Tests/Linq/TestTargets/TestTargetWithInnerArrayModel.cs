using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace OoBDev.System.Tests.Linq.TestTargets;

public class TestTargetWithInnerArrayModel
{
    public TestTargetWithInnerArrayModel(int index)
    {
        Index = index;
        Name = $"{nameof(Name)}{index}";
        Email = $"{nameof(Email)}{index:000}@domain.com";
        Children = Enumerable.Range(index / 100 % 10, index % 10).Select(i => $"Child{i:000}").ToArray();
    }

    [Key]
    public int Index { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }

    public string[] Children { get; set; }
}
