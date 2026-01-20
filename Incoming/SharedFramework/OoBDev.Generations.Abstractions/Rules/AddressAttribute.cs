using System;

namespace OoBDev.Generations.Rules
{
    [AttributeUsage( AttributeTargets.All)]
    public class AddressAttribute : Attribute, IGenerateObject
    {
        public int Priority { get; }

        public string[] Streets { get; set; } = new[]
        {
            "Main",
            "Broad",
            "1st",
            "3rd",
            "Oak",
            "Elm",
            "Pine",
            "Grant",
            "Washington",
            "Maple",
            "Park",
            "Ridge",
            "High",
        };
        public string[] StreetTypes { get; set; } = new[]
        {
            "St",
            "Dr",
            "Ave",
            "Pt",
            "Ct",
            "Ln",
            "Hwy",
            "Blvd",
            "Pl",
            "Way",
            "Run",
            "Ter",
            "Rd",
            "Plz",
        };
        public string[] Cities { get; set; } = new[]
        {
            "Washington",
            "Springfield",
            "Franklin",
            "Greenville",
            "Bristol",
            "Clinton",
            "Fairview",
            "Georgetown",
            "Madison",
            "Salem",
        };

        public string[] States { get; set; } = new[]
        {
            "AL",
            "AK",
            "AZ",
            "AR",
            "CA",
            "CO",
            "CT",
            "DE",
            "FL",
            "GA",
            "HI",
            "ID",
            "IL",
            "IN",
            "IA",
            "KS",
            "KY",
            "LA",
            "ME",
            "MD",
            "MA",
            "MI",
            "MN",
            "MS",
            "MO",
            "MT",
            "NE",
            "NV",
            "NH",
            "NJ",
            "NM",
            "NY",
            "NC",
            "ND",
            "OH",
            "OK",
            "OR",
            "PA",
            "RI",
            "SC",
            "SD",
            "TN",
            "TX",
            "UT",
            "VT",
            "VA",
            "WA",
            "WV",
            "WI",
            "WY",
        };


        public bool CanGenerateValue(IProcedualGenerationContext context) =>
            context.TargetType == typeof(string);

        public object? GenerateValue(IProcedualGenerationContext context) =>
            $@"{context.Random.Next(1, 9999)} {context.ChooseFrom(Streets)} {context.ChooseFrom(StreetTypes)}{context.Random.Next(0, 99) switch
            {
                int apt when apt > 10 => $", Apt {apt/10}",
                _ => ""
            }}
{context.ChooseFrom(Cities)} {context.ChooseFrom(States)} {context.Random.Next(9000, 99999)}";
    }
}
