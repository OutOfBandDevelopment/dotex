namespace OoBDev.Generations.Rules
{
    public class FirstSpaceLastNameAttribute : WordsAttribute
    {
        public FirstSpaceLastNameAttribute()
        {
            Words = new[]
            {
                "Thomas",
                "Scott",
                "Martin",
                "Allen",
                "Kelly",
                "Howard",
                "Lee",
                "Russell",
                "Henry",
                "Lewis",
                "Rose",
                "Alexander",
                "Ryan",
                "Lawrence",
                "Mitchell",
                "Nelson",
                "Warren",
                "Douglas",
                "Ray",
                "Bradley",
                "Gordon",
                "Terry",
                "Morris",
                "Stanley",
            };
            MinimumWordCount = 2;
            MaximumWordCount = 2;
            WordSeperator = " ";
            MinimumSentenceCount = 1;
            MaximumSentenceCount = 1;
            SentenceSeperator = "";
            ParagraphCount = 1;
            ParagraphSeperator = "";
            Priority--;
        }
        public override object TypeId => this;
    }
}
