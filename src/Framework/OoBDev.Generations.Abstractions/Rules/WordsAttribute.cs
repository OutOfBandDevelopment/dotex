using System;
using System.Collections.Generic;
using System.Linq;

namespace OoBDev.Generations.Rules
{
    [AttributeUsage(AttributeTargets.All)]
    public class WordsAttribute : Attribute, IHavePriority
    {
        public static readonly IReadOnlyList<string> DefaultWords = new List<string>{
            "lorem", "ipsum", "dolor", "sit", "amet", "consectetuer",
            "adipiscing", "elit", "sed", "diam", "nonummy", "nibh", "euismod",
            "tincidunt", "ut", "laoreet", "dolore", "magna", "aliquam", "erat"
        }.AsReadOnly();

        public string[] Words { get; set; } = DefaultWords.ToArray();

        public int MinimumWordCount { get; set; } = 1;
        public int MaximumWordCount { get; set; } = 5;
        public string WordSeperator { get; set; } = " ";

        public int MinimumSentenceCount { get; set; } = 1;
        public int MaximumSentenceCount { get; set; } = 5;
        public string SentenceSeperator { get; set; } = ". ";

        public int ParagraphCount { get; set; } = 2;
        public string ParagraphSeperator { get; set; } = Environment.NewLine;
        public int Priority { get; set; } = int.MaxValue;
        public override object TypeId => this;
    }
}
