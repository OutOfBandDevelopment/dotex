using System;

namespace OoBDev.Toolkit.Templating.Models
{
    public class TextTemplateModel
    {
        public Guid Id { get; set; }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public string Name { get; set; }
        public string Template { get; set; }
        public string Language { get; set; }
        public string Country { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    }
}
