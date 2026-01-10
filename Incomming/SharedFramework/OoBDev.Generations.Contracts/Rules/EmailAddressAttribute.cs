using System;

namespace OoBDev.Generations.Rules
{
    [AttributeUsage(AttributeTargets.All)]
    public class EmailAddressAttribute : Attribute, IGenerateObject
    {
        public int Priority { get; }

        public bool CanGenerateValue(IProcedualGenerationContext context) =>
            context.TargetType == typeof(string);

        public string[] Users { get; set; } = new[] {
            "tmcdonald51",
            "amelie.ochoa",
            "bridgesb15",
            "nmueller39",
            "qbarker56",
            "gsoto24",
            "sosaj99",
            "rosalesr82",
            "morsee87",
            "apacheco97",
            "pachecoa97",
            "bbridges15",
            "poncem84",
            "jayda.short",
            "frazierj69",
            "mcdonaldt51",
            "housem95",
            "harrison.hardy",
            "jazlynn.frazier",
            "janelle.francis",
            "francisj35",
            "sotog24",
            "emorse87",
            "ucurry23",
            "nathanial.mueller",
            "ryan.mccall",
            "mponce84",
            "rebecca.garner",
            "mccallr3",
            "cwyatt84",
            "lunaj40",
            "vlester14",
            "dominick.hinton",
            "john.luna",
            "moralesi95",
            "imorales95",
            "trent.mcdonald",
            "ulises.curry",
            "margaret.collins",
            "rchung96",
        };

        public string[] Domains { get; set; } = new[] {
            "hotmail.com",
            "microsoft.com",
            "google.com",
            "gmail.com",
            "yahoo.com",
            "nasa.gov",
            "outlook.com",
            "icloud.com",
        };

        public object? GenerateValue(IProcedualGenerationContext context) =>
            $"{context.ChooseFrom(Users)}@{context.ChooseFrom(Domains)}";
    }
}
