namespace OoBDev.Generations
{
    public interface IGenerateObject : IHavePriority
    {
        object? GenerateValue(IProcedualGenerationContext context);
        bool CanGenerateValue(IProcedualGenerationContext context);
    }
}
