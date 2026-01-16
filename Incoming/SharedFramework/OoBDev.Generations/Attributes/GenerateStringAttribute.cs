using OoBDev.Generations.Rules;
using System;
using System.Linq;
using System.Text;

namespace OoBDev.Generations.Attributes
{
    public class GenerateStringAttribute : GenerateNullableAttribute
    {
        public override bool CanGenerateValue(IProcedualGenerationContext context) =>
            new[] {
                typeof(string),
            }.Contains(context.TargetType);

        protected override object? OnGenerateValue(IProcedualGenerationContext context)
        {
            var rule = context.GetRule<WordsAttribute>();

            // https://stackoverflow.com/questions/4286487/is-there-any-lorem-ipsum-generator-in-c

            //TODO: add random capitalization

            int numSentences = context.Random.Next(rule.MaximumSentenceCount - rule.MinimumSentenceCount) + rule.MinimumSentenceCount;
            int numWords = context.Random.Next(rule.MaximumWordCount - rule.MinimumWordCount) + rule.MinimumWordCount;

            var result = new StringBuilder();

            for (int p = 0; p < rule.ParagraphCount; p++)
            {
                for (int s = 0; s < numSentences; s++)
                {
                    for (int w = 0; w < numWords; w++)
                    {
                        if (w > 0) { result.Append(rule.WordSeperator); }
                        result.Append(rule.Words[context.Random.Next(rule.Words.Length)]);
                    }
                    result.Append(rule.SentenceSeperator);
                }
                result.Append(rule.ParagraphSeperator);
            }

            return result.ToString();
        }
    }
}
