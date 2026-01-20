
using System.Collections.Generic;

namespace OoBDev.Toolkit.Templating.Models
{
    public class TemplateSummaryModel
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public string Name { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public List<string> Cultures { get; set; } = new List<string>();
    }
}
