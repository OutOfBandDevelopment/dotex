Create documentation for the follow source code. 

The files should be in markdown.
When reasonable include class diagrams, component models and sequence diagrams in Plant UML.
PlantUML blocks should all be identified with "```plantuml" and closed with "```"

## Source Files

```TestTargetWithInnerListModel.cs
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
        Children = Enumerable.Range(index / 100 % 10, index % 10).Select(i => $"Child{i:000}").ToList();
    }

    [Key]
    public int Index { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }

    public List<string> Children { get; set; }
}

```

