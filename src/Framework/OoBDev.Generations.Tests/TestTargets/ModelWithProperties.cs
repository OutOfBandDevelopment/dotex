namespace OoBDev.Generations.Tests.TestTargets
{
    public class ModelWithProperties
    {
#pragma warning disable CS8618
        public string String { get; set; }
        [SpecialGeneration]
        public string String2 { get; set; }

        public EnumValues EnumValues { get; set; }

        public int[] Integers { get; set; }
#pragma warning restore CS8618
    }
}
